#!/usr/bin/env python3
# BSW.DevSpark - (c) 2026 Baylor Scott & White Health. Source: https://bsw-devspark.bswhive.com
"""Deterministic helper for Planning Review and the Judge adjudication phase.

This script never discovers anything and never edits a planning artifact. It
reads the child gate artifacts (Analyze, Critic), reports their freshness,
normalizes their findings, validates a ``gates/judge.md`` against the Judge
contract, and derives the Planning Review verdict. Judgment stays in the
prompt; every rule that can be checked mechanically is checked here.

Subcommands (all emit JSON; exit 0 ok, 1 contract failure, 2 usage error):

  resolve       resolve the active feature directory once, for hand-off to children
    semantic-hash hash decision-bearing content of a supported planning artifact
  children      child gate freshness, normalized findings, remediation lineage
  validate      validate gates/judge.md and derive its verdict
  seal          record the child artifacts this run generated or reused; detect a replaced one
  remediation   split judge dispositions into current work / pending / routed / future work
  future-work   materialize preserved future work (a separate step from Judge)
  lifecycle     pre-PR completeness check for the active spec
  review-state  one-shot Planning Review state for `next` (verdict, recheck needed)
  guard         begin/end a read-only guard around the Judge phase
"""

from __future__ import annotations

import argparse
import hashlib
import json
import re
import secrets
import subprocess
import sys
import tempfile
from datetime import datetime, timezone
from fnmatch import fnmatchcase
from pathlib import Path

import yaml

CLASSIFICATIONS = ("blocking-defect", "accepted-limitation", "deferred-work", "learning")
NON_BLOCKING = ("accepted-limitation", "deferred-work", "learning")
RESOLUTIONS = ("require", "simplify", "accept", "defer")
CHANGE_SHAPES = ("documentation", "planning", "test", "code", "operational", "machinery")
MERGE_BASES = ("deterministic", "judgment")
CLASSIFICATION_SOURCES = ("child", "judge-inferred")
CHILD_MODES = ("generated", "reused")
VERDICTS = ("PROCEED", "CONDITIONAL", "STOP")
AUTHORITIES = ("human", "governing-requirement", "deterministic-evidence")
ROUTE_OWNERS = ("verify", "closeout", "pr-review", "implement")
FUTURE_TYPES = ("bug", "enhancement", "architecture", "operational", "research")
CHILD_STATES = (
    "fresh",
    "bookkeeping-changed",
    "stale-nonblocking",
    "stale-recheck-required",
    "superseded",
    "missing",
    "not-applicable",
)

RESOLVED_STATUSES = {"resolved", "closed", "fixed", "superseded"}
SIGNIFICANT_SEVERITIES = {"showstopper", "critical", "high"}
SIGNIFICANT_CATEGORIES = {
    "secrets_handling",
    "trust_boundaries",
    "auth_authz",
    "input_validation",
    "data_loss_continuity",
    "regulatory_privacy",
    "embedded_safety",
    "llm_prompt_integrity",
    "iac_blast_radius",
    "pipeline_semantics",
}
MECHANISM_KEYWORDS = (
    "persist",
    "table",
    "collection",
    "container",
    "migration",
    "schema",
    "endpoint",
    "route",
    "api",
    "state machine",
    "lifecycle",
    "status field",
    "queue",
    "cache",
    "snapshot",
    "config",
    "setting",
    "flag",
    "telemetry",
    "background job",
    "worker",
)

FENCE = re.compile(r"```ya?ml[ \t]*\r?\n(.*?)```", re.S)
FRONTMATTER = re.compile(r"\A---[ \t]*\r?\n(.*?)\r?\n---[ \t]*(?:\r?\n|\Z)", re.S)
REQ_RE = re.compile(r"\b(?:FR|SC|NFR|INV)-\d+\b")
TASK_RE = re.compile(r"\bT\d{2,4}\b")
RESOLVES_RE = re.compile(r"resolves:\s*([^)]+)\)", re.I)
UNCHECKED_RE = re.compile(r"^\s*[-*]\s*\[ \]")
TASK_LINE_RE = re.compile(r"^\s*[-*]\s*\[([ xX])\]")

FUTURE_WORK_SECTIONS = (
    "Problem",
    "Originating Spec",
    "Why Not Current Scope",
    "Evidence Already Discovered",
    "Candidate Direction",
    "Constraints",
    "Open Questions",
    "Activation Trigger",
    "Provenance",
)


# --------------------------------------------------------------------------- io


def _git(args: list[str], cwd: Path, timeout: int = 30) -> subprocess.CompletedProcess:
    return subprocess.run(
        ["git", *args],
        cwd=str(cwd),
        capture_output=True,
        text=True,
        timeout=timeout,
        stdin=subprocess.DEVNULL,
        check=False,
    )


def hash_file(path: Path, repo_root: Path) -> str:
    """Match `git hash-object <path>`, which is what gate artifacts record."""
    try:
        res = _git(["hash-object", "--", str(path)], repo_root)
        if res.returncode == 0 and res.stdout.strip():
            return res.stdout.strip()
    except (OSError, subprocess.SubprocessError):
        pass
    data = path.read_bytes()
    return hashlib.sha1(b"blob %d\0" % len(data) + data).hexdigest()


class _SemanticLoader(yaml.SafeLoader):
    def construct_mapping(self, node, deep=False):
        keys = set()
        for key_node, _ in node.value:
            key = self.construct_object(key_node, deep=deep)
            if not isinstance(key, str) or key in keys:
                raise yaml.constructor.ConstructorError(None, None, "ambiguous mapping key", key_node.start_mark)
            keys.add(key)
        return super().construct_mapping(node, deep=deep)


def _semantic_yaml(text: str, role: str) -> dict:
    data = yaml.load(text, Loader=_SemanticLoader)
    if not isinstance(data, dict):
        raise ValueError("artifact metadata must be a mapping")
    for field in ("devspark_version", "devspark_revision"):
        if field in data:
            if not isinstance(data[field], str):
                raise ValueError("producer metadata must be text")
            data.pop(field)
    if role == "tasks":
        if data.get("plan_semantic_hash") is not None and not isinstance(data["plan_semantic_hash"], str):
            raise ValueError("derived Plan hash must be text")
        data.pop("plan_semantic_hash", None)
    if role == "plan":
        if data.get("spec_semantic_hash") is not None and not isinstance(data["spec_semantic_hash"], str):
            raise ValueError("derived Spec hash must be text")
        data.pop("spec_semantic_hash", None)
    if role in ("analyze", "critic"):
        for field in ("generated", "generated_at"):
            value = data.get(field)
            if value is not None and not isinstance(value, (str, datetime)):
                raise ValueError("generation metadata must be a timestamp")
            data.pop(field, None)
        stamp = data.get("planning_review")
        if isinstance(stamp, dict):
            stamp.pop("run_id", None)
            if not stamp:
                data.pop("planning_review")
        for entry in data.get("reviewed_artifacts") or []:
            if not isinstance(entry, dict):
                raise ValueError("reviewed input must be a mapping")
            if re.fullmatch(r"sem1:[0-9a-f]{64}", str(entry.get("semantic_hash", ""))):
                entry.pop("hash", None)
        for entry in data.get("findings") or []:
            if not isinstance(entry, dict):
                raise ValueError("finding must be a mapping")
            status = str(entry.get("status", "open")).lower()
            if status in RESOLVED_STATUSES | {"open"}:
                entry.pop("status", None)
                if status in RESOLVED_STATUSES or not entry.get("outcome"):
                    entry.pop("outcome", None)
    return data


