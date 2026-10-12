# BSW.DevSpark - (c) 2026 Baylor Scott & White Health. Source: https://bsw-devspark.bswhive.com
"""Deterministic PR risk digest from changed paths.

Usage: python pr-risk.py [--repo-root DIR] [--range BASE...HEAD] [--lines-added N] [--lines-deleted N] PATH [PATH ...]

Prints one JSON object: per-file tier, delta_class and line counts, risk flags, an auditable
risk_score/rating, a report_form (short/standard/full) and pre-rendered markdown tables.
Tiers are a floor: the reviewer may escalate, never lower.
With --range, per-file line counts and change type come from git; without it they are omitted.
When `.knowledge/index.json` exists, default-tier files owned by a domain or integration entity are
promoted to P0 and unclaimed production files add a small score factor.
Repos may prepend patterns via `.devspark.work/devspark.json` -> `pr_risk.tier_patterns`
({"P0": [regex, ...], ...}).
"""

from __future__ import annotations

import argparse
import fnmatch
import json
import re
import subprocess
import sys
from pathlib import Path

TIER_ORDER = ["P0", "P1", "P2", "P3"]

# First match wins; evaluated top to bottom.
DEFAULT_TIER_PATTERNS: list[tuple[str, str]] = [
    ("P0", r"(^|/)(auth|authn|authz|oauth|security|crypto|middleware|payments?|billing|secrets?)(/|\.|_|-|$)"),
    ("P0", r"(^|/)(routes?|controllers?|handlers?|endpoints?)(/|\.|_|-|$)"),
    ("P0", r"(^|/)(migrations?|db|database)(/|$)|\.sql$"),
    ("P0", r"(^|/)(dbcontext|repository|repositories)[^/]*$"),
    ("P0", r"(^|/)(infra|infrastructure|terraform|bicep)(/|$)|\.(bicep|tf)$"),
    ("P3", r"\.(md|mdx|txt|rst)$|(^|/)(docs?|\.knowledge|\.archive)(/|$)|(^|/)(license|changelog)"),
    ("P2", r"(^|/)(tests?|__tests__|spec)(/|$)|(^|/)test_[^/]*$|[._-](test|spec)\.[a-z]+$|\.tests?(/|\.)"),
    ("P2", r"\.(json|ya?ml|toml|ini|cfg|csproj|props|targets|config)$|(^|/)(\.github|\.vscode|build|scripts)(/|$)|dockerfile|(^|/)\.[^/]+$"),
    ("P1", r".*"),
]

DELTA_PATTERNS: list[tuple[str, str]] = [
    ("work-product", r"^\.devspark\.work/"),
    ("knowledge", r"^\.knowledge/"),
    ("test", r"(^|/)(tests?|__tests__)(/|$)|(^|/)test_[^/]*$|[._-](test|spec)\.[a-z]+$|\.tests?(/|\.)"),
    ("infra", r"(^|/)(infra|infrastructure|terraform|bicep|\.github|\.azuredevops)(/|$)|\.(bicep|tf)$|(^|/)azure-pipelines[^/]*\.ya?ml$|dockerfile"),
    ("docs", r"\.(md|mdx|txt|rst)$|(^|/)docs?(/|$)"),
    ("generated", r"(^|/)(dist|_site|node_modules|bin|obj)(/|$)|\.min\.(js|css)$|\.lock$|package-lock\.json$"),
]

FLAG_PATTERNS: dict[str, str] = {
    "schema_or_migration": r"(^|/)(migrations?)(/|$)|\.sql$|schema[^/]*\.(json|sql|prisma)$",
    "dependency_manifest": r"(^|/)(package(-lock)?\.json|pyproject\.toml|requirements[^/]*\.txt|poetry\.lock|[^/]+\.csproj|packages\.config|go\.(mod|sum)|pom\.xml|Gemfile(\.lock)?|Directory\.Packages\.props)$",
    "ci_or_pipeline_config": r"(^|/)(\.github/workflows|\.azuredevops)(/|$)|(^|/)azure-pipelines[^/]*\.ya?ml$|(^|/)dockerfile",
    "auth_or_security_path": r"(^|/)(auth|authn|authz|oauth|security|crypto|secrets?)(/|\.|_|-|$)",
}

