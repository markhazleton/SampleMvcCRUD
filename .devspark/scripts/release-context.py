#!/usr/bin/env python3
# BSW.DevSpark - (c) 2026 Baylor Scott & White Health. Source: https://bsw-devspark.bswhive.com
"""Collect release context: the durable Git delta, plus .devspark.work retention/archive candidates."""

from __future__ import annotations

import argparse
import json
import re
import subprocess
import sys
import tomllib
from pathlib import Path

PLANNING_REFERENCE = re.compile(
    r"(?i)(?:\bspec[ -]?\d+\b|\bFR-\d+\b|\bT\d{3,}\b|\.devspark\.work/specs/|TODO\(spec|\bphase\s+\d+\b)"
)
HISTORY_NAME = re.compile(r"(?i)(?:^|[-_])(adr|decision|rationale|history|archive|superseded)(?:[-_.]|$)")
TEXT_EXTENSIONS = {".md", ".txt", ".py", ".ps1", ".sh", ".js", ".jsx", ".ts", ".tsx", ".cs", ".go", ".rs", ".json", ".yaml", ".yml", ".toml"}
STRAY_RELEASE_ARTIFACT_NAME = re.compile(r"(?i)^release-(?:context|history)-.*\.json$")
STATUS_COMPLETE = re.compile(r"(?im)^[^\S\n]*(?:[-*]\s*)?(?:\*\*)?status(?:\*\*)?[^\S\n]*:[^\S\n]*(?:\*\*)?[^\S\n]*complete\b")
LINKAGE_FIELD = re.compile(r"(?i)\b(code_ref|knowledge_ref)[^\S\n]*:[^\S\n]*([^|\n\r]*)")
# An explained `n/a` counts as populated per the release contract; only an absent or placeholder
# value is treated as unpopulated linkage.
UNPOPULATED_LINKAGE = {"", "pending", "tbd", "todo", "-", "none"}


def git(repo: Path, *args: str) -> str:
    result = subprocess.run(
        ["git", *args], cwd=repo, check=False, capture_output=True, text=True, encoding="utf-8"
    )
    return result.stdout.strip() if result.returncode == 0 else ""


def repository_root() -> Path:
    root = git(Path.cwd(), "rev-parse", "--show-toplevel")
    return Path(root) if root else Path.cwd().resolve()


def relative(repo: Path, path: Path) -> str:
    return path.relative_to(repo).as_posix()


def stable_path_sort_key(path: Path) -> str:
    """OS-independent, case-sensitive sort key -- see build_knowledge_index.py's canonical
    definition. Sorting `Path` objects directly is platform-flavor-dependent and would change
    this bundle's concatenation order (and therefore its content) depending on host OS.
    """
    return path.as_posix()


def source_version(repo: Path) -> str:
    pyproject = repo / "pyproject.toml"
    if not pyproject.is_file():
        return ""
    with pyproject.open("rb") as stream:
        return str(tomllib.load(stream).get("project", {}).get("version", ""))


def changed_files(repo: Path, base: str, head: str) -> list[str]:
    if not base:
        return []
    output = git(repo, "diff", "--name-only", f"{base}..{head}")
    return sorted(path for path in output.splitlines() if path)


def bounded_files(root: Path, limit: int) -> list[Path]:
    if not root.is_dir():
        return []
    return sorted((path for path in root.rglob("*") if path.is_file()), key=lambda path: path.as_posix())[:limit]


def bounded_dirs(root: Path, limit: int) -> list[Path]:
    if not root.is_dir():
        return []
    return sorted((path for path in root.iterdir() if path.is_dir()), key=lambda path: path.as_posix())[:limit]


def stray_release_artifacts(repo: Path, limit: int) -> list[str]:
    """Find leftover release-context/-history JSON dumps from a prior, interrupted release run
    that never reached the version bump/tag step — these are diagnostic scratch output, never
    meant to be committed, and should not be mistaken for evidence that prior release work exists.
    Scoped to Git-tracked files only (fast `git ls-files`, no repo-wide filesystem walk).
    """
    tracked = git(repo, "ls-files").splitlines()
    matches = [
        path
        for path in tracked
        if STRAY_RELEASE_ARTIFACT_NAME.match(Path(path).name)
        and ".devspark.work/releases/" not in path
    ]
    return sorted(matches)[:limit]


