# BSW.DevSpark - (c) 2026 Baylor Scott & White Health. Source: https://bsw-devspark.bswhive.com
"""Calibrate the PR risk digest against recorded PR review outcomes.

Usage: python pr-risk-calibrate.py [--repo-root DIR] [--reviews-dir DIR] [--base REF]
                                   [--merge-subject PATTERN] [--min-recall FLOAT] [--json]

For each review it pairs the predicted risk rating with the highest recorded finding severity,
grouped by the DevSpark version that produced the review. Reviews with frontmatter use their
recorded `risk_rating`; older reviews are re-scored from the PR's changed files (`BASE...commit`,
or the merge commit found by `--merge-subject`, where `{pr}` is the PR number).

Reports, per version: recall (critical/high PRs rated medium or above) and over-rating
(PRs with no or low findings rated high). Exit 1 only when --min-recall is given and missed.
"""

from __future__ import annotations

import argparse
import importlib.util
import json
import re
import subprocess
import sys
from collections import defaultdict
from pathlib import Path

import yaml

RATING_ORDER = {"low": 0, "medium": 1, "high": 2}
SEVERITIES = ("critical", "high", "medium", "low")


def _load_risk_engine():
    path = Path(__file__).resolve().parent / "pr-risk.py"
    spec = importlib.util.spec_from_file_location("pr_risk", path)
    module = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(module)
    return module


def parse_frontmatter(text: str) -> dict:
    if not text.startswith("---"):
        return {}
    parts = text.split("---", 2)
    if len(parts) < 3:
        return {}
    try:
        data = yaml.safe_load(parts[1])
    except yaml.YAMLError:
        return {}
    return data if isinstance(data, dict) else {}


def legacy_counts(text: str) -> dict[str, int] | None:
    """Counts from the first Revision Log row of a review that has no frontmatter."""
    lines = text.splitlines()
    for i, line in enumerate(lines):
        if not (line.startswith("|") and re.search(r"critical", line, re.I) and re.search(r"high", line, re.I)):
            continue
        header = [c.strip().lower() for c in line.strip().strip("|").split("|")]
        for row in lines[i + 2 :]:
            if not row.startswith("|"):
                break
            cells = [c.strip() for c in row.strip().strip("|").split("|")]
            counts = {}
            for sev in SEVERITIES:
                counts[sev] = 0
                for j, name in enumerate(header):
                    if name.startswith(sev) and j < len(cells):
                        m = re.match(r"\d+", cells[j])
                        counts[sev] = int(m.group()) if m else 0
            return counts
    return None


def legacy_commit(text: str) -> str | None:
    m = re.search(r"\*\*Reviewed Commit\*\*:\s*`?([0-9a-f]{7,40})", text, re.I)
    return m.group(1) if m else None


def max_severity(counts: dict[str, int]) -> str:
    for sev in SEVERITIES:
        if counts.get(sev):
            return sev
    return "none"


def _git(repo: Path, *args: str) -> str:
    result = subprocess.run(
        ["git", "-C", str(repo), *args], capture_output=True, text=True, timeout=60, check=False
    )
    return result.stdout if result.returncode == 0 else ""


def changed_files(repo: Path, base: str, commit: str, pr: str, merge_subject: str | None):
    """Return (paths, added, deleted) for a PR, or None when the diff cannot be recovered."""
    spans = [(f"{base}...{commit}",)]
    if merge_subject:
        subject = merge_subject.replace("{pr}", re.escape(pr))
        merged = _git(repo, "log", base, "--format=%H", "-1", f"--grep={subject}").strip()
        if merged:
            spans.append((f"{merged}^1", merged))
    for span in spans:
        paths = [p for p in _git(repo, "diff", "--name-only", *span).splitlines() if p]
        if paths:
            stat = _git(repo, "diff", "--shortstat", *span)
            add = re.search(r"(\d+) insertion", stat)
            dele = re.search(r"(\d+) deletion", stat)
            return paths, int(add.group(1)) if add else 0, int(dele.group(1)) if dele else 0
    return None