def semantic_projection(path: Path) -> str | None:
    """Project supported artifact roles; unsafe parsing yields no reusable evidence."""
    role = path.stem if path.name in ("spec.md", "plan.md", "tasks.md", "analyze.md", "critic.md") else None
    if role is None:
        return None
    try:
        text = path.read_text(encoding="utf-8-sig").replace("\r\n", "\n").replace("\r", "\n")
        front = FRONTMATTER.match(text)
        if text.startswith("---\n") and front is None:
            return None
        metadata = _semantic_yaml(front.group(1), role) if front else {}
        body = text[front.end():] if front else text
        if role in ("analyze", "critic"):
            blocks = 1 if front else 0

            def project_block(match):
                nonlocal blocks
                blocks += 1
                return "```yaml\n" + yaml.safe_dump(_semantic_yaml(match.group(1), role), sort_keys=True) + "```"

            body = FENCE.sub(project_block, body)
            if not blocks:
                return None
        kept = []
        fence = None
        notes = False
        metadata_region = True
        status_count = 0
        for line in body.splitlines():
            opening = re.match(r"^ {0,3}(`{3,}|~{3,})(.*)$", line)
            if fence is not None:
                if not notes:
                    kept.append(line)
                if re.fullmatch(r" {0,3}" + re.escape(fence[0]) + "{" + str(len(fence)) + r",}[ \t]*", line):
                    fence = None
                continue
            if opening:
                fence = opening.group(1)
                if not notes:
                    kept.append(line)
                continue
            if re.match(r"^#{1,2} ", line):
                notes = role == "plan" and line == "## Implementation Notes"
            if notes:
                continue
            if line.startswith("##"):
                metadata_region = False
            if role == "spec":
                if metadata_region and re.fullmatch(
                    r"\*\*Status\*\*: (Draft|In Progress|Complete)(?: <!-- Valid: Draft \| In Progress \| Complete -->)?", line
                ):
                    status_count += 1
                    if status_count > 1:
                        return None
                    continue
                line = re.sub(r"^(### User Story \d+ - .+) \u2705 Complete$", r"\1", line)
            elif role == "tasks":
                if re.match(r"^[-*] \[[ xX]\] T\d+\b", line):
                    line = re.sub(r"^([-*] \[)[xX](\])", r"\1 \2", line)
                    line = re.sub(r" <!-- WIP: [^\r\n]* -->$", "", line)
                    line = re.sub(r" \(code_ref: [^|\r\n]*\| knowledge_ref: [^)\r\n]*\)$", "", line)
                if re.fullmatch(r"<!-- WIP: [^\r\n]* -->", line) and kept and re.match(r"^[-*] \[ \] T\d+\b", kept[-1]):
                    continue
                if re.fullmatch(r"\*\*Checkpoint\*\*: Phase complete (?:-|\u2014) \d{4}-\d{2}-\d{2}\.?", line):
                    continue
            kept.append(line)
        if fence is not None:
            return None
        prefix = yaml.safe_dump(metadata, sort_keys=True) if metadata else ""
        return role + "\n" + prefix + "\n".join(kept).rstrip("\n") + "\n"
    except (OSError, UnicodeError, ValueError, TypeError, RecursionError, yaml.YAMLError):
        return None


def semantic_hash(path: Path) -> str | None:
    projection = semantic_projection(path)
    return "sem1:" + hashlib.sha256(projection.encode("utf-8")).hexdigest() if projection is not None else None


def find_repo_root(feature_dir: Path, explicit: str | None) -> Path:
    if explicit:
        return Path(explicit).resolve()
    for parent in feature_dir.resolve().parents:
        if parent.name == ".devspark.work":
            return parent.parent
    return feature_dir.resolve().parents[2]


def load_yaml_blocks(path: Path) -> list[dict]:
    text = path.read_text(encoding="utf-8")
    blocks: list[dict] = []
    front = FRONTMATTER.match(text)
    for chunk in ([front.group(1)] if front else []) + [m.group(1) for m in FENCE.finditer(text)]:
        try:
            data = yaml.safe_load(chunk)
        except yaml.YAMLError:
            continue
        if isinstance(data, dict):
            blocks.append(data)
    if not blocks:
        try:
            data = yaml.safe_load(text)
        except yaml.YAMLError:
            data = None
        if isinstance(data, dict):
            blocks.append(data)
    return blocks


def _finding_id(entry: dict) -> str:
    return str(entry.get("finding_id") or entry.get("id") or "").strip()


def new_run_id() -> str:
    return "pr-" + datetime.now(timezone.utc).strftime("%Y%m%dT%H%M%SZ") + "-" + secrets.token_hex(3)


def gate_run_id(header: dict) -> str:
    """The Planning Review run that stamped a gate artifact, or empty for an unstamped one."""
    stamp = header.get("planning_review")
    return str(stamp.get("run_id") or "").strip() if isinstance(stamp, dict) else ""


def child_run_ids(feature_dir: Path) -> dict[str, str]:
    out: dict[str, str] = {}
    for name in ("analyze", "critic"):
        path = feature_dir / "gates" / f"{name}.md"
        if path.exists():
            out[name] = gate_run_id(load_gate(path)["header"])
    return out


def load_gate(path: Path) -> dict:
    blocks = load_yaml_blocks(path)
    header = next((b for b in blocks if "gate" in b), blocks[0] if blocks else {})
    findings: list[dict] = []
    routed: list[dict] = []
    obligations: list[dict] = []
    seen: set[str] = set()
    reviewed = header.get("reviewed_artifacts") or []
    for block in blocks:
        for entry in block.get("findings") or []:
            if isinstance(entry, dict) and _finding_id(entry) not in seen:
                seen.add(_finding_id(entry))
                findings.append(entry)
        for entry in block.get("routed_findings") or []:
            if isinstance(entry, dict):
                routed.append(entry)
        for entry in block.get("verification_obligations") or []:
            if isinstance(entry, dict):
                obligations.append(entry)
        if not reviewed and block.get("reviewed_artifacts"):
            reviewed = block["reviewed_artifacts"]
    return {
        "header": header,
        "findings": findings,
        "routed": routed,
        "obligations": obligations,
        "reviewed_artifacts": [r for r in reviewed if isinstance(r, dict)],
    }


# ---------------------------------------------------------------- normalization


def _text_of(entry: dict) -> str:
    keys = ("description", "summary", "recommended_action", "location", "evidence")
    return " ".join(str(entry.get(k, "")) for k in keys)


def significance(entry: dict) -> list[str]:
    """Reasons a finding is independently significant (survives any merge)."""
    reasons: list[str] = []
    for key in ("effective_severity", "severity", "base_severity"):
        value = str(entry.get(key) or "").lower()
        if value in SIGNIFICANT_SEVERITIES:
            reasons.append(f"{key}:{value}")
            break
    category = str(entry.get("category") or "").lower()
    if category in SIGNIFICANT_CATEGORIES:
        reasons.append(f"category:{category}")
    if "constitution" in category or "§" in _text_of(entry):
        reasons.append("constitution")
    if entry.get("classification") == "blocking-defect":
        reasons.append("classification:blocking-defect")
    return reasons


def normalize_finding(gate: str, entry: dict) -> dict:
    reasons = significance(entry)
    provided = entry.get("classification")
    if provided in CLASSIFICATIONS:
        classification, source = provided, "child"
    else:
        classification = "blocking-defect" if reasons else "deferred-work"
        source = "judge-inferred"
    status = str(entry.get("status") or "open").lower()
    return {
        "source_gate": gate,
        "source_id": _finding_id(entry),
        "kind": "finding",
        "child_severity": str(entry.get("effective_severity") or entry.get("severity") or "").lower(),
        "base_severity": str(entry.get("base_severity") or "").lower(),
        "category": str(entry.get("category") or ""),
        "owner": str(entry.get("owner") or gate),
        "status": status,
        "open": status not in RESOLVED_STATUSES,
        "classification": classification,
        "classification_source": source,
        "root_cause": str(entry.get("root_cause") or ""),
        "requirements": sorted(set(REQ_RE.findall(_text_of(entry)))),
        "anchor": str(entry.get("location") or ""),
        "evidence": str(entry.get("evidence") or ""),
        "execution_mode": str(entry.get("execution_mode") or ""),
        "summary": str(entry.get("description") or entry.get("summary") or "")[:300],
        "significant": bool(reasons),
        "significance_reasons": reasons,
    }


def normalize_obligation(gate: str, index: int, entry: dict) -> dict:
    return {
        "source_gate": gate,
        "source_id": str(entry.get("id") or f"{gate}-vo-{index:03d}"),
        "kind": "verification-obligation",
        "status": "open",
        "open": True,
        "classification": "deferred-work",
        "classification_source": "judge-inferred",
        "significant": False,
        "significance_reasons": [],
        "summary": str(entry.get("assumption") or "")[:300],
        "proposed_proof": str(entry.get("proposed_proof") or ""),
    }


def normalize_gate(name: str, gate: dict) -> list[dict]:
    records = [normalize_finding(name, f) for f in gate["findings"]]
    records += [normalize_obligation(name, i, o) for i, o in enumerate(gate["obligations"], 1)]
    return records


# ------------------------------------------------------------------- freshness