def bundle_text(path: Path) -> str:
    """Concatenate a bundle's markdown, whether it is a directory or a single record file."""
    if path.is_file():
        sources = [path]
    else:
        sources = sorted((p for p in path.rglob("*.md") if p.is_file()), key=stable_path_sort_key)
    chunks: list[str] = []
    for source in sources:
        try:
            chunks.append(source.read_text(encoding="utf-8"))
        except (OSError, UnicodeDecodeError):
            continue
    return "\n".join(chunks)


def unpopulated_linkage(text: str) -> list[str]:
    return sorted(
        {
            field.lower()
            for field, value in LINKAGE_FIELD.findall(text)
            if value.strip().strip("*`").lower() in UNPOPULATED_LINKAGE
        }
    )


def branch_exists(repo: Path, name: str) -> bool:
    return bool(git(repo, "branch", "--all", "--list", f"*{name}*").strip())


def classify_bundle(repo: Path, path: Path) -> dict[str, object]:
    """Decide archive eligibility mechanically: a bundle is eligible only when it is marked
    Complete and no code_ref/knowledge_ref is still unpopulated."""
    text = bundle_text(path)
    complete = bool(STATUS_COMPLETE.search(text))
    pending = unpopulated_linkage(text)
    blockers: list[str] = []
    if not complete:
        blockers.append("not marked Complete")
    if pending:
        blockers.append(f"unpopulated linkage: {', '.join(pending)}")
    name = path.stem if path.is_file() else path.name
    return {
        "path": relative(repo, path),
        "complete": complete,
        "archive_eligible": not blockers,
        "blockers": blockers,
        "orphan_candidate": not complete and not branch_exists(repo, name),
    }


def knowledge_index_current(repo: Path, knowledge_files: list[Path], index: Path) -> bool:
    """Prefer the index builder's own staleness check; fall back to modification times only when
    the builder is unavailable, since timestamps cannot see a semantically stale index."""
    for candidate in (
        repo / ".devspark" / "scripts" / "build_knowledge_index.py",
        repo / "scripts" / "build_knowledge_index.py",
    ):
        if not candidate.is_file():
            continue
        try:
            result = subprocess.run(
                [sys.executable, str(candidate), "--repo-root", str(repo), "--check"],
                cwd=repo,
                capture_output=True,
                text=True,
                timeout=60,
                check=False,
            )
        except (OSError, subprocess.SubprocessError):
            break
        if result.returncode in (0, 1):
            return result.returncode == 0
        break
    newest_source = max((path.stat().st_mtime for path in knowledge_files if path != index), default=0.0)
    return index.is_file() and index.stat().st_mtime >= newest_source


