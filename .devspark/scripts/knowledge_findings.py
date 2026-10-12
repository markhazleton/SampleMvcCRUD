#!/usr/bin/env python3
# BSW.DevSpark - (c) 2026 Baylor Scott & White Health. Source: https://bsw-devspark.bswhive.com
"""Shared finding schema for `.knowledge` diagnostics (7.6 Knowledge Discovery & Integrity).

One repository should have one interpretation of current knowledge. Every command that reports a
`.knowledge` diagnostic -- `scripts/knowledge-integrity.py`, `scripts/discover-knowledge.py`, and
any future consumer -- emits findings built with `make_finding()` so severity, confidence, and
evidence stay comparable across commands, across runs, and across releases. See
`.knowledge/governance/devspark-philosophy.md` for the canonical explanation of severity vs.
confidence and of `changes_authoritative_truth`.
"""

from __future__ import annotations

import hashlib
from typing import Any

CATEGORIES = {
    "engine-divergence",
    "generated-artifact-drift",
    "taxonomy-mismatch",
    "missing-mapping",
    "broad-mapping",
    "overlapping-ownership",
    "missing-relationship",
    "governance-relationship-candidate",
    "alias-candidate",
    "contradiction",
    "historical-leakage",
    "knowledge-gap",
    "entity-candidate",
    "dangling-reference",
    "unreachable-knowledge-root",
    "schema-tooling-contradiction",
}
# Severity reflects impact, never how easy a finding was to detect.
SEVERITIES = {"high", "medium", "low", "informational"}
CONFIDENCES = {"high", "medium", "low"}
REQUIRED_EVIDENCE_KEYS = {"type"}


def finding_id(category: str, *id_parts: str) -> str:
    """Deterministic id: same logical problem across repeated scans -> same id.

    Format: `KNOW-<CATEGORY>-<short-hash>`, where the hash is derived only from the caller-supplied
    identity parts (never from a timestamp or scan-order index), so re-running discovery against
    an unchanged repository reproduces the same id every time.
    """
    slug = category.upper().replace("-", "_")
    digest = hashlib.sha256("|".join(id_parts).encode("utf-8")).hexdigest()[:12]
    return f"KNOW-{slug}-{digest}"


def make_finding(
    *,
    category: str,
    severity: str,
    confidence: str,
    subject: str,
    summary: str,
    evidence: list[dict[str, Any]],
    recommendation: str,
    changes_authoritative_truth: bool,
    source: str,
    id_parts: tuple[str, ...] | None = None,
) -> dict[str, Any]:
    """Build one finding conforming to the shared 7.6 schema.

    `source` names the DevSpark command/script that produced the finding (e.g.
    `knowledge-integrity`, `discover-knowledge`), so findings merged from multiple producers stay
    attributable. `id_parts` defaults to `(category, subject)`, which is sufficient for most
    findings; pass explicit parts when `subject` alone would collide across genuinely distinct
    problems (e.g. two relationship candidates naming the same pair of entities in swapped order).
    """
    if category not in CATEGORIES:
        raise ValueError(f"unknown finding category '{category}'")
    if severity not in SEVERITIES:
        raise ValueError(f"unknown finding severity '{severity}'")
    if confidence not in CONFIDENCES:
        raise ValueError(f"unknown finding confidence '{confidence}'")
    for item in evidence:
        if not REQUIRED_EVIDENCE_KEYS.issubset(item):
            raise ValueError(f"evidence entry missing required key(s) {REQUIRED_EVIDENCE_KEYS}: {item}")

    parts = id_parts or (category, subject)
    return {
        "finding_id": finding_id(category, *parts),
        "category": category,
        "severity": severity,
        "confidence": confidence,
        "subject": subject,
        "summary": summary,
        "evidence": evidence,
        "recommendation": recommendation,
        "changes_authoritative_truth": changes_authoritative_truth,
        "source": source,
    }


def validate_finding(finding: dict[str, Any]) -> list[str]:
    """Return a list of schema violations (empty means the finding conforms)."""
    errors: list[str] = []
    required = {
        "finding_id", "category", "severity", "confidence", "subject", "summary",
        "evidence", "recommendation", "changes_authoritative_truth", "source",
    }
    missing = required - finding.keys()
    if missing:
        errors.append(f"missing keys: {sorted(missing)}")
    if finding.get("category") not in CATEGORIES:
        errors.append(f"invalid category: {finding.get('category')!r}")
    if finding.get("severity") not in SEVERITIES:
        errors.append(f"invalid severity: {finding.get('severity')!r}")
    if finding.get("confidence") not in CONFIDENCES:
        errors.append(f"invalid confidence: {finding.get('confidence')!r}")
    if not isinstance(finding.get("evidence"), list):
        errors.append("evidence must be a list")
    else:
        for item in finding["evidence"]:
            if not isinstance(item, dict) or not REQUIRED_EVIDENCE_KEYS.issubset(item):
                errors.append(f"evidence entry missing required key(s): {item!r}")
    if not isinstance(finding.get("changes_authoritative_truth"), bool):
        errors.append("changes_authoritative_truth must be a bool")
    return errors


# Hard structural failures block CI; everything else is advisory (never a mandatory build gate).
HARD_FAILURE_CATEGORIES = {
    "engine-divergence",
    "generated-artifact-drift",
    "dangling-reference",
    "unreachable-knowledge-root",
    "missing-mapping",
}


def is_hard_failure(finding: dict[str, Any]) -> bool:
    return finding.get("category") in HARD_FAILURE_CATEGORIES and finding.get("severity") == "high"