def _resolve_artifact(entry_path: str, repo_root: Path, feature_dir: Path) -> Path | None:
    for base in (repo_root, feature_dir):
        candidate = base / entry_path
        if candidate.is_file():
            return candidate
    return None


def _role(entry_path: str) -> str:
    name = Path(entry_path).name
    return name if name in ("spec.md", "plan.md", "tasks.md") else "other"


def remediation_lineage(tasks_path: Path) -> list[dict]:
    """Tasks appended by Gate Remediation, with the findings each one resolves."""
    if not tasks_path.exists():
        return []
    lineage: list[dict] = []
    in_section = False
    for line in tasks_path.read_text(encoding="utf-8").splitlines():
        if line.startswith("## "):
            in_section = line.strip().lower().startswith("## gate remediation")
            continue
        if not in_section or not TASK_LINE_RE.match(line):
            continue
        task = TASK_RE.search(line)
        resolves = RESOLVES_RE.search(line)
        ids = [s.strip() for s in resolves.group(1).split(",")] if resolves else []
        lowered = line.lower()
        signals = sorted(k for k in MECHANISM_KEYWORDS if k in lowered)
        lineage.append(
            {
                "task": task.group(0) if task else "",
                "resolves": [i for i in ids if i],
                "mechanism_signals": signals,
            }
        )
    return lineage


def ratchet_candidates(records: list[dict], lineage: list[dict]) -> list[dict]:
    """Advisory only: findings about a remediation, and remediation-on-remediation."""
    by_task = {entry["task"]: entry for entry in lineage if entry["task"]}
    findings = {r["source_id"]: r for r in records if r["kind"] == "finding"}

    def depth(finding_id: str, trail: frozenset[str]) -> int:
        record = findings.get(finding_id)
        if record is None or finding_id in trail:
            return 0
        tasks = TASK_RE.findall(record["anchor"] + " " + record["summary"])
        best = 0
        for task in tasks:
            if task in by_task:
                parents = by_task[task]["resolves"]
                best = max(best, 1 + max((depth(p, trail | {finding_id}) for p in parents), default=0))
        return best

    out = []
    for record in records:
        if record["kind"] != "finding" or not record["open"]:
            continue
        level = depth(record["source_id"], frozenset())
        if level >= 1:
            out.append(
                {
                    "finding": record["source_id"],
                    "chain_depth": level,
                    "advisory": (
                        "remediation-on-remediation: would removing or simplifying the earlier "
                        "remediation resolve the original concern?"
                        if level >= 2
                        else "finding against a remediation task"
                    ),
                }
            )
    return out


def _compatible_child_evidence(path: Path, current_hash: str, declared: dict, header: dict) -> bool:
    if declared.get("hash") == current_hash:
        return True
    recorded = str(declared.get("semantic_hash") or "")
    if header.get("verdict") != "PROCEED" or not re.fullmatch(r"sem1:[0-9a-f]{64}", recorded):
        return False
    if semantic_hash(path) != recorded:
        return False
    covered = {str(source) for concern in header.get("concerns") or [] for source in concern.get("sources") or []}
    records = normalize_gate(path.stem, load_gate(path))
    return not any(record["open"] and record["significant"] and record["source_id"] not in covered for record in records)


def _planned_execution_path(path: Path | None, feature_dir: Path, repo_root: Path) -> bool:
    if path is None:
        return False
    try:
        relative = path.resolve().relative_to(repo_root.resolve()).as_posix()
        parts = relative.lower().split("/")
        if "governance" in parts or path.name.lower() in ("constitution.md", "agents.md", "copilot-instructions.md"):
            return False
        if ".knowledge" in parts:
            front = FRONTMATTER.match(path.read_text(encoding="utf-8-sig"))
            metadata = yaml.load(front.group(1), Loader=_SemanticLoader) if front else {}
            if not isinstance(metadata, dict) or "governance" in str(metadata.get("type", "")).lower():
                return False
        for block in load_yaml_blocks(feature_dir / "plan.md"):
            for entry in block.get("expected_change_surface") or []:
                if not isinstance(entry, dict):
                    continue
                pattern = str(entry.get("path") or "").replace("\\", "/").removeprefix("./")
                if not pattern or ".." in pattern.split("/") or Path(pattern).is_absolute():
                    continue
                if relative == pattern or (pattern.endswith("/") and relative.startswith(pattern)) or fnmatchcase(relative, pattern):
                    return True
    except (OSError, UnicodeError, ValueError, TypeError, yaml.YAMLError):
        return False
    return False


def child_freshness(
    name: str,
    gate_path: Path,
    gate: dict | None,
    records: list[dict],
    ctx: dict,
) -> dict:
    if gate is None:
        return {"state": "missing", "action": "run", "blocking": False, "changed": [], "reasons": []}
    repo_root, feature_dir = ctx["repo_root"], ctx["feature_dir"]
    current_hash = hash_file(gate_path, repo_root)
    declared = ctx["judge_children"].get(name) or {}
    if declared.get("state") == "superseded" and declared.get("hash") == current_hash:
        return {
            "state": "superseded",
            "action": "none",
            "blocking": False,
            "changed": [],
            "reasons": ["declared superseded by the Judge artifact; its open findings are not blockers"],
            "hash": current_hash,
        }

    changed: list[str] = []
    bookkeeping: list[str] = []
    semantic_changed: list[str] = []
    artifacts: list[dict] = []
    for entry in gate["reviewed_artifacts"]:
        rel = str(entry.get("path") or "")
        located = _resolve_artifact(rel, repo_root, feature_dir)
        artifact_state = "FRESH"
        if located is None:
            changed.append(rel)
            artifact_state = "SEMANTICALLY_STALE"
        elif hash_file(located, repo_root) != str(entry.get("hash") or ""):
            recorded = str(entry.get("semantic_hash") or "")
            current = semantic_hash(located) if re.fullmatch(r"sem1:[0-9a-f]{64}", recorded) else None
            if current is not None and current == recorded:
                bookkeeping.append(rel)
                artifact_state = "BOOKKEEPING_CHANGED"
            else:
                changed.append(rel)
                artifact_state = "SEMANTICALLY_STALE"
                if re.fullmatch(r"sem1:[0-9a-f]{64}", recorded):
                    semantic_changed.append(rel)
        artifacts.append({"path": rel, "state": artifact_state})
    planning_evidence = {}
    for entry in gate["reviewed_artifacts"]:
        role = _role(str(entry.get("path") or ""))
        located = _resolve_artifact(str(entry.get("path") or ""), repo_root, feature_dir)
        if role != "other" and located is not None and located.resolve() == (feature_dir / role).resolve():
            recorded = str(entry.get("semantic_hash") or "")
            planning_evidence[role] = bool(re.fullmatch(r"sem1:[0-9a-f]{64}", recorded)) and semantic_hash(located) == recorded
    execution_changes = []
    if set(planning_evidence) == {"spec.md", "plan.md", "tasks.md"} and all(planning_evidence.values()):
        approved = (ctx["implementation_started"] and ctx["judge_header"].get("verdict") == "PROCEED"
                    and _compatible_child_evidence(gate_path, current_hash, declared, ctx["judge_header"]))
        for rel in changed.copy():
            if _role(rel) == "other":
                if approved and _planned_execution_path(_resolve_artifact(rel, repo_root, feature_dir), feature_dir, repo_root):
                    execution_changes.append(rel)
                    changed.remove(rel)
                else:
                    semantic_changed.append(rel)
    if not changed:
        return {
            "state": "stale-nonblocking" if execution_changes else "bookkeeping-changed" if bookkeeping else "fresh",
            "action": "none",
            "blocking": False,
            "changed": sorted(bookkeeping + execution_changes),
            "reasons": (["approved execution changes require Verify and PR Review; source bytes are not unchanged"]
                        if execution_changes else
                        ["recorded semantic evidence is unchanged; only recognized bookkeeping differs"] if bookkeeping else []),
            "execution_changes": execution_changes,
            "artifacts": artifacts,
            "hash": current_hash,
        }

    roles = {_role(p) for p in changed}
    lineage = ctx["lineage"]
    signals = sorted({s for entry in lineage for s in entry["mechanism_signals"]})
    reasons: list[str] = []
    state, action = "stale-nonblocking", "optional-recheck"

    if semantic_changed:
        state = "stale-recheck-required"
        action = "rerun" if "spec.md" in {_role(path) for path in semantic_changed} else "recheck"
        reasons.append("recorded planning semantics changed: " + ", ".join(sorted(semantic_changed)))
    elif ctx["implementation_started"] and roles <= {"tasks.md"}:
        reasons.append("implementation is in progress; task bookkeeping does not invalidate planning evidence")
    elif "spec.md" in roles:
        state, action = "stale-recheck-required", "rerun"
        reasons.append("spec.md changed; the requirement set this gate reviewed no longer matches")
    elif "other" in roles:
        has_open = any(r["open"] and r["significant"] for r in records)
        state = "stale-recheck-required" if has_open else "stale-nonblocking"
        action = "recheck" if has_open else "optional-recheck"
        reasons.append("a reviewed supporting artifact changed")
    else:
        traceable = bool(lineage) and ctx["dispositioned_resolves"].issuperset(
            {i for entry in lineage for i in entry["resolves"]}
        )
        if "plan.md" in roles and not ctx["planning_shaped_only"]:
            state, action = "stale-recheck-required", "recheck"
            reasons.append("plan.md changed beyond planning-shaped remediation")
        elif traceable:
            reasons.append("changes trace to dispositioned findings (bounded remediation)")
        else:
            state, action = "stale-recheck-required", "recheck"
            reasons.append("artifact changed with no traceable disposition")

    if signals and not ctx["implementation_started"]:
        reasons.append("remediation introduces mechanism-bearing work: " + ", ".join(signals))
        if name == "critic":
            state, action = "stale-recheck-required", "recheck"
    return {
        "state": state,
        "action": action,
        "blocking": state == "stale-recheck-required",
        "changed": sorted(changed + bookkeeping),
        "artifacts": artifacts,
        "reasons": reasons,
        "mechanism_signals": signals,
        "hash": current_hash,
    }