SCORE_WEIGHTS = {
    "p0_files": 3,  # per P0 file, capped
    "p0_cap": 9,
    "schema_or_migration": 3,
    "dependency_manifest": 1,
    "ci_or_pipeline_config": 2,
    "auth_or_security_path": 3,
    "production_without_tests": 3,
    "large_diff": 2,
    "unmapped_production": 1,
}
LARGE_DIFF_LINES = 2000
LARGE_DIFF_NON_DOC_FILES = 25
MEDIUM_AT = 3
HIGH_AT = 7
REPORT_FORM = {"low": "short", "medium": "standard", "high": "full"}
# Domain rules originate outside the code and integrations cross a trust boundary.
CRITICAL_ENTITY_TYPES = {"domain", "integration"}
TABLE_ROW_CAP = 40
CHANGE_TYPES = {"A": "Added", "M": "Modified", "D": "Deleted"}


def git_stats(repo_root: Path, diff_range: str) -> dict[str, dict]:
    """Per-path added/deleted/change from git; empty when git or the range is unavailable."""

    def run(flag: str) -> list[str]:
        try:
            out = subprocess.run(
                ["git", "diff", flag, "--no-renames", diff_range],
                cwd=repo_root,
                capture_output=True,
                text=True,
                timeout=30,
                check=True,
            ).stdout
        except (OSError, subprocess.SubprocessError):
            return []
        return out.splitlines()

    stats: dict[str, dict] = {}
    for line in run("--numstat"):
        parts = line.split("\t", 2)
        if len(parts) == 3:
            add, dele, path = parts
            stats[path] = {
                "added": int(add) if add.isdigit() else 0,
                "deleted": int(dele) if dele.isdigit() else 0,
            }
    for line in run("--name-status"):
        status, _, path = line.partition("\t")
        if path in stats:
            stats[path]["change"] = CHANGE_TYPES.get(status[:1], "Modified")
    return stats


def _changes(f: dict) -> str:
    return f"+{f['added']} -{f['deleted']}" if "added" in f else "-"


def render_tables(files: list[dict]) -> dict[str, str]:
    ordered = sorted(files, key=lambda f: (TIER_ORDER.index(f["tier"]), f["path"]))
    shown = ordered[:TABLE_ROW_CAP]
    rows = ["| File | Tier | Changes | Type |", "|------|------|---------|------|"]
    rows += [f"| {f['path']} | {f['tier']} | {_changes(f)} | {f.get('change', '-')} |" for f in shown]
    if len(ordered) > len(shown):
        rows.append(f"| ... {len(ordered) - len(shown)} more P2/P3 files not listed | | | |")

    tests = [f for f in ordered if f["delta_class"] == "test"]
    if tests:
        trows = ["| File | Changes | Type |", "|------|---------|------|"]
        trows += [f"| {f['path']} | {_changes(f)} | {f.get('change', '-')} |" for f in tests[:TABLE_ROW_CAP]]
        tests_table = "\n".join(trows)
    else:
        tests_table = "No test files changed in this PR."
    return {"changed_files": "\n".join(rows), "test_files": tests_table}


def _load_overrides(repo_root: Path) -> list[tuple[str, str]]:
    cfg = repo_root / ".devspark.work" / "devspark.json"
    if not cfg.is_file():
        return []
    try:
        data = json.loads(cfg.read_text(encoding="utf-8"))
    except (OSError, ValueError):
        return []
    raw = (data.get("pr_risk") or {}).get("tier_patterns") or {}
    out: list[tuple[str, str]] = []
    for tier in TIER_ORDER:
        for pattern in raw.get(tier, []) if isinstance(raw, dict) else []:
            try:
                re.compile(pattern)
            except re.error:
                continue
            out.append((tier, pattern))
    return out


def _first(patterns: list[tuple[str, str]], path: str, default: str) -> str:
    for label, pattern in patterns:
        if re.search(pattern, path, re.IGNORECASE):
            return label
    return default


def classify(path: str, tier_patterns: list[tuple[str, str]]) -> dict[str, str]:
    norm = path.replace("\\", "/")
    return {
        "path": path,
        "tier": _first(tier_patterns, norm, "P1"),
        "delta_class": _first(DELTA_PATTERNS, norm, "production"),
    }


def load_knowledge(repo_root: Path) -> dict | None:
    """Claims and entity types from a prebuilt `.knowledge/index.json`; None when absent or unreadable."""
    index_path = repo_root / ".knowledge" / "index.json"
    if not index_path.is_file():
        return None
    try:
        index = json.loads(index_path.read_text(encoding="utf-8"))
    except (OSError, ValueError):
        return None
    claims: list[tuple[str, str | None]] = []
    for node in index.get("nodes", []):
        for key in ("appliesTo", "source_of_truth"):
            for claim in node.get(key) or []:
                pattern = claim.get("path") if isinstance(claim, dict) else claim
                if isinstance(pattern, str) and pattern:
                    claims.append((pattern, node.get("entity")))
    types = {e.get("id"): e.get("type") for e in index.get("entities", [])}
    return {"claims": claims, "entity_types": types}


