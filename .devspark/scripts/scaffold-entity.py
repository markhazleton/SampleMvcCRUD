#!/usr/bin/env python3
# BSW.DevSpark - (c) 2026 Baylor Scott & White Health. Source: https://bsw-devspark.bswhive.com
"""Draft a new .knowledge/entities/<id>/ (assisted authoring, human-confirmed).

Given explicit source paths and layer names, drafts `_entity.yaml` plus one candidate layer doc
per requested layer for a NEW entity. This is an authoring aid, not an automated writer: it never
fabricates a behavioral claim, a source_of_truth path, or a relation - only what the caller passes
in and, best-effort, a Python module docstring extracted from the cited source. Defaults to
dry-run; writes only with --write, and refuses to overwrite an existing entity without --force.

Usage:
    python scaffold-entity.py --repo-root . --entity-id widget --type subsystem --owner team-x \
        --source src/widget/service.py --layer architecture --json
    python scaffold-entity.py --repo-root . --entity-id widget --type subsystem --owner team-x \
        --source src/widget/service.py --layer architecture --write
"""

from __future__ import annotations

import argparse
import ast
import json
import re
from datetime import UTC, datetime
from pathlib import Path
from typing import Any

from build_knowledge_index import ALLOWED_LAYERS, ENTITY_TYPES, _is_forbidden_reference

SLUG_RE_MESSAGE = "entity-id must be lowercase alphanumeric with single hyphens (e.g. 'token-service')"
ENTITY_ID_RE = re.compile(r"^[a-z0-9]+(?:-[a-z0-9]+)*$")


def _validate_entity_id(entity_id: str) -> None:
    if not ENTITY_ID_RE.fullmatch(entity_id):
        raise ValueError(SLUG_RE_MESSAGE)


def _extract_python_docstring(repo_root: Path, source: str) -> str | None:
    path = repo_root / source
    if path.suffix != ".py" or not path.is_file():
        return None
    try:
        tree = ast.parse(path.read_text(encoding="utf-8"))
    except (SyntaxError, UnicodeDecodeError):
        return None
    return ast.get_docstring(tree)


def _title_case(entity_id: str, layer: str) -> str:
    return f"{entity_id.replace('-', ' ').title()} {layer}"


def draft_entity_yaml(entity_id: str, entity_type: str, owner: str) -> str:
    return f"id: {entity_id}\ntype: {entity_type}\nowner: {owner}\n"


def draft_layer_doc(
    repo_root: Path, entity_id: str, layer: str, sources: list[str], today: str
) -> str:
    docstring = None
    for source in sources:
        docstring = _extract_python_docstring(repo_root, source)
        if docstring:
            break
    body = (
        docstring.strip()
        if docstring
        else "<!-- TODO: describe current behavior here, grounded in source_of_truth above. -->"
    )
    sources_yaml = "\n".join(f"  - {source}" for source in sources)
    title = _title_case(entity_id, layer)
    return (
        "---\n"
        "type: entity-layer\n"
        f"layer: {layer}\n"
        f'title: "{title}"\n'
        "source_of_truth:\n"
        f"{sources_yaml}\n"
        f"last_verified: {today}\n"
        "---\n\n"
        f"# {title}\n\n"
        "<!-- Draft: scaffold-entity.py generated this from cited sources. A human must review\n"
        "     and confirm accuracy before this is considered current truth. -->\n\n"
        f"{body}\n"
    )


def build_draft(
    repo_root: Path,
    entity_id: str,
    entity_type: str,
    owner: str,
    sources: list[str],
    layers: list[str],
) -> dict[str, Any]:
    _validate_entity_id(entity_id)
    for layer in layers:
        if layer not in ALLOWED_LAYERS:
            raise ValueError(f"layer must be one of {sorted(ALLOWED_LAYERS)}: got '{layer}'")
    for source in sources:
        if _is_forbidden_reference(source):
            raise ValueError(f"source cannot target .devspark.work/ or .archive/: {source}")
        if not (repo_root / source).exists():
            raise ValueError(f"source path does not exist: {source}")

    today = datetime.now(UTC).date().isoformat()
    entity_dir = f".knowledge/entities/{entity_id}"
    files = [
        {"path": f"{entity_dir}/_entity.yaml", "content": draft_entity_yaml(entity_id, entity_type, owner)}
    ]
    for layer in layers:
        files.append(
            {
                "path": f"{entity_dir}/{layer}.md",
                "content": draft_layer_doc(repo_root, entity_id, layer, sources, today),
            }
        )
    return {"entity_id": entity_id, "entity_dir": entity_dir, "files": files}


def write_draft(repo_root: Path, draft: dict[str, Any], force: bool) -> None:
    entity_yaml = repo_root / draft["files"][0]["path"]
    if entity_yaml.exists() and not force:
        raise SystemExit(f"{entity_yaml} already exists; pass --force to overwrite")
    for file_entry in draft["files"]:
        target = repo_root / file_entry["path"]
        target.parent.mkdir(parents=True, exist_ok=True)
        target.write_text(file_entry["content"], encoding="utf-8")


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--repo-root", default=".")
    parser.add_argument("--entity-id", required=True)
    parser.add_argument(
        "--type",
        dest="entity_type",
        required=True,
        choices=sorted(ENTITY_TYPES),
        help="Controlled entity classification. "
        + "; ".join(f"{name}: {desc}" for name, desc in sorted(ENTITY_TYPES.items())),
    )
    parser.add_argument("--owner", required=True)
    parser.add_argument("--source", dest="sources", action="append", required=True)
    parser.add_argument("--layer", dest="layers", action="append", required=True)
    parser.add_argument("--write", action="store_true", help="Write files; omit for dry-run (default)")
    parser.add_argument("--force", action="store_true", help="Overwrite an existing entity's _entity.yaml")
    parser.add_argument("--json", action="store_true", help="Emit the draft as JSON instead of file previews")
    args = parser.parse_args()

    repo_root = Path(args.repo_root).resolve()
    try:
        draft = build_draft(
            repo_root, args.entity_id, args.entity_type, args.owner, args.sources, args.layers
        )
    except ValueError as exc:
        print(f"Error: {exc}", flush=True)
        return 1

    if args.write:
        write_draft(repo_root, draft, args.force)
        print(f"Wrote {len(draft['files'])} file(s) under {draft['entity_dir']}/")
        engine = Path(__file__).resolve().parent / "build_knowledge_index.py"
        print(f"Review each draft, then run {engine} --repo-root . to validate.")
        return 0

    if args.json:
        print(json.dumps(draft, indent=2))
        return 0

    print(f"DRY RUN - no files written. Draft for entity '{draft['entity_id']}':\n")
    for file_entry in draft["files"]:
        print(f"--- {file_entry['path']} ---")
        print(file_entry["content"])
    print("Review the draft above, then re-run with --write to create these files.")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