def collect(repo: Path, reviews_dir: Path, base: str, merge_subject: str | None) -> list[dict]:
    engine = _load_risk_engine()
    rows = []
    for review in sorted(reviews_dir.glob("pr-*.md")):
        text = review.read_text(encoding="utf-8", errors="replace")
        pr = review.stem.split("-", 1)[1]
        front = parse_frontmatter(text)

        counts = front.get("findings") if isinstance(front.get("findings"), dict) else None
        counts = counts or legacy_counts(text)
        if counts is None:
            rows.append({"pr": pr, "skipped": "no-severity"})
            continue
        severity = front.get("max_severity") or max_severity(counts)

        rating = front.get("risk_rating")
        source = "recorded"
        if rating not in RATING_ORDER:
            commit = front.get("reviewed_commit") or legacy_commit(text)
            diff = changed_files(repo, base, str(commit), pr, merge_subject) if commit else None
            if diff is None:
                rows.append({"pr": pr, "skipped": "no-diff"})
                continue
            paths, added, deleted = diff
            rating = engine.build_digest(paths, engine._load_overrides(repo), added, deleted)["rating"]
            source = "recomputed"

        rows.append(
            {
                "pr": pr,
                "version": str(front.get("devspark_version") or "legacy"),
                "max_severity": severity,
                "rating": rating,
                "source": source,
            }
        )
    return rows


def summarize(rows: list[dict]) -> dict:
    groups: dict[str, list[dict]] = defaultdict(list)
    skipped: dict[str, int] = defaultdict(int)
    for row in rows:
        if "skipped" in row:
            skipped[row["skipped"]] += 1
        else:
            groups[row["version"]].append(row)

    per_version = {}
    for version, items in sorted(groups.items()):
        serious = [r for r in items if r["max_severity"] in ("critical", "high")]
        clean = [r for r in items if r["max_severity"] in ("none", "low")]
        caught = [r for r in serious if RATING_ORDER[r["rating"]] >= RATING_ORDER["medium"]]
        over = [r for r in clean if r["rating"] == "high"]
        per_version[version] = {
            "reviews": len(items),
            "serious": len(serious),
            "recall": round(len(caught) / len(serious), 3) if serious else None,
            "missed": [r["pr"] for r in serious if r not in caught],
            "clean": len(clean),
            "over_rating": round(len(over) / len(clean), 3) if clean else None,
        }
    return {"per_version": per_version, "skipped": dict(skipped)}


def main(argv: list[str] | None = None) -> int:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--repo-root", default=".")
    parser.add_argument("--reviews-dir")
    parser.add_argument("--base", default="origin/main")
    parser.add_argument("--merge-subject", help="regex for merge commit subjects; {pr} is the PR number")
    parser.add_argument("--min-recall", type=float)
    parser.add_argument("--json", action="store_true")
    args = parser.parse_args(argv)

    repo = Path(args.repo_root).resolve()
    reviews_dir = Path(args.reviews_dir) if args.reviews_dir else repo / ".devspark.work" / "pr-review"
    summary = summarize(collect(repo, reviews_dir, args.base, args.merge_subject))

    if args.json:
        json.dump(summary, sys.stdout, indent=2)
        sys.stdout.write("\n")
    else:
        for version, stats in summary["per_version"].items():
            recall = "n/a" if stats["recall"] is None else f"{stats['recall']:.0%}"
            over = "n/a" if stats["over_rating"] is None else f"{stats['over_rating']:.0%}"
            print(
                f"{version}: reviews={stats['reviews']} serious={stats['serious']} recall={recall} "
                f"over-rating={over} missed={stats['missed']}"
            )
        if summary["skipped"]:
            print(f"skipped: {summary['skipped']}")

    if args.min_recall is not None:
        for stats in summary["per_version"].values():
            if stats["recall"] is not None and stats["recall"] < args.min_recall:
                return 1
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