def _claim_matches(pattern: str, path: str) -> bool:
    return fnmatch.fnmatchcase(path, pattern) or path.startswith(pattern.rstrip("/") + "/")


def apply_knowledge(files: list[dict], knowledge: dict) -> None:
    """Promote default-tier files owned by a domain or integration entity to P0; flag unclaimed files."""
    for f in files:
        norm = f["path"].replace("\\", "/")
        owners = {entity for pattern, entity in knowledge["claims"] if _claim_matches(pattern, norm)}
        f["claimed"] = bool(owners)
        critical = sorted(
            entity for entity in owners if entity and knowledge["entity_types"].get(entity) in CRITICAL_ENTITY_TYPES
        )
        if critical and f["tier"] == "P1":
            f["tier"] = "P0"
            f["promoted_by"] = critical[0]


def build_digest(
    paths: list[str],
    overrides: list[tuple[str, str]] | None = None,
    lines_added: int = 0,
    lines_deleted: int = 0,
    stats: dict[str, dict] | None = None,
    knowledge: dict | None = None,
) -> dict:
    tier_patterns = (overrides or []) + DEFAULT_TIER_PATTERNS
    files = [classify(p, tier_patterns) for p in paths]
    for f in files:
        f.update((stats or {}).get(f["path"], {}))
    if knowledge:
        apply_knowledge(files, knowledge)

    tier_counts = {t: 0 for t in TIER_ORDER}
    for f in files:
        tier_counts[f["tier"]] += 1

    flags = {
        name: any(re.search(pat, p.replace("\\", "/"), re.IGNORECASE) for p in paths)
        for name, pat in FLAG_PATTERNS.items()
    }
    production = [f for f in files if f["delta_class"] == "production"]
    tests = [f for f in files if f["delta_class"] == "test"]
    flags["production_without_tests"] = bool(production) and not tests
    non_doc_files = len(files) - tier_counts["P3"]
    flags["large_diff"] = (lines_added + lines_deleted) >= LARGE_DIFF_LINES or non_doc_files >= LARGE_DIFF_NON_DOC_FILES
    flags["unmapped_production"] = bool(knowledge) and any(
        not f["claimed"] and f["tier"] != "P3" for f in production
    )

    factors: list[dict] = []
    score = 0
    if tier_counts["P0"]:
        pts = min(tier_counts["P0"] * SCORE_WEIGHTS["p0_files"], SCORE_WEIGHTS["p0_cap"])
        factor = {"factor": "p0_files", "count": tier_counts["P0"], "points": pts}
        promoted = sum(1 for f in files if "promoted_by" in f)
        if promoted:
            factor["from_knowledge"] = promoted
        factors.append(factor)
        score += pts
    for name in (
        "schema_or_migration",
        "dependency_manifest",
        "ci_or_pipeline_config",
        "auth_or_security_path",
        "production_without_tests",
        "large_diff",
        "unmapped_production",
    ):
        if flags[name]:
            factors.append({"factor": name, "points": SCORE_WEIGHTS[name]})
            score += SCORE_WEIGHTS[name]

    rating = "high" if score >= HIGH_AT else "medium" if score >= MEDIUM_AT else "low"
    return {
        "files": files,
        "tier_counts": tier_counts,
        "flags": flags,
        "risk_score": score,
        "rating": rating,
        "report_form": REPORT_FORM[rating],
        "factors": factors,
        "tables": render_tables(files),
    }


def main(argv: list[str] | None = None) -> int:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--repo-root", default=".")
    parser.add_argument("--lines-added", type=int, default=0)
    parser.add_argument("--lines-deleted", type=int, default=0)
    parser.add_argument("--range", dest="diff_range", default="")
    parser.add_argument("paths", nargs="*")
    args = parser.parse_args(argv)

    repo_root = Path(args.repo_root)
    digest = build_digest(
        list(args.paths),
        _load_overrides(repo_root),
        args.lines_added,
        args.lines_deleted,
        git_stats(repo_root, args.diff_range) if args.diff_range else None,
        load_knowledge(repo_root),
    )
    json.dump(digest, sys.stdout, separators=(",", ":"))
    sys.stdout.write("\n")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
