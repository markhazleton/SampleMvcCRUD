<!-- BSW.DevSpark — © 2026 Baylor Scott & White Health. Source: https://bsw-devspark.bswhive.com -->

````markdown
---
description: Perform comprehensive codebase audit against project constitution/standards, producing structured compliance report
handoffs:
  - label: Re-run Current Audit
    agent: devspark-site-audit
    prompt: Re-run the audit against the current durable repository state.
scripts:
  sh: .devspark/scripts/bash/site-audit.sh $ARGUMENTS --json
  ps: .devspark/scripts/powershell/site-audit.ps1 $ARGUMENTS -Json
---

## User Input

```text
$ARGUMENTS
```

You **MUST** consider the user input before proceeding (if not empty).

## Overview

This command performs a comprehensive codebase audit against the project constitution/standards document. It scans the entire repository (or specified scope) for compliance violations, code quality issues, unused dependencies, and architectural concerns.

**IMPORTANT**: This command **only provides analysis** - it does not make any code changes.

Load and obey the shared preamble contract core at `/.devspark/templates/command-preamble-contract.md` (installed repos) or `templates/command-preamble-contract.md` (source repos) before step 1 — in particular §9 (Genuine Fix Discipline) and its §9.1 intent-cue table when phrasing code-quality findings. Preamble modules: `finding-fix`, `convergence`, `provenance`, `configuration`.

## Prerequisites

- Project constitution at `/.knowledge/governance/constitution.md` (REQUIRED)
- PowerShell 7+ (for script execution)
- pip-audit (optional, for Python security scanning)

## Scope Options

Parse `$ARGUMENTS` for scope flags:

| Flag | Description |
|------|-------------|
| `--scope=full` | Complete audit (default) - all checks |
| `--scope=constitution` | Constitution compliance only |
| `--scope=packages` | Package/dependency analysis only |
| `--scope=quality` | Code quality metrics only |
| `--scope=unused` | Unused code/dependencies detection |
| `--scope=duplicate` | Duplicate code detection |
| `--scope=comments` | Stale comments and escaped planning references only |
| `--scope=evidence` | Evidence-accuracy re-run and graph-adjacent contradiction scan only |
| `--scope=knowledge` | Knowledge completeness and knowledge/code mapping accuracy only |

If no scope specified, default to `--scope=full`.

## Outline

Multi-app scope and the 2-tier override check for scripts are defined by the shared preamble contract already loaded above.

### 1. Initialize Audit Context

Run `{SCRIPT}` to gather codebase data and parse JSON output for:
- `REPO_ROOT`: Repository root path
- `CONSTITUTION_PATH`: Path to constitution file
- `FILES`: Categorized file listings
- `PACKAGES`: Dependency information
- `METRICS`: Code metrics (line counts, file counts)

Treat pre-scan JSON as summary context:
- Use `files.counts` and sampled file arrays as the primary source.
- Do not assume sampled arrays are exhaustive.
- Only request full inventories when explicitly needed and user-approved.

Execution limits (required):
- Max findings in report: 5 highest-signal items
- Max broad follow-up searches: 6
- Max file reads per finding: 3
- Stop early once evidence is sufficient for high-confidence findings
- If confidence is low, ask one clarifying question instead of broadening scope

**Error Handling**:
If the script fails:
- **Constitution missing**: Guide user to run `/devspark-constitution`
- **Script execution failed**: Provide PowerShell troubleshooting

For single quotes in args like "I'm auditing", use escape syntax: e.g 'I'\''m auditing' (or double-quote if possible: "I'm auditing").

### 2. Load Constitution

Read and parse `/.knowledge/governance/constitution.md`:
- Extract all core principles with their names
- Identify MUST requirements (non-negotiable/mandatory)
- Identify SHOULD requirements (recommended)
- Note constitution version and amendment date
- Build a checklist of principles to audit against

If constitution doesn't exist:
- **STOP** and inform user that constitution is required
- Provide guidance: "Run `/devspark-constitution` to create project principles first"
- Do not proceed with audit

### 3. File Discovery and Categorization

Using script output or file system scan, categorize files:

#### Categories
- **Source Code**: `.py`, `.ts`, `.js`, `.cs`, `.java`, `.go`, `.rs`, etc.
- **Configuration**: `*.json`, `*.yaml`, `*.yml`, `*.toml`, `*.ini`, `*.env*`
- **Documentation**: `*.md`, `*.rst`, `*.txt`
- **Tests**: Files in `test*/`, `*_test.*`, `*.test.*`, `*_spec.*`
- **Scripts**: `*.sh`, `*.ps1`, `*.bash`
- **Build/CI**: `Dockerfile*`, `.github/workflows/*`, `Makefile`, `*.gradle`

#### Exclusions
Skip these by default:
- `node_modules/`, `venv/`, `.venv/`, `__pycache__/`
- `.git/`, `.vs/`, `.idea/`
- `dist/`, `build/`, `bin/`, `obj/`
- `*.min.js`, `*.map`

### 4. BSW.DevSpark Version Check

Before auditing code, check whether the project's BSW.DevSpark installation is
current. Stale installations may have outdated command files or missing framework scripts.

#### A. Read Version Stamp

Check for `.devspark/BSW.DevSpark.version` first (fallback: legacy `.documentation/DEVSPARK_VERSION`):

- **If both are missing**: Flag `VER1` — stamp absent, version unknown (HIGH)
- **If present**: Parse `version`, `installed`, and `method` fields (legacy stamp may use `agent`)

#### B. Detect Latest Version

Read the most recent `## [X.Y.Z]` entry in `CHANGELOG.md` (repo root) to get
`LATEST_VERSION`. Fallback: read `version = "..."` from `pyproject.toml`.

#### C. Compare and Flag

| Condition | Finding ID | Severity |
|-----------|-----------|---------|
| `.devspark/BSW.DevSpark.version` absent and legacy stamp absent | VER1 | HIGH |
| Installed version < latest version | VER2 | MEDIUM |
| Agent command files reference `.specify/` or root `memory/`, `scripts/`, `templates/`, or `specs/` paths | VER3 | HIGH |
| Root-level `memory/`, `scripts/`, `templates/`, or `specs/` directories exist | VER4 | HIGH |
| Old `devspark.*-old.md` files in agent folder | VER5 | LOW |

Include in the audit report under a **BSW.DevSpark Version** section:

```markdown
## BSW.DevSpark Version

| Field | Value |
|-------|-------|
| Installed Version | {version or "absent"} |
| Latest Version    | {LATEST_VERSION} |
| Install Date      | {installed field} |
| Method            | {method or agent field} |
| Status            | UP TO DATE / UPGRADE AVAILABLE / UNKNOWN |
```

If VER1 or VER2 is present, add to the Recommendations section:
> Re-run your agent's quickstart guide (or `/devspark-upgrade`) to update BSW.DevSpark.

### 5. Durable Delta Consistency Audit

Audit the repository's current durable state: source code, tests, durable configuration/scripts,
and `.knowledge/`. Do not treat `.devspark.work/` workflow artifacts or `.archive/` contents as
current truth, and do not require, recover, or judge temporary plans. Missing
`.devspark.work/specs/` content is normal. `.devspark.work/devspark.json` may be read only when
needed to resolve repository or application scope. Existing reports under
`.devspark.work/audit/` may be read for historical comparison, and the current audit report is
written there; neither is audit evidence.

1. Map changed or behavior-bearing code to affected tests and `.knowledge/` nodes using `appliesTo`.
2. Flag code behavior that contradicts current knowledge (`DELTA1`, HIGH).
3. Flag current knowledge that claims behavior unsupported by code or tests (`DELTA2`, HIGH).
4. Flag changed behavior without proportionate executable coverage (`DELTA3`, severity by risk).
5. Scan durable code, tests, and `.knowledge/` for planning identifiers or links
  such as spec IDs, `FR-###`, `T###`, phase references, or paths into `.devspark.work/` or
  `.archive/` (`DELTA4`, HIGH) — an archived path is just as forbidden as a live planning path,
  since it still points at ephemeral state that may be pruned or renumbered. Do not scan the
  temporary planning root or `.archive/` itself for this finding.
6. Report consistency by durable output, with direct evidence and a concrete repair. Do not use a
  plan's status or task completion as evidence.
7. **Evidence-accuracy pass** (repo-wide, distinct from the existence check above): for every
  `.knowledge/` node/decision citing `verified_by: execution` (or an evidence entry of `type: test`),
  re-run the cited test now and confirm it still passes. The coverage/gap report only checks that a
  document *exists* for a required layer — this pass catches documents that exist but whose claim no
  longer holds. A test that no longer passes, or no longer exists, is `DELTA5` (HIGH — the node's
  claim is unverifiable as written).