def build_children_report(feature_dir: Path, repo_root: Path, run_id: str | None = None) -> dict:
    gates_dir = feature_dir / "gates"
    judge_path = gates_dir / "judge.md"
    judge = load_gate(judge_path) if judge_path.exists() else None
    judge_header = judge["header"] if judge else {}
    judge_children = judge_header.get("children") or {}

    tasks_path = feature_dir / "tasks.md"
    lineage = remediation_lineage(tasks_path)
    implementation_started = tasks_path.exists() and not remediation_only_checked(tasks_path)

    dispositioned: set[str] = set()
    planning_shaped_only = True
    for concern in (judge_header.get("concerns") or []) if judge else []:
        if concern.get("resolution") in ("require", "simplify"):
            dispositioned.update(str(s) for s in concern.get("sources") or [])
            if concern.get("change_shape") not in ("documentation", "planning", "test"):
                planning_shaped_only = False

    ctx = {
        "repo_root": repo_root,
        "feature_dir": feature_dir,
        "judge_children": judge_children,
        "judge_header": judge_header,
        "lineage": lineage,
        "implementation_started": implementation_started,
        "dispositioned_resolves": dispositioned,
        "planning_shaped_only": planning_shaped_only and bool(dispositioned),
    }

    report: dict = {
        "feature_dir": str(feature_dir),
        "repo_root": str(repo_root),
        "children": {},
        "findings": [],
        "lineage": lineage,
    }
    for name in ("analyze", "critic"):
        path = gates_dir / f"{name}.md"
        gate = load_gate(path) if path.exists() else None
        records = normalize_gate(name, gate) if gate else []
        info = child_freshness(name, path, gate, records, ctx)
        declared = judge_children.get(name) or {}
        if gate is None and declared.get("applicable") is False:
            info["state"] = "not-applicable"
            info["action"] = "none"
        info["artifact"] = f"gates/{name}.md"
        if gate is not None:
            artifact_run = gate_run_id(gate["header"])
            info["artifact_run_id"] = artifact_run
            if run_id:
                info["mode"] = "generated" if artifact_run == run_id else "reused"
        info["open_findings"] = sum(1 for r in records if r["open"] and r["kind"] == "finding")
        report["children"][name] = info
        report["findings"].extend(records)

    report["ratchet_candidates"] = ratchet_candidates(report["findings"], lineage)
    report["judge"] = judge_freshness(judge_path, judge_header, report["children"], repo_root)
    return report


def remediation_only_checked(tasks_path: Path) -> bool:
    """True when no task outside the Gate Remediation section has been checked off."""
    in_section = False
    for line in tasks_path.read_text(encoding="utf-8").splitlines():
        if line.startswith("## "):
            in_section = line.strip().lower().startswith("## gate remediation")
        if re.match(r"^\s*[-*]\s*\[[xX]\]", line) and not in_section:
            return False
    return True


def judge_freshness(judge_path: Path, header: dict, children: dict, repo_root: Path) -> dict:
    if not judge_path.exists():
        return {"exists": False, "state": "missing"}
    recorded = header.get("children") or {}
    stale = [
        name
        for name, info in children.items()
        if info.get("hash") and (recorded.get(name) or {}).get("hash") not in (None, info["hash"])
        and not _compatible_child_evidence(judge_path.parent / f"{name}.md", info["hash"], recorded.get(name) or {}, header)
    ]
    return {
        "exists": True,
        "state": "stale" if stale else "fresh",
        "stale_children": stale,
        "verdict": header.get("verdict"),
        "hash": hash_file(judge_path, repo_root),
    }


# ----------------------------------------------------------------- validation


def derive_verdict(concerns: list[dict], decisions: list[dict]) -> str:
    # A routed concern is owned by a later phase; lifecycle_check keeps it part of completion.
    blocking = [c for c in concerns if c.get("classification") == "blocking-defect" and not c.get("route")]
    if any(not c.get("needs_authority") for c in blocking):
        return "STOP"
    if any(c.get("needs_authority") for c in concerns) or decisions:
        return "CONDITIONAL"
    return "PROCEED"


