# BSW.DevSpark - (c) 2026 Baylor Scott & White Health. Source: https://bsw-devspark.bswhive.com
"""Deterministic consistency checks for a written PR review.

Usage: python pr-review-validate.py REVIEW.md [--repo-root DIR] [--range BASE...HEAD]

Prints {"ok": bool, "issues": [{"code", "message"}]} and exits 1 when any issue is found.
With --range the risk digest is recomputed from git so the rating, report form and pasted
tables can be checked as well. Checks only what can be decided mechanically: frontmatter vs body,
recommendation vs open blocking findings, and risk never lowered.
"""

from __future__ import annotations

import argparse
import importlib.util
import json
import re
import subprocess
import sys
from pathlib import Path

import yaml

RATING_ORDER = {"low": 0, "medium": 1, "high": 2}
REQUIRED_KEYS = (
    "pr",
    "devspark_version",
    "reviewed_commit",
    "revision",
    "findings",
    "max_severity",
    "risk_rating",
    "risk_escalated",
)
ROW = re.compile(r"^\|\s*(?P<id>C|H|M|L|CON|PRD\d)-(?P<n>\d+)\s*\|\s*(?P<status>[^|]*)\|", re.M)
CLOSED = re.compile(r"resolved|withdrawn", re.I)


def _load_risk_engine():
    path = Path(__file__).resolve().parent / "pr-risk.py"
    spec = importlib.util.spec_from_file_location("pr_risk", path)
    module = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(module)
    return module


def split_frontmatter(text: str) -> tuple[dict, str]:
    if not text.startswith("---"):
        return {}, text
    parts = text.split("---", 2)
    if len(parts) < 3:
        return {}, text
    try:
        data = yaml.safe_load(parts[1])
    except yaml.YAMLError:
        return {}, text
    return (data if isinstance(data, dict) else {}), parts[2]


def finding_rows(body: str) -> list[tuple[str, str, str]]:
    """(kind, full id, status) for every finding table row; kind is C/H/M/L/CON/PRD."""
    rows = []
    for m in ROW.finditer(body):
        kind = "PRD" if m["id"].startswith("PRD") else m["id"]
        rows.append((kind, f"{m['id']}-{m['n']}", m["status"].strip()))
    return rows


def recommendation(body: str) -> str | None:
    m = re.search(r"^## Approval Decision\s+\*\*Recommendation\*\*:\s*(.+)$", body, re.M)
    if not m:
        return None
    line = m.group(1).upper()
    for key, label in (("REQUEST CHANGES", "request-changes"), ("REJECT", "reject"), ("APPROVE", "approve")):
        if key in line:
            return label
    return None


def recompute_digest(repo_root: Path, diff_range: str) -> dict | None:
    engine = _load_risk_engine()
    try:
        names = subprocess.run(
            ["git", "diff", "--name-only", "--no-renames", diff_range],
            cwd=repo_root,
            capture_output=True,
            text=True,
            timeout=30,
            check=True,
        ).stdout.split()
    except (OSError, subprocess.SubprocessError):
        return None
    if not names:
        return None
    stats = engine.git_stats(repo_root, diff_range)
    return engine.build_digest(
        names,
        engine._load_overrides(repo_root),
        sum(s.get("added", 0) for s in stats.values()),
        sum(s.get("deleted", 0) for s in stats.values()),
        stats,
        engine.load_knowledge(repo_root),
    )


def validate(text: str, digest: dict | None = None) -> list[dict]:
    issues: list[dict] = []

    def add(code: str, message: str) -> None:
        issues.append({"code": code, "message": message})

    fm, body = split_frontmatter(text)
    missing = [k for k in REQUIRED_KEYS if k not in fm]
    if missing:
        add("frontmatter-missing", f"frontmatter lacks: {', '.join(missing)}")
        return issues

    rows = finding_rows(body)
    counts = fm.get("findings") or {}
    if fm.get("revision") == 1:
        raised = {k: len({fid for kind, fid, _ in rows if kind == label}) for k, label in
                  (("critical", "C"), ("high", "H"), ("medium", "M"), ("low", "L"), ("con", "CON"))}
        for key, value in raised.items():
            if counts.get(key) != value:
                add("findings-count", f"frontmatter findings.{key}={counts.get(key)} but the body has {value}")

    present = [s for s in ("critical", "high", "medium", "low") if counts.get(s)]
    expected_max = present[0] if present else "none"
    if fm.get("max_severity") != expected_max:
        add("max-severity", f"max_severity is {fm.get('max_severity')} but counts imply {expected_max}")

    blocking_open = [
        fid for kind, fid, status in rows if kind in ("C", "H", "PRD") and not CLOSED.search(status)
    ]
    rec = recommendation(body)
    if rec in ("request-changes", "reject") and not blocking_open:
        add("rec-without-blocker", "REQUEST CHANGES/REJECT needs at least one open blocking finding (C, H or PRD)")
    if rec == "approve" and blocking_open:
        add("approve-with-blocker", f"APPROVE while blocking findings are open: {', '.join(blocking_open[:5])}")
    immediate = re.search(r"### Immediate Actions[^\n]*\n(.*?)(?=\n### |\n## )", body, re.S)
    if immediate:
        says_none = "None found" in immediate.group(1)
        if says_none and blocking_open:
            add("immediate-actions", "Immediate Actions says none, but blocking findings are open")
        if not says_none and not blocking_open:
            add("immediate-actions", "Immediate Actions lists items, but no blocking finding is open")

    if digest:
        floor = digest["rating"]
        rating = fm.get("risk_rating")
        if rating not in RATING_ORDER or RATING_ORDER[rating] < RATING_ORDER[floor]:
            add("risk-lowered", f"risk_rating {rating} is below the computed floor {floor}")
        elif (rating != floor) != bool(fm.get("risk_escalated")):
            add("risk-escalation-flag", "risk_escalated must be true exactly when risk_rating exceeds the computed rating")
        if digest["report_form"] == "short" and "## Changed Files" in body:
            add("form-bloat", "short report form must not include the Changed Files section")
        if digest["report_form"] != "short" and digest["tables"]["changed_files"].splitlines()[0] not in body:
            add("table-not-pasted", "the script-generated Changed Files table is missing")
    return issues


def main(argv: list[str] | None = None) -> int:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("review")
    parser.add_argument("--repo-root", default=".")
    parser.add_argument("--range", dest="diff_range", default="")
    args = parser.parse_args(argv)

    text = Path(args.review).read_text(encoding="utf-8")
    digest = recompute_digest(Path(args.repo_root), args.diff_range) if args.diff_range else None
    issues = validate(text, digest)
    json.dump({"ok": not issues, "issues": issues}, sys.stdout, indent=2)
    sys.stdout.write("\n")
    return 1 if issues else 0


if __name__ == "__main__":
    raise SystemExit(main())