8. **Contradiction scan**, scoped to graph-adjacent objects only — never brute-force pairwise
  comparison across the whole repo: entities/decisions sharing the same entity id, entities named in
  the same decision's `constrains`, or objects citing the same `source_of_truth`. Surface a
  candidate contradiction as `DELTA6` (always a **warning**, never a gate — judging genuine
  contradiction vs. acceptable nuance is a human call every time).
9. **Knowledge completeness** (repo-wide structural check, `.knowledge/` in its entirety — this is
   the repo-wide counterpart to `/devspark-pr-review`'s diff-scoped §6c gate): run
   `scripts/build_knowledge_index.py --repo-root . --check` (or the installed-repo path). A non-zero
   exit means `.knowledge/index.json` and/or `.knowledge/ontology/coverage.json` are stale relative
   to the source docs — `KNOW1` (HIGH, regenerate before trusting any other knowledge finding in
   this audit). Then read the regenerated `.knowledge/ontology/coverage.json` in full (every entity,
   not just diff-touched ones) and flag every reported gap — missing required layer doc, invalid
   `source_of_truth`, stale `last_verified` — as `KNOW2` (severity by entity criticality, floor
   MEDIUM).
10. **Knowledge/code mapping accuracy** — for every `appliesTo` and `source_of_truth` path cited
    anywhere under `.knowledge/` (flat docs and entities alike), confirm the path still exists in
    the repository **and** does not resolve into `.devspark.work/` or `.archive/`. A reference to a
    moved, renamed, or deleted file, or one pointing into either ephemeral root (`build_knowledge_index.py`
    already hard-fails this at generation time — this pass is the repo-wide backstop for a
    hand-edited or pre-generation file), is `KNOW3` (HIGH — the mapping between knowledge and code
    is exactly what the field exists to guarantee, and a dangling or ephemeral reference silently
    breaks retrieval for every command that resolves context through it).
11. **Relation/decision resolution** — for every `relations[].object` in an `_entity.yaml`, every
    `constrains` entry in a governance decision, and its reciprocal `constrained_by` on the named
    entity, confirm the cited id resolves to a real, currently-existing node. An unresolved or
    one-directional reference is `KNOW4` (HIGH).
12. **Taxonomy and coverage sweep** — cross-reference `.knowledge/taxonomy-registry.json`'s
    `pathPattern` entries against the actual `.knowledge/` tree: a file matching no pattern is
    unclassified content (`KNOW5`, MEDIUM). Separately, using the same repo-structure signals
    `/devspark-discover-constitution` already gathers (directory layout, layer separation, natural
    module boundaries), flag a central, behavior-bearing module with **no** corresponding flat doc
    or entity anywhere under `.knowledge/` as a coverage gap (`KNOW6`, MEDIUM) — evidence this exists
    is required, never a guess; do not fabricate an entity to close the gap, only report it.
13. **Knowledge integrity validation** (DevSpark 7.6) — run
    `scripts/knowledge-integrity.py --repo-root . --json` (installed-repo path:
    `.devspark/scripts/knowledge-integrity.py`, or the corresponding
    `scripts/{bash,powershell}/knowledge-integrity.{sh,ps1}` wrapper) rather than reimplementing
    its checks inline. Fold its structured findings into this report by category:
    `engine-divergence` and `generated-artifact-drift` are `KNOW7` (HIGH — two disagreeing
    definitions of `.knowledge` truth, or a stale generated artifact, make every other knowledge
    finding in this audit unreliable until resolved), `unreachable-knowledge-root` is `KNOW8`
    (HIGH — a `.knowledge/` directory the canonical index never covers means content sitting
    there is invisible to every command that resolves context through the index), and
    `schema-tooling-contradiction` is `KNOW9` (MEDIUM — the taxonomy registry declares a
    `nodeType` the engine does not recognize). A non-zero exit from `knowledge-integrity.py`
    means at least one hard-failure finding was reported; surface its findings verbatim (subject,
    summary, evidence) rather than re-deriving them, since the checks are already source-of-truth
    for engine-divergence and root-reachability signals.

### 6. Constitution Compliance Audit

For **each principle** in the constitution:

#### A. Pattern Detection
Based on principle type, scan for violations:

**Security Principles**:
- Hardcoded secrets (API keys, passwords, tokens)
- Insecure patterns (`eval()`, `exec()`, SQL string concatenation)
- Missing input validation patterns
- Exposed sensitive data in logs

**Code Quality Principles**:
- Naming convention violations
- Missing type hints/annotations
- Excessive function length (>50 lines)
- Deep nesting (>4 levels)
- Magic numbers/strings

**Architecture Principles**:
- Circular dependencies
- Layer violations (e.g., UI calling DB directly)
- Missing abstractions
- Coupling issues

**Testing Principles**:
- Source files without corresponding tests
- Test coverage patterns
- Missing test fixtures

**Documentation Principles**:
- Missing docstrings/comments
- Outdated README references
- Missing API documentation

#### B. Generate Findings
For each violation found:
- **ID**: Unique identifier (SEC1, QUAL1, ARCH1, TEST1, DOC1, etc.)
- **Principle**: Name of constitution principle violated
- **File:Line**: Exact location
- **Issue**: Specific description
- **Intent**: For code-quality/structural findings (function length, nesting, complexity, duplication, swallowed exceptions), one sentence naming the behavioral intent the metric is a proxy for, per `command-preamble-contract.md` §9.1 — so a downstream fix resolves the intent, not just the number. Phrase **Issue** and **Recommendation** around that intent rather than the bare metric: a published eval measured genuine-fix rates of ~5.6% for bare-metric phrasing vs. ~83% when intent was stated first.
- **Recommendation**: Concrete fix

### 7. Package/Dependency Audit

#### A. Detect Package Manager
Identify from files present:
- `requirements.txt`, `pyproject.toml`, `setup.py` → Python/pip
- `package.json`, `package-lock.json` → Node/npm
- `Cargo.toml` → Rust/cargo
- `go.mod` → Go modules
- `*.csproj`, `packages.config` → .NET/NuGet

#### B. Dependency Analysis
For each detected package manager:
- **Outdated packages**: Compare versions to latest
- **Security vulnerabilities**: Run `pip-audit`, `npm audit`, etc.
- **Unused dependencies**: Detect imported but unused
- **Missing dependencies**: Used but not declared
- **License compliance**: Check for incompatible licenses

#### C. Dependency Graph
- Identify direct vs transitive dependencies
- Flag heavy transitive chains
- Note conflicting version requirements

### 8. Code Quality Metrics

Calculate and report:

#### Size Metrics
- Total lines of code (excluding blanks/comments)
- Lines per file (average, max)
- Files per directory (average, max)

#### Complexity Indicators
- Files with excessive length (>500 lines)
- Functions with high cyclomatic complexity
- Deep nesting occurrences
- Large classes/modules

#### Maintainability Signals
- Code duplication percentage
- TODO/FIXME/HACK comment count
- Commented-out code blocks
- Inconsistent formatting patterns

### 9. Stale Comments and Escaped Planning References Audit

Scan source files for code comments that have lost their value or reference completed work.

#### A. Planning References in Durable Outputs

Search durable source, tests, current knowledge, and published documentation for planning-only
identifiers:

```text
# spec 026
# FR-013
# T006
# Phase 5
# TODO(spec-018)
# See spec-032
```

Each match is stale regardless of planning status. Flag it, include file:line and the matched text,
and recommend rewriting it as a self-contained statement of current behavior. Never inspect a plan
to interpret the reference.

#### B. Old-Behavior Comments

Detect comments that describe behavior that no longer matches the code:

- Comments with past tense ("previously", "used to", "was changed in")
- Comments referencing removed functions, classes, or variables that no longer exist in the file
- Inline explanations that contradict the current logic in the same block

#### C. Commented-Out Code Blocks

Flag commented-out code blocks exceeding 3 consecutive lines. These accumulate technical debt and should either be deleted in a dedicated commit or restored as active code — use `git blame` to understand the original intent before removing.

#### D. Version Migration Comments

Flag comments of the form "Added in v2.8.0", "Deprecated since v3.0", "TODO: remove after upgrade" when the referenced version is already past. These provide no value over `git blame` and clutter the codebase.

#### False-Positive Suppression Policy

Before flagging any finding in sections 9–11, apply these suppression rules to avoid false positives:

**DO NOT flag**:

1. **Unused imports that are re-exports**: If an import appears in an `__init__.py` or barrel file (e.g., `index.ts`) and is explicitly re-exported with `__all__`, `export`, or a wildcard, suppress the "unused import" finding.

2. **Dead code reachable via dynamic dispatch**: If a function or class appears uncalled but the file registers plugins, uses `getattr`, metaclasses, `importlib`, or decorator-based registries, do not flag it as dead code. Note the dynamic dispatch pattern instead.

3. **Test-only utilities appearing unused from production**: Functions in `test*/`, `conftest.py`, `fixtures/`, or `*_helpers.py` that have no callers in the production source tree are not dead code — they serve test infrastructure. Only flag if the file has no test callers either.

Include findings from this phase in the audit report under **Stale Code Comments**:

```markdown
## Stale Code Comments

### Escaped Planning References

| ID | File:Line | Reference | Action |
|----|-----------|-----------|--------|
| CMT1 | src/handler.py:45 | `# spec 026` | Rewrite as current behavior |

### Commented-Out Code Blocks

| ID | File:Lines | Size | Action |
|----|-----------|------|--------|
| CMT2 | src/utils.py:89-95 | 7 lines | Delete or restore |

### Version Migration Comments

| ID | File:Line | Comment | Action |
|----|-----------|---------|--------|
| CMT3 | src/api.py:12 | `# Added in v2.0` | Remove — no value over git blame |
```

### 10. Unused Code Detection

Scan for potentially unused:

#### Dead Code
- Functions/methods never called
- Classes never instantiated
- Variables assigned but never read
- Imports never used

Apply the false-positive suppression policy from section 9 before flagging any unused code.

#### Dead Files
- Source files not imported anywhere
- Test files for non-existent sources
- Config files not referenced

#### Dead Dependencies
- Packages in requirements but never imported
- DevDependencies in package.json unused

### 11. Duplicate Code Detection

Identify copy-paste patterns:

#### Detection Criteria
- Exact duplicate blocks (>10 lines)
- Near-duplicate blocks (>80% similarity, >15 lines)
- Repeated patterns across files

#### Report Format
For each duplicate:
- Locations (file:line ranges)
- Similarity percentage
- Suggested consolidation approach

### 12. Severity Classification

Apply consistent severity across all findings:

| Severity | Criteria |
|----------|----------|
| **CRITICAL** | Security vulnerability, constitution MUST violation, blocking issue |
| **HIGH** | Constitution SHOULD violation, significant quality issue, outdated security packages |
| **MEDIUM** | Code quality concern, maintainability issue, missing tests |
| **LOW** | Style suggestion, minor improvement, optimization opportunity |

### 13. Generate Audit Report

Create comprehensive report at `/.devspark.work/audit/YYYY-MM-DD_results.md`:

#### Ensure Directory Exists
- Check if `/.devspark.work/audit/` exists
- Create directory structure if missing

#### Report Structure

Use this format:

```markdown
# Codebase Audit Report

## Audit Metadata

- **Audit Date**: [YYYY-MM-DD HH:MM:SS UTC]
- **Scope**: [full|constitution|packages|quality|unused|duplicate]
- **Auditor**: devspark-site-audit
- **Constitution Version**: [VERSION from constitution]
- **Repository**: [REPO_NAME]

## Executive Summary

### Compliance Score

| Category | Score | Status |
|----------|-------|--------|
| BSW.DevSpark Version | [UP TO DATE / UPGRADE AVAILABLE / UNKNOWN] | [Status] |
| Durable Delta Consistency | [CONSISTENT / FINDINGS] | [Status] |
| Knowledge Completeness & Mapping Accuracy | [COMPLETE / GAPS] | [Status] |
| Constitution Compliance | [X]% | [✅ PASS / ⚠️ PARTIAL / ❌ FAIL] |
| Security | [X]% | [Status] |
| Code Quality | [X]% | [Status] |
| Test Coverage | [X]% | [Status] |
| Documentation | [X]% | [Status] |
| Dependencies | [X]% | [Status] |

**Overall Health**: [HEALTHY / NEEDS ATTENTION / CRITICAL ISSUES]

### Issue Summary

| Severity | Count |
|----------|-------|
| 🔴 CRITICAL | [X] |
| 🟠 HIGH | [X] |
| 🟡 MEDIUM | [X] |
| 🔵 LOW | [X] |

## Constitution Compliance

### Principle Compliance Matrix

| Principle | Status | Violations | Key Issues |
|-----------|--------|------------|------------|
| [Principle 1] | ✅ PASS | 0 | - |
| [Principle 2] | ⚠️ PARTIAL | 3 | Missing tests for 3 modules |
| [Principle 3] | ❌ FAIL | 12 | Hardcoded credentials found |

### Detailed Violations

| ID | Principle | File:Line | Issue | Severity | Recommendation |
|----|-----------|-----------|-------|----------|----------------|
| SEC1 | Security | src/config.py:45 | Hardcoded API key | CRITICAL | Use environment variable |

## BSW.DevSpark Version

| Field | Value |
|-------|-------|
| Installed Version | [version from `.devspark/BSW.DevSpark.version`, or "absent"] |
| Latest Version | [LATEST_VERSION] |
| Install Date | [installed field] |
| Agent | [agent field] |
| Status | [UP TO DATE / UPGRADE AVAILABLE / UNKNOWN] |

### Version Findings

| ID | Issue | Severity | Recommendation |
|----|-------|----------|----------------|
| VER1 | VERSION stamp absent | HIGH | Re-run your agent's quickstart guide to install or refresh the version stamp |
| VER2 | Version X.Y.Z installed, X.Y.Z available | MEDIUM | Run `/devspark-upgrade` to update |

## Security Findings

### Vulnerability Summary

| Type | Count | Severity |
|------|-------|----------|
| Hardcoded Secrets | [X] | CRITICAL |
| Insecure Patterns | [X] | HIGH |
| Missing Validation | [X] | MEDIUM |

### Security Checklist

- [ ] No hardcoded secrets or credentials
- [ ] Input validation present where needed
- [ ] No SQL injection vulnerabilities
- [ ] No XSS vulnerabilities
- [ ] Dependencies free of known vulnerabilities
- [ ] Secure configuration practices

### Detailed Security Issues

[List each security finding with file, line, issue, recommendation]

## Package/Dependency Analysis

### Package Manager: [pip/npm/cargo/etc.]

#### Dependency Summary

| Metric | Value |
|--------|-------|
| Total Dependencies | [X] |
| Direct Dependencies | [X] |
| Transitive Dependencies | [X] |
| Outdated | [X] |
| Vulnerable | [X] |
| Unused | [X] |

#### Vulnerable Packages

| Package | Current | Fixed In | Vulnerability | Severity |
|---------|---------|----------|---------------|----------|
| [package] | 1.0.0 | 1.0.1 | CVE-XXXX-XXXX | CRITICAL |

#### Outdated Packages

| Package | Current | Latest | Type |
|---------|---------|--------|------|
| [package] | 1.0.0 | 2.0.0 | Major |

#### Unused Dependencies

| Package | Declared In | Notes |
|---------|-------------|-------|
| [package] | requirements.txt | No imports found |

## Code Quality Analysis

### Metrics Overview

| Metric | Value | Threshold | Status |
|--------|-------|-----------|--------|
| Total Lines of Code | [X] | - | - |
| Average Lines per File | [X] | <300 | [Status] |
| Max Lines per File | [X] | <500 | [Status] |
| High Complexity Functions | [X] | 0 | [Status] |
| Deep Nesting Occurrences | [X] | 0 | [Status] |
| TODO Comments | [X] | - | INFO |

### Files Requiring Attention

| File | Issue | Metric | Recommendation |
|------|-------|--------|----------------|
| src/large_module.py | Excessive length | 850 lines | Split into smaller modules |

### Quality Issues

| ID | Category | File:Line | Issue | Severity |
|----|----------|-----------|-------|----------|
| QUAL1 | Complexity | src/handler.py:120 | Function exceeds 50 lines | MEDIUM |

## Test Coverage Analysis

### Coverage Summary

| Category | Files | With Tests | Coverage |
|----------|-------|------------|----------|
| Source Files | [X] | [Y] | [Z]% |
| Critical Paths | [X] | [Y] | [Z]% |

### Untested Files

| File | Importance | Recommendation |
|------|------------|----------------|
| src/auth.py | HIGH | Add unit tests for authentication logic |

## Documentation Status

### Documentation Coverage

| Type | Present | Quality |
|------|---------|---------|
| README.md | ✅ | Good |
| API Documentation | ⚠️ | Incomplete |
| Code Comments | ✅ | Adequate |
| Inline Docstrings | ⚠️ | Partial |

### Missing Documentation

| Item | Location | Priority |
|------|----------|----------|
| Function docstring | src/utils.py:45 | MEDIUM |

## Unused Code Analysis

### Potentially Unused Items

| Type | Item | Location | Confidence |
|------|------|----------|------------|
| Function | `deprecated_helper` | src/utils.py:89 | HIGH |
| Import | `unused_module` | src/main.py:5 | HIGH |
| Variable | `old_config` | src/config.py:23 | MEDIUM |

## Duplicate Code Analysis

### Duplicate Blocks Found

| ID | Locations | Lines | Similarity | Recommendation |
|----|-----------|-------|------------|----------------|
| DUP1 | src/a.py:10-25, src/b.py:45-60 | 15 | 100% | Extract to shared function |

## Recommendations

### Immediate Actions (CRITICAL)

1. **[Issue ID]**: [Brief description and fix]
2. **[Issue ID]**: [Brief description and fix]

### High Priority (This Sprint)

1. **[Issue ID]**: [Description and approach]

### Medium Priority (Next Sprint)

1. **[Issue ID]**: [Description]

### Low Priority (Backlog)

1. **[Issue ID]**: [Description]

## Comparative Analysis

[If previous audit exists, show trends]

| Metric | Previous | Current | Trend |
|--------|----------|---------|-------|
| Critical Issues | [X] | [Y] | [↑/↓/→] |
| Code Quality Score | [X]% | [Y]% | [Trend] |

## Next Steps

1. Address all CRITICAL issues before next deployment
2. Schedule HIGH priority items for current sprint
3. Add MEDIUM items to backlog
4. Re-run audit weekly to track progress

---

*Audit generated by devspark-site-audit v1.0*
*Constitution-driven codebase audit for [PROJECT_NAME]*
*Next audit recommended: [DATE + 7 days]*
*To re-run: `/devspark-site-audit` or `/devspark-site-audit --scope=constitution`*
```

### 13. Output Summary to User

Display concise summary:

```
✅ Site Audit Complete!

📄 Report saved: /.devspark.work/audit/YYYY-MM-DD_results.md
📅 Audit date: {DATETIME}
🎯 Scope: {SCOPE}

Health Summary:
- 🔴 {COUNT} Critical issues
- 🟠 {COUNT} High priority
- 🟡 {COUNT} Medium priority
- 🔵 {COUNT} Low priority

Constitution Compliance: {X}%
Overall Health: {HEALTHY/NEEDS ATTENTION/CRITICAL}

{If critical issues:}
⚠️ Critical issues require immediate attention:
- {ID}: {Brief description}

View full report: /.devspark.work/audit/YYYY-MM-DD_results.md
```

## Guidelines

### Constitution Authority

The constitution is **non-negotiable** and the **authoritative source** for all audit criteria.

All findings must:
- Reference the specific constitution section (by principle name)
- Quote the exact constitution language (MUST/SHOULD/etc.)
- Explain how the code violates the principle
- Use the constitution's own terminology

### Evidence-Based Findings

Every issue must include:
- **Specific location**: File path and line number
- **Code evidence**: Actual code snippet showing the issue
- **Constitution reference**: Which principle is violated
- **Actionable fix**: Specific remediation with example

### Audit Objectivity

- Focus on facts, not opinions
- Base all findings on constitution principles
- Avoid subjective language
- If not in constitution, classify as LOW or skip

### Graceful Error Handling

**If constitution missing**:
```
❌ Cannot perform site audit - Constitution required

The project constitution defines audit criteria. Create one first:

1. Run: /devspark-constitution
2. Define your project's core principles
3. Then retry: /devspark-site-audit

Learn more: https://dev.azure.com/bswdev/HealthSource/_git/bsw.devspark
```

**If no issues found**:
```
✅ Site Audit Complete - No Issues Found!

Your codebase is fully compliant with the project constitution.

Constitution Compliance: 100%
Overall Health: HEALTHY

Keep up the great work! 🎉

Report saved: /.devspark.work/audit/YYYY-MM-DD_results.md
```

### Historical Comparison

When previous audits exist:
- Load most recent audit from `/.devspark.work/audit/`
- Compare issue counts by severity
- Show improvement/regression trends
- Highlight newly introduced vs. fixed issues

## Context

$ARGUMENTS
````