def validate_judge(
    header: dict,
    index: dict[str, dict],
    superseded: set[str],
    child_runs: dict[str, str] | None = None,
) -> dict:
    errors: list[dict] = []
    warnings: list[dict] = []

    def err(code: str, where: str, message: str) -> None:
        errors.append({"code": code, "where": where, "message": message})

    if header.get("gate") != "judge":
        err("not-a-judge-artifact", "gate", "`gate: judge` is required")
    if not str(header.get("objective") or "").strip():
        err("missing-objective", "objective", "a one-sentence objective is required")
    if not header.get("feature"):
        err("missing-feature", "feature", "the resolved feature identity is required")
    if not header.get("devspark_version"):
        warnings.append({"code": "missing-version", "where": "devspark_version", "message": "provenance field absent"})

    children = header.get("children") or {}
    for name in ("analyze", "critic"):
        child = children.get(name)
        if not isinstance(child, dict):
            err("missing-child-record", f"children.{name}", "both children must be recorded, even when not run")
            continue
        if child.get("state") not in CHILD_STATES:
            err("bad-child-state", f"children.{name}.state", f"state must be one of {CHILD_STATES}")
        if name == "critic" and child.get("applicable") is True and not child.get("signals"):
            err("missing-signals", "children.critic.signals", "applicable Critic must record its change-shape signals")
        if name == "analyze" and child.get("ran") is False and child.get("state") != "fresh":
            err("analyze-not-run", "children.analyze", "Analyze always runs; it may be skipped only when fresh")

    # --- run provenance (only for artifacts that record a Planning Review run) --
    run_id = str((header.get("planning_review") or {}).get("run_id") or "").strip()
    if run_id:
        for name in ("analyze", "critic"):
            child = children.get(name)
            if not isinstance(child, dict) or child.get("state") in ("missing", "not-applicable"):
                continue
            if name == "critic" and child.get("ran") is False:
                continue
            where = f"children.{name}"
            mode = child.get("mode")
            if mode not in CHILD_MODES:
                err("bad-child-provenance", where, f"mode must be one of {CHILD_MODES} when planning_review.run_id is recorded")
                continue
            if child_runs is None:
                continue
            actual = child_runs.get(name, "")
            if mode == "generated":
                if child.get("run_id") != run_id or actual != run_id:
                    err("provenance-mismatch", where, "a generated child must carry this run's id in its own artifact")
            elif child.get("accepted_by_run_id") != run_id or str(child.get("artifact_run_id") or "") != actual:
                err("provenance-mismatch", where, "a reused child must record the artifact's own run id and be accepted by this run")

    concerns = header.get("concerns") or []
    decisions = header.get("decisions_needed") or []
    routed = header.get("routed_obligations") or []
    covered: set[str] = set()
    seen_ids: set[str] = set()
    concern_ids: set[str] = set()

    for concern in concerns:
        cid = str(concern.get("id") or "")
        where = f"concerns.{cid or '?'}"
        if not cid or cid in seen_ids:
            err("bad-concern-id", where, "concern ids must be present and unique")
        seen_ids.add(cid)
        concern_ids.add(cid)

        sources = concern.get("sources")
        if not isinstance(sources, list) or not sources:
            err("empty-sources", where, "a Judge concern must cite at least one child finding (Judge does not discover)")
            sources = []
        records = []
        for src in sources:
            record = index.get(str(src))
            if record is None:
                err("unknown-source", where, f"source {src!r} is not a finding in any child artifact")
            else:
                records.append(record)
                covered.add(str(src))

        classification = concern.get("classification")
        resolution = concern.get("resolution")
        if classification not in CLASSIFICATIONS:
            err("bad-classification", where, f"classification must be one of {CLASSIFICATIONS}")
        if resolution is None:
            if concern.get("needs_authority") is not True:
                err("unset-resolution", where, "resolution may be null only while needs_authority is true")
            decision = concern.get("decision")
            if isinstance(decision, dict) and decision.get("state") not in (None, "pending"):
                err("bad-decision-state", where, "an unset resolution means decision.state is pending")
        elif resolution not in RESOLUTIONS:
            err("bad-resolution", where, f"resolution must be one of {RESOLUTIONS}")
        if concern.get("change_shape") not in CHANGE_SHAPES:
            err("bad-change-shape", where, f"change_shape must be one of {CHANGE_SHAPES}")
        if concern.get("revision_target") not in (None, "plan", "tasks"):
            err("bad-revision-target", where, "revision_target must be plan or tasks when supplied")
        if concern.get("classification_source") not in CLASSIFICATION_SOURCES:
            err("bad-classification-source", where, f"classification_source must be one of {CLASSIFICATION_SOURCES}")
        if not str(concern.get("action") or "").strip():
            err("missing-action", where, "action is required")
        if not isinstance(concern.get("needs_authority"), bool):
            err("bad-needs-authority", where, "needs_authority must be true or false")

        if len(sources) > 1:
            if concern.get("merge_basis") not in MERGE_BASES:
                err("bad-merge-basis", where, f"merge_basis must be one of {MERGE_BASES}")
            if not str(concern.get("merge_reason") or "").strip():
                err("missing-merge-reason", where, "a merged concern must explain the merge")

        # --- reclassification authority ------------------------------------
        source_classes = {r["classification"] for r in records}
        child_classes = {r["classification"] for r in records if r.get("classification_source") == "child"}
        blocking_in = "blocking-defect" in source_classes
        significant = any(r["significant"] for r in records)
        change = concern.get("classification_change")
        if concern.get("classification_source") == "child" and classification not in child_classes:
            err("classification-source-mismatch", where, "classification_source is 'child' but no child supplied it")
        if classification in CLASSIFICATIONS and records:
            crosses = (blocking_in and classification != "blocking-defect") or (
                classification == "blocking-defect" and not blocking_in
            )
            if crosses and not isinstance(change, dict):
                err(
                    "missing-classification-change",
                    where,
                    "crossing the blocking boundary needs a classification_change record",
                )
            elif crosses:
                for key in ("from", "source", "reason"):
                    if not str(change.get(key) or "").strip():
                        err("incomplete-classification-change", where, f"classification_change.{key} is required")
                if change.get("from") not in source_classes:
                    err("bad-classification-change", where, "classification_change.from must be a source classification")
                if str(change.get("source") or "") not in [str(s) for s in sources]:
                    err("bad-classification-change", where, "classification_change.source must be one of the sources")
                if blocking_in and classification != "blocking-defect" and change.get("authority") not in AUTHORITIES:
                    err(
                        "unauthorized-downgrade",
                        where,
                        "a blocking source may not be downgraded without human authority, a governing "
                        "requirement, or deterministic contradictory evidence",
                    )

        # --- resolution consistency ----------------------------------------
        future = concern.get("future_work")
        route = concern.get("route")
        if resolution == "accept":
            if classification not in ("accepted-limitation", "learning"):
                err("accept-classification", where, "accept requires accepted-limitation or learning")
            if future:
                err("accept-with-future-work", where, "an accepted limitation makes no promise of future work")
            if classification == "accepted-limitation":
                accepted_by = str(concern.get("accepted_by") or "").strip()
                if not accepted_by:
                    err("missing-accepted-by", where, "accepted-limitation requires accepted_by")
                elif significant and accepted_by == "planning-review":
                    err(
                        "significant-acceptance-needs-authority",
                        where,
                        "a significant finding cannot be accepted on planning-review's own authority",
                    )
        elif resolution == "defer":
            if classification != "deferred-work":
                err("defer-classification", where, "defer requires deferred-work")
            if not isinstance(future, dict):
                err("defer-without-future-work", where, "defer is a disposition that must preserve the work in future_work")
            else:
                if future.get("preserve") is not True:
                    err("deferral-without-preservation", where, "future_work.preserve must be true for a deferral")
                if future.get("type") not in FUTURE_TYPES:
                    err("bad-future-type", where, f"future_work.type must be one of {FUTURE_TYPES}")
                for key in ("title", "reason"):
                    if not str(future.get(key) or "").strip():
                        err("incomplete-future-work", where, f"future_work.{key} is required")
        elif resolution in ("require", "simplify"):
            if future:
                err("future-work-on-current-work", where, "future_work belongs only to a deferral")
            if classification != "blocking-defect" and not str(concern.get("promoted_by") or "").strip():
                err(
                    "unpromoted-scope-growth",
                    where,
                    "a non-blocking concern becomes current work only on explicit developer intent (promoted_by)",
                )
            if (
                resolution == "require"
                and concern.get("change_shape") == "machinery"
                and not concern.get("forcing_requirement")
                and not concern.get("needs_authority")
            ):
                err(
                    "machinery-without-forcing-requirement",
                    where,
                    "new machinery needs a forcing requirement, or an authority decision",
                )
        if route:
            if not isinstance(route, dict) or route.get("owner") not in ROUTE_OWNERS or not route.get("reason"):
                err("bad-route", where, f"route needs an owner in {ROUTE_OWNERS} and a reason")
            if resolution in ("defer", "accept"):
                err("routed-but-dropped", where, "a routed obligation remains current; it cannot be deferred or accepted")

    # --- decisions --------------------------------------------------------
    referenced: set[str] = set()
    for decision in decisions:
        if not str(decision.get("question") or "").strip():
            err("missing-question", f"decisions_needed.{decision.get('id', '?')}", "a decision needs a question")
        for cid in decision.get("concerns") or []:
            referenced.add(str(cid))
            if str(cid) not in concern_ids:
                err("unknown-concern", f"decisions_needed.{decision.get('id', '?')}", f"{cid!r} is not a concern")
    for concern in concerns:
        if concern.get("needs_authority") and str(concern.get("id")) not in referenced:
            err("authority-without-decision", f"concerns.{concern.get('id')}", "needs_authority requires a decision")

    # --- routed obligations ----------------------------------------------
    for item in routed:
        source = str(item.get("source") or "")
        if source not in index:
            err("unknown-source", "routed_obligations", f"source {source!r} is not in any child artifact")
        else:
            covered.add(source)
        if item.get("owner") not in ROUTE_OWNERS or not str(item.get("reason") or "").strip():
            err("bad-route", "routed_obligations", f"needs an owner in {ROUTE_OWNERS} and a reason")

    # --- every open child finding has a disposition ------------------------
    for source_id, record in index.items():
        if record["source_gate"] in superseded or not record["open"] or source_id in covered:
            continue
        err("undisposed-finding", source_id, "open child finding has no Judge disposition")

    derived = derive_verdict(concerns, decisions)
    declared = header.get("verdict")
    if declared not in VERDICTS:
        err("bad-verdict", "verdict", f"verdict must be one of {VERDICTS}")
    elif declared != derived:
        err("verdict-mismatch", "verdict", f"declared {declared} but the concerns derive {derived}")
    if isinstance(header.get("round"), int) and header["round"] >= 3:
        warnings.append(
            {"code": "bounded-convergence", "where": "round", "message": "third adjudication round; stop and ask a human"}
        )
    return {"errors": errors, "warnings": warnings, "derived_verdict": derived}