def planning_references(repo: Path, limit: int) -> list[dict[str, object]]:
    findings: list[dict[str, object]] = []
    roots = [repo / "src", repo / "tests", repo / ".knowledge"]
    for root in roots:
        for path in bounded_files(root, limit):
            if path.suffix.lower() not in TEXT_EXTENSIONS:
                continue
            try:
                lines = path.read_text(encoding="utf-8").splitlines()
            except (OSError, UnicodeDecodeError):
                continue
            for number, line in enumerate(lines, start=1):
                if PLANNING_REFERENCE.search(line):
                    findings.append({"file": relative(repo, path), "line": number, "text": line.strip()[:240]})
                    if len(findings) >= limit:
                        return findings
    return findings


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("--json", action="store_true")
    parser.add_argument("--dry-run", action="store_true")
    parser.add_argument("--from", dest="from_ref", default="")
    parser.add_argument("--to", dest="to_ref", default="HEAD")
    parser.add_argument("--scope", default="full")
    parser.add_argument("--sample-limit", type=int, default=100)
    args, _ = parser.parse_known_args()

    repo = repository_root()
    limit = max(1, args.sample_limit)
    last_tag = git(repo, "describe", "--tags", "--abbrev=0")
    base_ref = args.from_ref or last_tag
    head_ref = args.to_ref
    files = changed_files(repo, base_ref, head_ref)
    commits = git(repo, "rev-list", f"{base_ref}..{head_ref}").splitlines() if base_ref else []

    work_root = repo / ".devspark.work"
    knowledge_root = repo / ".knowledge"
    specs_root = work_root / "specs"
    quickfixes_root = work_root / "quickfixes"
    knowledge_files = bounded_files(knowledge_root, limit)
    index = knowledge_root / "index.json"
    index_current = knowledge_index_current(repo, knowledge_files, index)
    bundles = [
        classify_bundle(repo, path)
        for path in [*bounded_dirs(specs_root, limit), *bounded_files(quickfixes_root, limit)]
    ]
    retained_work_products = [
        relative(repo, path)
        for path in bounded_files(work_root, limit)
        if not relative(repo, path).startswith((".devspark.work/specs/", ".devspark.work/quickfixes/"))
    ]

    payload = {
        "REPO_ROOT": str(repo),
        "SCOPE": args.scope,
        "WORK_ROOT": str(work_root),
        "KNOWLEDGE_ROOT": str(knowledge_root),
        "CHANGELOG_PATH": str(repo / "CHANGELOG.md"),
        "VERSION_FILE": str(repo / ".devspark" / "BSW.DevSpark.version"),
        "CURRENT_VERSION": source_version(repo),
        "LAST_TAG": last_tag,
        "RELEASE_FROM": base_ref,
        "RELEASE_TO": head_ref,
        "BASE_SHA": git(repo, "rev-parse", base_ref) if base_ref else "",
        "HEAD_SHA": git(repo, "rev-parse", head_ref),
        "COMMITS": commits,
        "COMMITS_SINCE": len(commits),
        "CHANGED_FILES": files,
        "SOURCE_CHANGED": [path for path in files if path.startswith("src/")],
        "TESTS_CHANGED": [path for path in files if path.startswith("tests/")],
        "KNOWLEDGE_CHANGED": [path for path in files if path.startswith(".knowledge/")],
        "PLANNING_LEAKS": [
            path
            for path in files
            if path.startswith((".devspark.work/specs/", ".devspark.work/quickfixes/"))
        ],
        "SPEC_BUNDLES": [relative(repo, path) for path in bounded_dirs(specs_root, limit)],
        "QUICKFIX_RECORDS": [relative(repo, path) for path in bounded_files(quickfixes_root, limit)],
        "ARCHIVE_ELIGIBLE": [bundle["path"] for bundle in bundles if bundle["archive_eligible"]],
        "ARCHIVE_BLOCKED": [bundle for bundle in bundles if not bundle["archive_eligible"]],
        "ORPHAN_CANDIDATES": [bundle["path"] for bundle in bundles if bundle["orphan_candidate"]],
        "RETAINED_WORK_PRODUCTS": retained_work_products,
        "KNOWLEDGE_FILES": [relative(repo, path) for path in knowledge_files],
        "KNOWLEDGE_HISTORY_NODES": [
            relative(repo, path)
            for path in knowledge_files
            if HISTORY_NAME.search(path.name) and path.name.lower() not in {"known-limitations.md"}
        ],
        "KNOWLEDGE_INDEX_PRESENT": index.is_file(),
        "KNOWLEDGE_INDEX_CURRENT": index_current,
        "PLANNING_REFERENCES": planning_references(repo, limit),
        "STRAY_RELEASE_ARTIFACTS": stray_release_artifacts(repo, limit),
        "DRY_RUN": args.dry_run,
    }

    if args.json:
        print(json.dumps(payload, separators=(",", ":")))
    else:
        print(f"Release window: {base_ref or '(initial)'} -> {head_ref}")
        print(f"Commits: {len(commits)}")
        print(f"Changed files: {len(files)}")
        print(f"Planning leaks: {len(payload['PLANNING_LEAKS'])}")
        print(f"Spec bundles to review for archive-eligibility: {len(payload['SPEC_BUNDLES'])}")
        print(f"Quickfix records to review for archive-eligibility: {len(payload['QUICKFIX_RECORDS'])}")
        print(f"Archive-eligible bundles: {len(payload['ARCHIVE_ELIGIBLE'])}")
        print(f"Blocked from archiving: {len(payload['ARCHIVE_BLOCKED'])}")
        print(f"Orphan candidates: {len(payload['ORPHAN_CANDIDATES'])}")
        print(f"Stray release-context/-history artifacts: {len(payload['STRAY_RELEASE_ARTIFACTS'])}")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