def load_judge_context(feature_dir: Path, repo_root: Path) -> tuple[dict, dict[str, dict], set[str]]:
    judge_path = feature_dir / "gates" / "judge.md"
    header = load_gate(judge_path)["header"]
    index: dict[str, dict] = {}
    for name in ("analyze", "critic"):
        path = feature_dir / "gates" / f"{name}.md"
        if path.exists():
            for record in normalize_gate(name, load_gate(path)):
                index[record["source_id"]] = record
    superseded = {
        name
        for name, child in (header.get("children") or {}).items()
        if isinstance(child, dict) and child.get("state") == "superseded"
    }
    return header, index, superseded


# ------------------------------------------------------------------ remediation


def future_work_slug(concern: dict) -> str:
    title = str((concern.get("future_work") or {}).get("title") or concern.get("id") or "future-work")
    slug = re.sub(r"[^a-z0-9]+", "-", title.lower()).strip("-")[:60] or "future-work"
    return f"future-{slug}-{str(concern.get('id') or '').lower()}".strip("-")


def future_work_path(repo_root: Path, concern: dict) -> Path:
    return repo_root / ".devspark.work" / "intake" / f"{future_work_slug(concern)}.md"


def split_dispositions(header: dict, repo_root: Path) -> dict:
    current, routed, accepted, future, pending = [], [], [], [], []
    plan_revisions, task_work = [], []
    questions = {
        str(cid): str(d.get("question") or "")
        for d in header.get("decisions_needed") or []
        for cid in d.get("concerns") or []
    }
    for concern in header.get("concerns") or []:
        summary = {
            "id": concern.get("id"),
            "sources": concern.get("sources"),
            "classification": concern.get("classification"),
            "resolution": concern.get("resolution"),
            "change_shape": concern.get("change_shape"),
            "action": concern.get("action"),
            "needs_authority": concern.get("needs_authority"),
        }
        if concern.get("needs_authority"):
            # Awaiting a human: no task, no future work, not accepted, until the decision is recorded.
            question = (concern.get("decision") or {}).get("question") or questions.get(str(concern.get("id")), "")
            pending.append({**summary, "route": concern.get("route"), "question": question})
        elif concern.get("route"):
            routed.append({**summary, "route": concern["route"]})
        elif concern.get("resolution") in ("require", "simplify"):
            target = concern.get("revision_target")
            if target is None and concern.get("change_shape") in ("planning", "machinery"):
                target = "plan"
            if target not in ("plan", "tasks"):
                pending.append({**summary, "needs_authority": True, "question": "Which approved correction belongs to Plan or Tasks? Record revision_target before acting."})
            else:
                summary["revision_target"] = target
                current.append(summary)
                (plan_revisions if target == "plan" else task_work).append(summary)
        elif concern.get("resolution") == "defer":
            path = future_work_path(repo_root, concern)
            future.append({**summary, "future_work": concern.get("future_work"), "path": str(path)})
        else:
            accepted.append(summary)
    return {
        "current_planning_work": current,
        "plan_revisions": plan_revisions,
        "task_work": task_work,
        "routed_obligations": routed + list(header.get("routed_obligations") or []),
        "accepted": accepted,
        "future_work": future,
        "pending_decisions": pending,
    }


def render_future_work(header: dict, concern: dict, records: list[dict]) -> str:
    future = concern.get("future_work") or {}
    placeholder = "Not captured. A Future Work Plan does not authorize further investigation."
    evidence = (
        "\n".join(f"- `{r['source_id']}` ({r['source_gate']}): {r['summary']}" for r in records)
        or placeholder
    )
    sections = {
        "Problem": future.get("problem") or future.get("title") or placeholder,
        "Originating Spec": str(header.get("feature") or ""),
        "Why Not Current Scope": future.get("reason") or placeholder,
        "Evidence Already Discovered": evidence,
        "Candidate Direction": future.get("candidate_direction") or placeholder,
        "Constraints": future.get("constraints") or placeholder,
        "Open Questions": future.get("open_questions") or placeholder,
        "Activation Trigger": future.get("activation_trigger") or placeholder,
        "Provenance": (
            f"Judge concern {concern.get('id')}; sources "
            + ", ".join(str(s) for s in concern.get("sources") or [])
            + f"; DevSpark {header.get('devspark_version') or 'unknown'}; "
            + datetime.now(timezone.utc).strftime("%Y-%m-%dT%H:%M:%SZ")
        ),
    }
    lines = [f"# Future Work Plan: {future.get('title') or concern.get('id')}", ""]
    lines.append(f"**Type**: {future.get('type')}")
    lines.append("")
    for heading in FUTURE_WORK_SECTIONS:
        lines += [f"## {heading}", "", str(sections[heading]), ""]
    return "\n".join(lines)


# -------------------------------------------------------------------- lifecycle


def _frontmatter_value(path: Path, key: str) -> str:
    if not path.exists():
        return ""
    for line in path.read_text(encoding="utf-8").splitlines():
        match = re.match(rf"^{re.escape(key)}:\s*(.*)$", line)
        if match:
            return match.group(1).strip()
    return ""


def _gate_header(path: Path) -> dict:
    return load_gate(path)["header"] if path.exists() else {}


def verification_state(feature_dir: Path) -> dict:
    spec = feature_dir / "spec.md"
    metadata = load_yaml_blocks(spec) if spec.exists() else []
    required = next((block["required_gates"] for block in metadata if "required_gates" in block), [])
    if isinstance(required, str):
        required = [value.strip() for value in required.split(",")]
    if not isinstance(required, list):
        required = []
    modes = [str(value).removeprefix("verify:") for value in required if str(value).startswith("verify:")]
    verify = _gate_header(feature_dir / "gates/verify.md")
    entries = verify.get("modes") or []
    missing = []
    for mode in modes:
        matches = [entry for entry in entries if isinstance(entry, dict) and entry.get("mode") == mode]
        if not matches or any(entry.get("status") != "pass" for entry in matches):
            missing.append(mode)
    closeout = _gate_header(feature_dir / "gates/closeout.md")
    return {
        "verify_required": bool(modes),
        "required_modes": modes,
        "unmet_modes": missing,
        "verify_passed": verify.get("status") == "pass" and not missing,
        "closeout_decision": closeout.get("decision", ""),
    }


def lifecycle_check(feature_dir: Path, repo_root: Path) -> dict:
    failures: list[dict] = []
    warnings: list[dict] = []
    pending_later: list[dict] = []

    def fail(code: str, message: str) -> None:
        failures.append({"code": code, "message": message})

    if not feature_dir.exists():
        return {"status": "pass", "failures": [], "warnings": [], "pending_later_phase": [], "note": "planning bundle already finalized"}

    tasks_path = feature_dir / "tasks.md"
    if tasks_path.exists():
        open_tasks = [
            (TASK_RE.search(line).group(0) if TASK_RE.search(line) else line.strip()[:40])
            for line in tasks_path.read_text(encoding="utf-8").splitlines()
            if UNCHECKED_RE.match(line)
        ]
        if open_tasks:
            fail("incomplete-tasks", f"{len(open_tasks)} required task(s) are not complete: {', '.join(open_tasks[:10])}")

    gates = feature_dir / "gates"
    required = _frontmatter_value(feature_dir / "spec.md", "required_gates")
    verify = _gate_header(gates / "verify.md")
    closeout = _gate_header(gates / "closeout.md")
    proof = verification_state(feature_dir)
    if proof["verify_required"] and not proof["verify_passed"]:
        fail("verify-not-passed", "a required verify proof mode has not passed")
    if closeout:
        if str(closeout.get("decision")) == "not-complete":
            fail("closeout-not-complete", "closeout decided not-complete; blocking defects remain")
    elif "verify:" in required:
        fail("closeout-missing", "verify is required but no closeout decision exists")

    judge_path = gates / "judge.md"
    if not judge_path.exists():
        needed = [n for n in ("analyze", "critic") if n in required and not (gates / f"{n}.md").exists()]
        if needed:
            fail("planning-review-missing", f"planning review is required ({', '.join(needed)}) but has not run")
    if judge_path.exists():
        header, index, superseded = load_judge_context(feature_dir, repo_root)
        result = validate_judge(header, index, superseded, child_run_ids(feature_dir))
        for error in result["errors"]:
            fail("judge-" + error["code"], f"{error['where']}: {error['message']}")
        if header.get("verdict") != "PROCEED":
            fail(
                "planning-review-open",
                f"planning review verdict is {header.get('verdict')}; record the resolved disposition "
                "with a judge-only recheck before the PR",
            )
        # A PROCEED is evidence only while the children it adjudicated still stand.
        report = build_children_report(feature_dir, repo_root)
        for name, info in report["children"].items():
            if info.get("blocking"):
                fail("planning-review-recheck-required", f"{name} is {info['state']}; {info['action']} it, then re-run planning review")
        if report["judge"].get("state") == "stale":
            fail("planning-review-stale", "a child gate changed after Judge adjudicated; re-run planning review --judge-only")
        parts = split_dispositions(header, repo_root)
        for item in parts["pending_decisions"]:
            fail("pending-decision", f"{item['id']}: a human decision is still pending")
        for item in parts["future_work"]:
            captured = Path(item["path"]).exists() or (item.get("future_work") or {}).get("captured_as")
            if not captured:
                fail("future-work-not-captured", f"{item['id']}: preserved future work was never materialized")
        for item in parts["routed_obligations"]:
            route = item.get("route") or item
            owner = route.get("owner")
            if owner == "verify" and str(verify.get("status")) != "pass":
                fail("routed-verify-incomplete", f"{item.get('id') or item.get('source')}: routed to verify, which has not passed")
            elif owner == "closeout" and str(closeout.get("decision")) != "complete":
                fail("routed-closeout-incomplete", f"{item.get('id') or item.get('source')}: routed to closeout, which is not complete")
            elif owner == "pr-review":
                pending_later.append({"id": item.get("id") or item.get("source"), "owner": "pr-review"})
    return {
        "status": "fail" if failures else "pass",
        "failures": failures,
        "warnings": warnings,
        "pending_later_phase": pending_later,
    }


# ------------------------------------------------------------------------ guard


def _temp_state(prefix: str, feature_dir: Path, run_id: str | None = None) -> Path:
    key = hashlib.sha1(f"{feature_dir.resolve()}|{run_id or ''}".encode("utf-8")).hexdigest()[:16]
    return Path(tempfile.gettempdir()) / f"devspark-judge-{prefix}-{key}.json"


def _guard_file(feature_dir: Path, run_id: str | None = None) -> Path:
    return _temp_state("guard", feature_dir, run_id)


def _snapshot(feature_dir: Path, repo_root: Path) -> dict:
    # `protected` is what Judge may never change (the whole planning bundle); `dirty` and HEAD are
    # context that other, legitimate writers can also move, so they are reported but never blamed.
    protected: dict[str, str] = {}
    for path in sorted(p for p in feature_dir.rglob("*") if p.is_file()):
        protected[path.relative_to(repo_root).as_posix()] = hashlib.sha256(path.read_bytes()).hexdigest()
    status = _git(["-c", "status.renames=false", "status", "--porcelain=v1", "-z", "--untracked-files=all"], repo_root)
    dirty: dict[str, str] = {}
    for record in status.stdout.split("\0"):
        if len(record) < 4:
            continue
        rel = record[3:]
        target = repo_root / rel
        dirty[rel] = hashlib.sha256(target.read_bytes()).hexdigest() if target.is_file() else "<absent>"
    head = _git(["rev-parse", "HEAD"], repo_root).stdout.strip()
    return {"head": head, "protected": protected, "dirty": dirty}


def _seal_file(feature_dir: Path, run_id: str) -> Path:
    return _temp_state("seal", feature_dir, run_id)


def _child_hashes(feature_dir: Path, repo_root: Path) -> dict[str, str]:
    out: dict[str, str] = {}
    for name in ("analyze", "critic"):
        path = feature_dir / "gates" / f"{name}.md"
        if path.exists():
            out[name] = hash_file(path, repo_root)
    return out


def seal_children(feature_dir: Path, repo_root: Path, run_id: str, generated: list[str]) -> dict:
    """Record which child artifacts this run generated or reused; refuse a replaced one.

    `generated` names the children this run just executed; each must carry this run's id.
    """
    seal_path = _seal_file(feature_dir, run_id)
    prior = json.loads(seal_path.read_text(encoding="utf-8")) if seal_path.exists() else {}
    current = _child_hashes(feature_dir, repo_root)
    replaced = sorted(n for n, rec in prior.items() if current.get(n) != rec["hash"])
    sealed: dict[str, dict] = {}
    unexpected: list[str] = []
    for name, digest in current.items():
        artifact_run = gate_run_id(load_gate(feature_dir / "gates" / f"{name}.md")["header"])
        mode = "generated" if artifact_run == run_id else "reused"
        if name in generated and mode != "generated":
            unexpected.append(name)
        sealed[name] = {"hash": digest, "mode": mode, "artifact_run_id": artifact_run, "accepted_by_run_id": run_id}
    if replaced or unexpected:
        return {
            "status": "invalidated",
            "replaced_after_sealing": replaced,
            "not_stamped_with_this_run": sorted(unexpected),
            "message": "a child artifact is not the one this run produced or accepted; rerun that child",
        }
    seal_path.write_text(json.dumps(sealed), encoding="utf-8")
    return {"status": "sealed", "run_id": run_id, "children": sealed}


def _verify_seal(feature_dir: Path, repo_root: Path, run_id: str) -> dict | None:
    """None when every sealed child is unchanged; otherwise a failure payload."""
    seal_path = _seal_file(feature_dir, run_id)
    if not seal_path.exists():
        return {"status": "error", "message": "children were not sealed for this run; run `seal` first"}
    sealed = json.loads(seal_path.read_text(encoding="utf-8"))
    current = _child_hashes(feature_dir, repo_root)
    replaced = sorted(n for n, rec in sealed.items() if current.get(n) != rec["hash"])
    if replaced:
        return {
            "status": "invalidated",
            "replaced_after_sealing": replaced,
            "message": "a child artifact changed after this run sealed it; rerun that child",
        }
    return None


def guard_begin(feature_dir: Path, repo_root: Path, run_id: str | None = None) -> dict:
    if run_id:
        failure = _verify_seal(feature_dir, repo_root, run_id)
        if failure:
            return failure
    snapshot = _snapshot(feature_dir, repo_root)
    _guard_file(feature_dir, run_id).write_text(json.dumps(snapshot), encoding="utf-8")
    return {"status": "armed", "protected_files": len(snapshot["protected"])}


def guard_end(feature_dir: Path, repo_root: Path, run_id: str | None = None) -> dict:
    guard = _guard_file(feature_dir, run_id)
    if not guard.exists():
        return {"status": "error", "message": "guard was never armed"}
    before = json.loads(guard.read_text(encoding="utf-8"))
    after = _snapshot(feature_dir, repo_root)
    guard.unlink()
    allowed = (feature_dir / "gates" / "judge.md").relative_to(repo_root).as_posix()

    def moved(key: str) -> set[str]:
        return {p for p in set(before[key]) | set(after[key]) if before[key].get(p) != after[key].get(p)}

    violations = sorted(moved("protected") - {allowed})
    if violations:
        return {
            "status": "violated",
            "modified": violations,
            "note": "a protected planning artifact changed during the Judge window; if another process "
            "wrote it, re-run planning review",
        }
    # Outside the protected set the change cannot be attributed to Judge.
    unattributed = sorted(moved("dirty") - {allowed})
    if before["head"] != after["head"]:
        unattributed.append("<HEAD moved>")
    if unattributed:
        return {"status": "concurrent-change-detected", "unattributed": unattributed[:20], "unattributed_count": len(unattributed)}
    return {"status": "ok", "modified": []}


def edge_state(downstream: Path, field: str, upstream: Path) -> str:
    """`current`, `stale` or `unknown` for one provenance stamp; read-only, and `unknown` is never `stale`."""
    if not downstream.is_file() or not upstream.is_file():
        return "unknown"
    try:
        text = downstream.read_text(encoding="utf-8-sig")
        front = FRONTMATTER.match(text)
        metadata = yaml.load(front.group(1), Loader=_SemanticLoader) if front else None
    except (OSError, UnicodeError, yaml.YAMLError):
        return "unknown"
    stamp = metadata.get(field) if isinstance(metadata, dict) else None
    if not isinstance(stamp, str) or not re.fullmatch(r"sem1:[0-9a-f]{64}", stamp):
        return "unknown"
    current = semantic_hash(upstream)
    if current is None:
        return "unknown"
    return "current" if current == stamp else "stale"


def review_state(feature_dir: Path, repo_root: Path) -> dict:
    """Compact Planning Review state for `next`; the verdict is the recorded one, recheck is computed."""
    judge_path = feature_dir / "gates" / "judge.md"
    proof = verification_state(feature_dir)
    edges = {
        "plan_spec_state": edge_state(feature_dir / "plan.md", "spec_semantic_hash", feature_dir / "spec.md"),
        "tasks_plan_state": edge_state(feature_dir / "tasks.md", "plan_semantic_hash", feature_dir / "plan.md"),
    }
    if not judge_path.exists():
        return {"state": "missing", "needs_recheck": False, **edges, **proof}
    report = build_children_report(feature_dir, repo_root)
    header = load_gate(judge_path)["header"]
    stale_children = sorted(n for n, i in report["children"].items() if i.get("blocking"))
    judge_stale = report["judge"].get("state") == "stale"
    dispositions = split_dispositions(header, repo_root)

    def input_changed(name: str) -> bool | None:
        path = feature_dir / name
        if not path.is_file():
            return None
        raw = hash_file(path, repo_root)
        semantic = semantic_hash(path)
        comparisons = []
        for child in ("analyze", "critic"):
            gate_path = feature_dir / "gates" / f"{child}.md"
            if not gate_path.is_file():
                continue
            for entry in load_gate(gate_path)["reviewed_artifacts"]:
                resolved = _resolve_artifact(str(entry.get("path") or ""), repo_root, feature_dir)
                if resolved != path.resolve():
                    continue
                recorded = entry.get("semantic_hash")
                same = entry.get("hash") == raw or (
                    isinstance(recorded, str) and recorded.startswith("sem1:") and semantic == recorded
                )
                comparisons.append(not same)
        return any(comparisons) if comparisons else None

    plan_hash = semantic_hash(feature_dir / "plan.md")
    tasks_path = feature_dir / "tasks.md"
    task_metadata = load_yaml_blocks(tasks_path) if tasks_path.is_file() else []
    recorded_plan = next((block["plan_semantic_hash"] for block in task_metadata if "plan_semantic_hash" in block), None)
    return {
        "state": "present",
        "verdict": header.get("verdict"),
        "needs_recheck": bool(stale_children or judge_stale),
        "recheck_children": stale_children,
        "judge_stale": judge_stale,
        "pending_decisions": len(dispositions["pending_decisions"]),
        "plan_revision_count": len(dispositions["plan_revisions"]),
        "task_work_count": len(dispositions["task_work"]),
        "plan_changed": input_changed("plan.md"),
        "spec_changed": input_changed("spec.md"),
        "tasks_changed": input_changed("tasks.md"),
        "tasks_stale": plan_hash is None or recorded_plan != plan_hash,
        **edges,
        **proof,
    }


# ---------------------------------------------------------------------- resolve


def resolve_feature(repo_root: Path, explicit: str | None, branch: str | None) -> dict:
    specs = repo_root / ".devspark.work" / "specs"
    if explicit:
        candidate = Path(explicit)
        directory = candidate if candidate.is_absolute() else repo_root / candidate
    else:
        if not branch:
            res = _git(["branch", "--show-current"], repo_root)
            branch = res.stdout.strip()
        exact = specs / branch
        match = re.match(r"^(\d{3,})-", branch or "")
        if (exact / "spec.md").exists():
            directory = exact
        elif match:
            prefix = match.group(1)
            found = sorted(p for p in specs.glob(f"{prefix}-*") if (p / "spec.md").exists())
            if len(found) != 1:
                return {"status": "error", "message": f"{len(found)} spec directories match prefix {prefix}"}
            directory = found[0]
        else:
            return {"status": "error", "message": f"no spec resolves for branch {branch!r}"}
    if not (directory / "spec.md").exists():
        return {"status": "error", "message": f"{directory} has no spec.md"}
    run_id = new_run_id()
    return {
        "status": "ok",
        "feature": directory.name,
        "feature_dir": str(directory),
        "branch": branch,
        "run_id": run_id,
        "handoff": {
            "FEATURE_DIR": str(directory),
            "DEVSPARK_FEATURE": directory.name,
            "PLANNING_REVIEW_RUN_ID": run_id,
        },
    }


# ------------------------------------------------------------------------- main


def _emit(payload: dict, code: int = 0) -> int:
    print(json.dumps(payload, indent=2))
    return code


def main(argv: list[str] | None = None) -> int:
    parser = argparse.ArgumentParser(description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    sub = parser.add_subparsers(dest="command", required=True)
    semantic = sub.add_parser("semantic-hash")
    semantic.add_argument("--path", required=True)
    for name in ("resolve", "children", "validate", "remediation", "future-work", "lifecycle", "guard", "seal", "review-state"):
        p = sub.add_parser(name)
        p.add_argument("--repo-root")
        p.add_argument("--feature-dir")
        if name in ("children", "guard", "seal"):
            p.add_argument("--run-id")
        if name == "seal":
            p.add_argument("--generated", default="", help="comma-separated children this run just executed")
        if name == "resolve":
            p.add_argument("--branch")
        if name == "future-work":
            p.add_argument("--write", action="store_true")
        if name == "guard":
            p.add_argument("phase", choices=("begin", "end"))
    args = parser.parse_args(argv)

    if args.command == "semantic-hash":
        path = Path(args.path).resolve()
        value = semantic_hash(path)
        return _emit({"path": str(path), "role": path.stem, "algorithm": "sem1", "semantic_hash": value,
                      "status": "ok" if value is not None else "unavailable"})

    if args.command == "resolve":
        root = Path(args.repo_root).resolve() if args.repo_root else Path.cwd()
        result = resolve_feature(root, args.feature_dir, args.branch)
        return _emit(result, 0 if result["status"] == "ok" else 1)

    if not args.feature_dir:
        print("--feature-dir is required", file=sys.stderr)
        return 2
    feature_dir = Path(args.feature_dir).resolve()
    repo_root = find_repo_root(feature_dir, args.repo_root)

    if args.command == "children":
        return _emit(build_children_report(feature_dir, repo_root, args.run_id))
    if args.command == "review-state":
        return _emit(review_state(feature_dir, repo_root))
    if args.command == "seal":
        if not args.run_id:
            print("--run-id is required", file=sys.stderr)
            return 2
        generated = [n.strip() for n in args.generated.split(",") if n.strip()]
        result = seal_children(feature_dir, repo_root, args.run_id, generated)
        return _emit(result, 0 if result["status"] == "sealed" else 1)
    if args.command == "guard":
        if args.phase == "begin":
            result = guard_begin(feature_dir, repo_root, args.run_id)
        else:
            result = guard_end(feature_dir, repo_root, args.run_id)
        return _emit(result, 0 if result["status"] in ("armed", "ok", "concurrent-change-detected") else 1)
    if args.command == "lifecycle":
        result = lifecycle_check(feature_dir, repo_root)
        return _emit(result, 0 if result["status"] == "pass" else 1)

    if not (feature_dir / "gates" / "judge.md").exists():
        return _emit({"status": "error", "message": "gates/judge.md does not exist"}, 2)
    header, index, superseded = load_judge_context(feature_dir, repo_root)
    result = validate_judge(header, index, superseded, child_run_ids(feature_dir))

    if args.command == "validate":
        result["status"] = "fail" if result["errors"] else "pass"
        return _emit(result, 1 if result["errors"] else 0)

    if result["errors"]:
        return _emit({"status": "fail", "message": "judge.md does not validate", **result}, 1)
    parts = split_dispositions(header, repo_root)
    if args.command == "remediation":
        return _emit({"status": "ok", "verdict": header.get("verdict"), **parts})

    written, skipped = [], []
    if args.write:
        for item in parts["future_work"]:
            target = Path(item["path"])
            if target.exists():
                skipped.append(str(target))
                continue
            concern = next(c for c in header["concerns"] if c.get("id") == item["id"])
            records = [index[str(s)] for s in concern.get("sources") or [] if str(s) in index]
            target.parent.mkdir(parents=True, exist_ok=True)
            target.write_text(render_future_work(header, concern, records), encoding="utf-8")
            written.append(str(target))
    return _emit({"status": "ok", "written": written, "skipped": skipped, "planned": [i["path"] for i in parts["future_work"]]})


if __name__ == "__main__":
    sys.exit(main())
