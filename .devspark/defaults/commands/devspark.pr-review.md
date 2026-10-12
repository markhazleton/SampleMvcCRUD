---
source: "BSW.DevSpark — © 2026 Baylor Scott & White Health. Source: https://bsw-devspark.bswhive.com"
description: Perform constitution-aware pull request review with actionable feedback for any PR in the repository
handoffs:
  - label: Address Current Findings
    agent: devspark-pr-review
    prompt: Address the current unresolved review findings.
scripts:
  sh: .devspark/scripts/bash/get-pr-context.sh $ARGUMENTS --json
  ps: .devspark/scripts/powershell/get-pr-context.ps1 $ARGUMENTS -Json
---

## User Input

```text
$ARGUMENTS
```

You **MUST** consider the user input before proceeding (if not empty).

## Overview

Load and obey the shared preamble contract core at `/.devspark/templates/command-preamble-contract.md` (installed repos) or `templates/command-preamble-contract.md` (source repos) before step 1 — in particular §9 (Genuine Fix Discipline) when reviewing a PR that claims to resolve a lint/test/code-smell finding. Preamble modules: `finding-fix`, `convergence`, `provenance`, `configuration`.

This command reviews the durable Pull Request delta against current code, tests, `.knowledge/`,
published documentation, and the constitution. It works for any PR regardless of how the change was
planned. One replaceable working report is stored at `/.devspark.work/pr-review/pr-{id}.md`; Git and
the hosting platform are the sole review history.

**IMPORTANT**: This command **only provides suggestions** - it does not make any code changes.

Reviews are advisory. The agent must explain constitution or lifecycle issues, recommend a disposition, and let the human decide the next action.

Planning artifacts are neither required nor valid review evidence. Missing plans are normal. Do not
recover, recreate, inspect, or score them; review the resulting delta.

## Prerequisites

- Project constitution at `/.knowledge/governance/constitution.md` (REQUIRED)
- A hosted repository with PR context (GitHub or Azure DevOps)
- Platform CLI installed and authenticated — `gh` for GitHub, `az` (with the `azure-devops` extension) for Azure DevOps (required). The host is resolved automatically from `.devspark.work/devspark.json` → `platform` or the `origin` remote; `{SCRIPT}`/get-pr-context handle PR detection per platform.
- **HARD RULE — Branch Sync**: The source (head) branch **MUST** be fully in sync with the target (base) branch. Do **NOT** proceed with review or approval if the source branch is behind the target. Instruct the user to rebase or merge the target branch into the source branch first.

## Definition of Done

Done when: the report is written to `/.devspark.work/pr-review/pr-{PR_NUMBER}.md` (step 9) and the step-10 chat summary is printed. The execution limits in §1 (max 20 findings, max 25 files, "stop once evidence is sufficient") are the convergence condition for the analysis itself — don't expand scope beyond them speculatively. Chat output is the step-10 template only; the full report (all tables, all sections) lives in the file — don't re-paste it into chat.

## Outline

Multi-app scope and the 2-tier override check for scripts are defined by the shared preamble contract already loaded above.

### 1. Initialize Review Context

Run `{SCRIPT}` to extract PR context and parse JSON output for:

- `PR_CONTEXT`: PR metadata (number, title, branches, commit SHA, files, diff)
- `CONSTITUTION_PATH`: Path to constitution file
- `REVIEW_DIR`: Directory where review will be saved

Treat script JSON as bounded context:

- `files_changed` may be sampled for large PRs.
- Use `files_changed_total` and `files_changed_truncated` to decide whether to zoom in further.
- Do not expand to all files unless user explicitly requests a full exhaustive review.
- `unmapped_files` (changed files matching no `.knowledge/` node's `appliesTo` pattern) is a
  computed hint for §6 `PRD5`, not itself a finding — see that step for how to use it.

Execution limits (required):

- Max findings in report: 20 highest-signal items
- Max files to inspect deeply: 25
- Max follow-up searches/reads beyond provided context: 8
- Stop once evidence is sufficient for high-confidence conclusions
- If confidence is low for a specific area, ask one clarifying question
- `PR_CONTEXT.risk` carries the per-file tiers, rating, and `report_form`; when `report_form` is
  `short`, spend no more than P0/P1 reads and skip additional-dimension scans (§5) that no risk
  flag or finding calls for
**Iteration Detection**:
Before running a full review, check whether the user's input contains iteration keywords: "update", "re-review", "check my fixes", "revised", "changes made", "addressed". If detected:

1. Load the existing review file at `/.devspark.work/pr-review/pr-{PR_NUMBER}.md`
2. Extract all finding IDs and their last-known status
3. Diff only what changed since the last reviewed commit SHA
4. Carry forward all unresolved findings unchanged
5. Mark previously flagged findings as `✅ Resolved`, `⚠️ Partially Addressed`, or `❌ Still Present` based on the new diff
6. Append new findings (if any) introduced by the new commits
7. If a re-review determines an earlier finding fails the Finding Legitimacy Check (§4C), mark it `🚫 Withdrawn` with a one-line reason — never delete the row. The audit trail must show the question was asked and answered, not erased.
8. Update the Revision Log section with the new commit SHA and pass/fail counts
9. **Co-mingling check**: Detect whether the review file (`.devspark.work/pr-review/pr-{PR_NUMBER}.md`) was committed in the same commit as production code changes. Compare commit SHAs from `git log main...{source_branch} --oneline -- .devspark.work/pr-review/pr-{PR_NUMBER}.md` against `git log main...{source_branch} --oneline -- {app_path}/`. If any SHA appears in both outputs, flag it as an M-NN finding: "Review file and production code changes committed together — iteration diff may be polluted. Commit review updates separately from code fixes."

**PR Number Detection**:
The script will try to determine PR number in this order:

1. User explicitly provides PR number in arguments: `#123` or `123`
2. GitHub environment variables: `GITHUB_PR_NUMBER`, `PR_NUMBER`
3. Current branch PR detection via `gh pr view`
4. If unable to detect, the script will error with clear instructions

**Error Handling**:
If the script fails:

- **Constitution missing**: Guide user to run `/devspark-constitution`
- **GitHub CLI not installed**: Provide installation instructions
- **PR not found**: Ask user to verify PR number and `gh auth status`
- **No PR number**: Ask user to provide PR number explicitly
- **Branch out of sync** (`is_behind_target: true`): **STOP immediately.** Do not review or approve. Inform the user:
  > "BLOCKED: The source branch `{source_branch}` is behind target branch `{target_branch}`. Sync it first before this PR can be reviewed or approved."
  > Suggested fix: `gh pr update-branch {PR_NUMBER}` or `git fetch origin && git rebase origin/{target_branch}`

For single quotes in args like "I'm reviewing", use escape syntax: e.g 'I'\''m reviewing' (or double-quote if possible: "I'm reviewing").

### 1b. Delta Boundary Classification

Classify the PR only by changed durable paths and risk. Identify production code, tests,
`.knowledge/`, infrastructure, generated files, and temporary `.devspark.work/`
artifacts. A PR's own `.devspark.work/` planning bundle (the spec/plan/tasks/gates for the feature
this PR delivers) is expected to ride in the diff — `command-preamble-contract.md` §0 documents
that the bundle stays live under `.devspark.work/` through merge and is archived only later, by
`/devspark-release`. Do not flag that as a scope finding, and never use its presence or absence to
adjust trust. Only flag `.devspark.work/` content that is genuinely unrelated to this PR's own
feature (e.g. an unrelated bundle's files caught up by an errant `git add`, or files whose
`spec_path`/feature id doesn't match this PR's source branch) as a scope finding.

### 2. Load Constitution

Read and parse `/.knowledge/governance/constitution.md`:

- Extract all core principles with their names
- Identify MUST requirements (non-negotiable/mandatory)
- Identify SHOULD requirements (recommended)
- Note constitution version and amendment date
- If `.knowledge/governance/severity-registry.md` exists, load it and use its `§{section}.{LEVEL}` entries to validate finding codes when emitting findings
- Build a checklist of principles to evaluate

If constitution doesn't exist:

- **STOP** and inform user that constitution is required
- Provide guidance: "Run `/devspark-constitution` to create project principles first"
- Do not proceed with review

### 3. Analyze PR Changes

Using the PR_CONTEXT data from the script:

#### A. Risk-Tiered File Triage

The script has already tiered every changed file. Use `PR_CONTEXT.risk` as the source of truth;
do not re-derive tiers, flags, or the score by reading paths.

- `risk.files[]` — `path`, `tier` (P0–P3), `delta_class`, and (when available) `added`/`deleted`/`change`;
  `promoted_by` names the domain or integration entity (from `.knowledge/`) that made a file P0
- `risk.rating`, `risk.risk_score`, `risk.factors[]` — the auditable floor for risk; an
  `unmapped_production` factor means changed production code no knowledge node claims (a PRD5 lead)
- `risk.report_form` — `short`, `standard`, or `full` (see §7 Report Forms)
- `risk.tables.changed_files` and `risk.tables.test_files` — ready-made markdown, paste verbatim

Review depth by tier:

| Tier | Meaning | Review Depth |
|------|---------|--------------|
| **P0** | Auth, routes/handlers, middleware, data access, schema, infra | Full line-by-line review |
| **P1** | Business logic, domain models, integrations (the default) | Function-level review |
| **P2** | Tests, config, scripts, build files | Spot-check only |
| **P3** | Documentation, markdown, generated content | Scan for obvious errors |

Tiers are a floor. If a file the script placed in P1 is demonstrably critical (it gates safety, money,
access, or data integrity), review it as P0 and say so in the Executive Summary. Never lower a tier
or the rating. When `PR_CONTEXT.risk` is null, assign tiers yourself using the table above.

Per file:

- Read the diff at the depth its tier requires
- Identify the type of change (new file, modified, deleted) and its scope

Prioritization policy:

- Always start with P0 files regardless of context budget
- Do not read the diff of P3 files unless a finding needs it; the script's counts suffice
- If `files_changed_truncated` is true, limit P2 to a quick scan and focus on P0/P1
- Expand beyond sampled files only if necessary to validate a top-severity finding

#### B. Examine PR Diff

Parse the full diff to:

- Identify new functionality added
- Check for removed functionality
- Look for modified behavior
- Note refactoring vs. feature changes

**Take churn stats** from `PR_CONTEXT` (`lines_added`, `lines_deleted`) and `risk` and record them in the Stats section. Re-collect on every revision for trend tracking.

#### C. Review Commit Messages

- Check if commits follow conventions
- Identify the intent behind changes
- Look for breaking change indicators

### 4. Perform Constitution-Based Review

Start the review with a gate result block:

```yaml
gate: pr-review
devspark_version: "<installed version, or `unknown`>"
generated: "<ISO-8601 timestamp of this run>"
status: pass | warn | fail
blocking: true | false
severity: info | warning | error | showstopper
summary: "<concise outcome>"
```

For **each principle** in the constitution:

#### A. Compliance Check

- Review changed files against this specific principle
- Determine if the PR violates, partially complies, or fully complies
- Collect specific evidence (file names, line numbers, code snippets)

#### B. Severity Classification

Based on the principle's importance and violation type:

- **CRITICAL**: Violates mandatory (MUST) principle - blocks merge
- **HIGH**: Violates recommended (SHOULD) principle significantly
- **MEDIUM**: Partial compliance, improvement opportunity
- **LOW**: Style suggestions or minor improvements

For each finding:

- Quote the specific code that demonstrates the issue
- Reference the exact constitution section violated
- Explain clearly why this is an issue
- Provide a specific, actionable recommendation

#### C. Finding Legitimacy Check (required before any numbered finding)

Severity inflation is the reviewer-side mirror of a gamed fix (§9, Genuine Fix Discipline): both
swap a surface signal — a clean metric there, a superficially alarming pattern here — for the
behavioral question the check exists to answer. Failing a test relocates the finding to a
lower-friction form (see destinations); it does not authorize an inflated severity to stand.

A concern is only a **candidate** until it passes this check; only validated concerns become
numbered findings (internal reasoning: no artifact, no extra pass). A true observation is not
necessarily a defect.

**Basic legitimacy — every numbered finding, including Low.** It needs a concrete observation, a
violated expectation (requirement, invariant, contract, documented intent, or engineering
obligation), relevance to *this* PR, and a correction that belongs in this PR. Behavior that matches
approved intent, is merely surprising, or is unrelated inherited debt is not a defect: record it
under `### Observations` (§7) with no ID and no severity.

CRITICAL, HIGH, and MEDIUM findings must also pass the tests below (Test 5: behavioral claims only).

1. **Intent** — An approved, documented design decision is not a defect. If the PR context, the
   constitution, or `.knowledge/` shows the behavior was deliberately chosen and signed off (not
   merely unremarked), disagreeing with that decision is at most a `CON-NN` finding proposing a
   governance amendment, or no finding at all. A defect in *how* an approved intent was
   implemented is still fair game at full severity.
   - Standing example: a service refusing to start on incomplete configuration is the contract,
     not a risk to be flagged.
2. **Actionability** — The finding must name a concrete change to the diff. If the only available
   remediation is "confirm / verify / coordinate with / ensure before deploy," it is not a code
   finding — move it to the report's **Deployment Notes** section, not a Findings Detail table.
3. **Ownership** — If remediation is outside the PR author's control (DevOps sequencing, product
   sign-off, an upstream team's change), it is a handoff, not a finding against the author — move
   it to the report's **Handoff Notes** section.
4. **Self-consistency** — A row in the Behavioral Changes table marked `Intentional? Yes` cannot
   itself back a CRITICAL or HIGH finding. If the same behavior also has an implementation defect,
   name that defect explicitly and cite it, separately from the intentional-change row.
5. **Consumer trace** (behavioral concern at MEDIUM or higher) — Name the reader of the changed
   value and show that *this* instance can reach the claimed consequence, using the nearest
   evidence (consumer, caller, callee, helper, existing test, runtime condition, design note),
   including whatever nearby evidence would prove the concern wrong. Not exhaustive tracing. If it
   disproves the concern, drop it (an Observation if useful) — do not keep a defeated concern at
   lower severity. If the consequence cannot be established, it is an unconfirmed risk: label it,
   never block on it, never rate it above Low. Uncertainty alone is not evidence of a defect.
6. **Attribution** — When the issue is not obviously introduced by this PR, say in prose whether
   it is *introduced* (normal finding), *exposed* (judge materiality), *pre-existing but material*
   (the PR depends on it, expands its risk, or leaves the change unverifiable — may block), or
   *pre-existing and unrelated* (never blocks; Observation or deferred work). Pre-existing is never
   automatically ignorable.

An item that fails Test 2 or 3 is not discarded — it is relocated so the question stays visible in
the report, just not as a finding against the author. An item that fails basic legitimacy or Test 1
is dropped, recorded as an Observation, or recast as `CON-NN`. If a finding of this kind was already
emitted in a prior revision, withdraw it per the Iteration Detection rule above rather than
deleting it.

#### D. Generate Findings

Create structured findings with **stable IDs** that persist across re-reviews. Use zero-padded identifiers so status changes across revisions instead of being deleted:

- **ID**: Stable identifier with zero-padded number. The prefix maps to severity tier:
  - `C-NN` = Critical (blocking)
  - `H-NN` = High priority
  - `M-NN` = Medium priority
  - `L-NN` = Low priority
  - `CON-NN` = Constitution (code may be correct; governance needs updating)
- **Status**: `🔴 Open` | `✅ Resolved` | `⚠️ Partial` | `➡️ Carried` (for re-reviews) | `🚫 Withdrawn` (failed the Finding Legitimacy Check after being emitted; kept with a one-line reason, never deleted)
- **Principle**: Name of constitution principle
- **File:Line**: Exact location in code
- **Issue**: Specific description of the problem, including broken code snippet for CRITICAL/HIGH
- **Fix**: Concrete code fix for CRITICAL/HIGH findings (required); recommendation for others.
  State the defect, then the required outcome, then the least-complex sufficient correction first —
  a risk does not prescribe architecture. Discovery alone does not make work mandatory: classify
  what is found as a blocking defect, accepted limitation, deferred work, learning, or observation.

### 5. Additional Review Dimensions

#### Security Analysis

- **Hardcoded Secrets**: Scan for API keys, passwords, tokens in code
- **Input Validation**: Check if user inputs are validated
- **Authentication**: Review auth/authz changes for correctness
- **SQL Injection**: Look for unsafe database queries
- **XSS**: Check for unescaped output in web contexts
- **Dependency Vulnerabilities**: Note if new dependencies added

Create checklist:

- [ ] No hardcoded secrets or credentials
- [ ] Input validation present where needed
- [ ] Authentication/authorization checks appropriate
- [ ] No SQL injection vulnerabilities
- [ ] No XSS vulnerabilities
- [ ] Dependencies reviewed for vulnerabilities

#### Behavioral Regression Detection

Scan the diff for silent behavioral changes that may break callers or remove safety guarantees:

**Default Value Changes in Function Signatures**:

- Detect changed default parameter values in function/method definitions
- Flag when a previously-required argument gains a default (could mask missing args)
- Flag when a default changes in a way that silently alters behavior

**Return Type Changes**:

- Detect when a function's return type annotation changes (e.g., `str` → `Optional[str]`, `List` → `None`)
- Flag implicit return type changes where `None` may now be returned without a type annotation update
- Note downstream callers that may be broken

**Removed Defensive Code (4-Step Verification)**:
Before flagging a removed safety guard, complete all 4 steps:

1. **Identify the guard**: Quote the removed code and describe what it was protecting against
2. **Check if condition can still occur**: Determine whether the protected scenario is still possible based on what is observable in the PR diff and the PR context script output — do not speculate about runtime state not visible in the provided context
3. **Check if protection moved elsewhere**: Search the PR diff for equivalent protection at a different layer (middleware, validator, caller site)
4. **Flag only if genuinely removed**: Only create a finding if steps 2 and 3 confirm the protection is missing and the risk remains real

Include behavioral regression findings in the Critical or High section as appropriate. A behavioral
finding at MEDIUM or higher must also pass §4C Test 5 (consumer trace).

If constitution has code quality principles:

- **Naming Conventions**: Verify names follow standards
- **Code Organization**: Check structure matches guidelines
- **Error Handling**: Review exception handling patterns
- **Duplication**: Identify code duplication issues
- **Complexity**: Note overly complex code

**Gamed-fix detection** (command-preamble-contract.md §9): when a diff claims to resolve a lint rule, complexity metric, or test failure, verify the fix addresses the underlying behavioral intent, not just the mechanical check — a published eval measured genuine-fix rates as low as 5.6% for fixes that only reacted to the bare metric. Specifically:

- **Complexity "fixes"**: if branches were merely moved into new helper functions without reducing total branching or separating real responsibilities, flag it — the tool's number went down but the function still does more than one thing.
- **Exception-handling "fixes"**: if an `except` clause was narrowed (e.g., `except Exception` → `except ValueError`) but the handler still silently discards the error instead of naming and surfacing it, flag it — the check passes but the failure is still swallowed.
- Any fix that a reviewer would describe as "satisfies the check without changing behavior" is a gamed fix: raise it as an `H-NN` finding ("resolves the check, not the root cause"), not a style nit.

Identify:

- **Strengths**: What the PR does well
- **Areas for Improvement**: Specific suggestions

#### Testing Validation

**Mandatory test execution** (default behavior — opt out only if constitution explicitly marks test execution as impractical):

1. **Detect the test command**: Identify the project's test runner from project files (`pytest.ini`, `pyproject.toml`, `package.json`, `go.mod`, etc.). If re-reviewing, use the command recorded in the first Revision Log row to maintain a consistent baseline.
2. **Scope to changed test files**: Run the test suite scoped to test files changed in this PR. Only run the full suite if a scoped run is not possible.
3. **Record result in Revision Log**: Write the test command and pass/fail result into the Revision Log row for this review.
4. **Test failures are automatic HIGH findings**: If tests fail, create an H-NN finding citing the failure output. Do not classify test failures as MEDIUM or lower.

Additionally:

- Check if tests exist for new/modified code
- Verify test quality and coverage
- Confirm tests were written appropriately
- Review test naming and organization

**Planned Tests delta** (when the spec bundle has `qa-acceptance.md`): verify proportionately that each planned `update` or `retire` was applied, from the source diff for repository-owned tests or from the durable reference for externally owned ones. Do not require a new permanent test for a `none` disposition. Never execute manual QA, require deployment, invent scenarios, or convert observations into current-spec scope.

#### Documentation Review

If constitution requires documentation:

- Check if README updated if needed
- Verify code comments for complex logic
- Confirm API documentation updated
- Check if CHANGELOG updated

### 6. Durable Delta Consistency Validation

Validate the changed behavior, affected tests, and current knowledge together. This is a **strong,
blocking gate** — the primary control point on what reaches main — not an advisory note. Every item
below produces a stable `PRD{N}-{seq}` finding (e.g. `PRD1-01`), tracked in its own Findings
Detail table (§7) exactly like Critical/High constitution findings, and every open one belongs in
the Immediate Actions (Blocking) list. (`PRD` = PR Delta, distinct from `/devspark-site-audit`'s
repo-wide `DELTA{N}`/`KNOW{N}` IDs — same class of check, diff-scoped instead of repo-wide, kept
separately named so the two commands' findings are never ambiguous when read side by side.)

1. Identify the truth nodes this diff bears on with `explain-context.py --files <changed files>`
   (`.devspark/scripts/bash/explain-context.sh` or `.devspark/scripts/powershell/explain-context.ps1`
   in installed repos; read-only). It reports, per file, each node that applies to it or cites it
   through `source_of_truth`, and the unmapped files. The owners to inspect are nodes the PR touched,
   nodes citing a changed source, and nodes applying to a changed surface. Do not read the
   Knowledge corpus; read only what a claim check below needs.
   - **Claim-targeted check.** For each owner, find the material claims describing the changed
     mechanism — especially enumerable contracts: operators, enums, state values, configuration
     keys, public fields, status codes, allowed values, defaults, endpoint behaviors, invariant
     tables. When the diff adds or changes a member of such a set, take one existing sibling and
     the new member and search each owner with a **word-bounded** match (substring matching lets
     `field_eq` hit `field_equals`). An owner that lists the siblings but omits the new member names
     a candidate section: read it and compare the claim with the implementation. A search hit alone
     proves nothing; a promoted `PRD1` rests on a claim you read.
   - **`last_verified`** is evidence that someone claims the document was reviewed, not proof that
     every claim is correct. A bump on a node citing a changed source, whose changed hunks do not
     address the changed mechanism, triggers the claim check — it is not itself a defect.
   - **No unsupported "matches".** Do not write "Knowledge matches", "documentation is adequate",
     "PRD1: none", or "operator tables match" because the node was touched, is freshly dated,
     exists, or its changed hunk looked right. For a Code ↔ Knowledge surface this PR materially
     changes, that conclusion must rest on claims you actually compared (the report need not list
     every search).
2. **`PRD1` — Code/knowledge contradiction (BLOCKING).** The diff's code or configuration
   contradicts what an existing `.knowledge/` node (flat doc or entity) claims about that behavior.
   It needs a material claim you read, that contradicts the changed implementation or required
   behavior, and that is relevant to this PR; a stale-looking node about an untouched mechanism is
   not `PRD1`. Mechanical freshness (generated-artifact validity, stale entities) belongs to
   `/devspark-create-pr` and §6c; `PRD1` is semantic truth.
   Fix is either: correct the code to match an intentional, still-valid claim, or correct the
   knowledge node in the same PR to match the new, intentional behavior — never merge with the two
   disagreeing.
3. **`PRD2` — Unsupported knowledge claim (BLOCKING).** A `.knowledge/` node touched by this diff
   (new or edited) claims behavior the diff's own code and tests do not actually support — an
   aspirational or inaccurate claim introduced by this PR itself, distinct from `PRD1` (existing
   code vs. existing knowledge) because the claim is new.
4. **`PRD3` — Insufficient test depth (BLOCKING, severity by risk).** Changed durable behavior
   lacks executable coverage proportionate to its risk. Floor severity MEDIUM; escalate to HIGH for
   security, data-integrity, or public-API surface changes. Proportionate means relative to the
   changed behavior and to existing coverage at the layer that carries it: does the PR introduce
   externally observable behavior, is it already covered where a directly tested helper owns it, are
   the changed call sites simple delegation (which does not require duplicated caller tests), is a
   contract-level assertion needed to establish the actual change, or is the PR merely passing
   through unrelated historical test debt (an Observation, §4C Test 6)? `PRD3` blocks only when the
   changed behavior lacks coverage at a layer proportionate to its acceptance risk.
5. **`PRD4` — Planning-identifier leak (BLOCKING).** Durable code, tests, or
   `.knowledge/` in this diff references a spec ID, task ID, `FR-###`, plan link, gate file, or a
   path into `.devspark.work/` or `.archive/` — an archived path is just as forbidden as a live one,
   since it still points at ephemeral state. Rewrite as self-contained current truth. The inverse
   direction (an ephemeral artifact pointing at durable code/knowledge) is fine by construction and
   is not flagged.
6. **`PRD5` — Knowledge completeness for this diff (BLOCKING for architecturally significant
   behavior, otherwise a non-blocking suggestion).** The diff introduces or materially changes
   durable behavior — a new module/integration/pattern, a changed contract, a new operational
   procedure — but touches **no** `.knowledge/` node at all: neither an existing one nor a new one.
   This is the completeness half of the gate (`PRD1`/`PRD2` only catch *contradictions* between
   what's touched; `PRD5` catches behavior that should have been captured in `.knowledge/` and
   simply wasn't). The preflight JSON's `unmapped_files` list (changed files matching no node's
   `appliesTo` pattern) is a computed starting hint for this judgment — it is not itself a
   finding, since plenty of unmapped files are routine (tests, config, generated output); use it to
   focus attention, then judge significance conservatively — do not flag routine internal
   refactors, bugfixes with no behavior-contract change, or test-only diffs.
7. Confirm `.knowledge/guides/` describes only the current supported behavior (non-blocking unless a
   published page now makes a false claim, which escalates to `PRD1`).
8. Report every `PRD{N}` with direct evidence (file:line, the specific contradiction or gap) and a
   concrete repair — never a vague "knowledge may be out of date."
9. Base approval only on the durable delta, validation evidence, constitution, and unresolved
   findings — including every open `PRD{N}` finding. Missing planning artifacts are expected and
   must never substitute for or excuse an open `PRD{N}` finding.

### 6b. PR Scope Validation (Multi-App Mode)

If the repository operates in multi-app mode (`.devspark.work/devspark.json` exists with `mode: "multi-app"`), perform scope validation on the PR:

#### A. Check for Scope Declaration

Look for a PR scope declaration in the PR description. A scope declaration specifies:

- **mode**: `single-app`, `cross-app`, or `repo-scope`
- **primary_app**: The primary application being changed
- **affected_apps**: All applications intentionally touched by this PR

If no scope declaration is present, infer scope from the changed files:

- If all changed files belong to a single app (plus approved shared paths), infer `single-app` mode.
- If changed files span multiple apps, infer `cross-app` mode.
- If changes are purely in shared/repo-level paths, infer `repo-scope`.

#### B. Validate Scope Against Changed Paths

Using the PR's changed file list and the registry from `.devspark.work/devspark.registry.json`:

1. Map each changed file to its owning application (by matching `app.path` prefixes).
2. Identify shared paths (`.knowledge/`, `.github/`, `.devspark/`, root-level config files).
3. Validate that the changed paths are consistent with the declared (or inferred) scope:
   - **single-app**: Only the declared app's path and approved shared paths should be touched. Flag files in other apps as scope mismatches.
   - **cross-app**: All touched app paths must be listed in `affected_apps`. Flag undeclared app paths.
   - **repo-scope**: All paths are allowed.

#### C. Report Scope Findings

Include scope validation results in the review output:

- If scope is valid, note it in the Executive Summary as a passing check.
- If scope mismatches are detected, report them as **HIGH** severity findings:
  - List which files violate the declared scope.
  - Recommend updating the scope declaration or splitting the PR.
- Add a row to the Constitution Alignment table for scope compliance when scope is partial or failing.

### 6c. Ontology Gap-Report Gate & Evidence/Contradiction Warnings

For every `.knowledge/entities/<id>/` this PR's changed paths map to (via `appliesTo` or direct path
under `entities/<id>/`):

1. Confirm `git rev-parse HEAD` equals the PR's commit SHA from `PR_CONTEXT`. If it does not, check
   out the PR head first or stop and say so; never judge Knowledge against a different commit.
2. Run `scripts/build_knowledge_index.py --repo-root . --check` (or the installed-repo path). It is
   read-only. Exit 1 is **blocking**: with `DRIFT:` the committed generated Knowledge does not match
   the committed HEAD; without it the Knowledge cannot build. After a passing check, read the
   committed `.knowledge/ontology/coverage.json` and the `stale_entities` list in
   `.knowledge/index.json` for those entities only — do not evaluate entities the diff doesn't touch.
   This check is independent of `/devspark-create-pr`, which runs it earlier; a later commit, a
   rebase, or a corrupted index can still fail here.
3. A gap-report failure on a touched entity (missing required layer doc, invalid `source_of_truth`,
   stale `last_verified`) is **blocking** — same tier as a constitution violation. This is the
   mechanical half of "nothing merges without pr-review": the generator's existing hard checks now
   gate the PR, not just a separate CI step.
4. Never regenerate Knowledge during review: do not run the builder in its default mode, and do not
   edit or overwrite `.knowledge/index.json` or `coverage.json`. Report the finding; the author
   regenerates and commits.
5. Scan touched entities' evidence entries for a code-only reference (`type: code`) with no adjacent
   note explaining why a test wasn't used. Missing rationale is a **non-blocking warning**, not a
   gate — the philosophy treats "which evidence type" as a quality/degree question, never existence.
6. Scan for contradiction candidates among graph-adjacent objects only (same entity, entities sharing
   a `constrains` decision, objects citing the same `source_of_truth`) — never brute-force
   pairwise comparison across the whole repo. Judging whether a candidate is a genuine contradiction
   or acceptable nuance is a human call; surface it as a **non-blocking warning** for review, always.

### 6d. Closeout Consumption (review the convergence decision, don't restart it)

If the PR's planning bundle contains `gates/closeout.md`, read it. Your job here is to judge whether
the **convergence decision is defensible** — not to rediscover the route's findings from zero, and
not to treat a classified finding as unfinished work. The contract governing that artifact is
`/.devspark/templates/closeout-contract.md` (or `templates/closeout-contract.md` in source repos);
§9 of it defines exactly this exchange.

| Closeout state | Response |
|---|---|
| An `unclassified` finding remains | **`PRD6` (BLOCKING)** — the route never converged |
| A `blocking-defect` finding is unresolved | **`PRD6` (BLOCKING)** |
| A `requirement` is `not-satisfied`, or an `invariant` is violated, but the finding was classified as anything other than `blocking-defect` | **`PRD6` (BLOCKING)** — laundering, per contract §3/§4 |
| `accepted-limitation` with a real `accepted_by` | OK — do not reopen |
| `deferred-work` with a `captured_as` pointing at a record that actually exists | OK — do not reopen |
| `learning` retaining original expectation, evidence, and revised understanding | OK — do not reopen |
| `decision: complete` whose counts contradict the `findings:` list | **`PRD6` (BLOCKING)** — the arithmetic must hold |

`PRD6 — Unconverged or indefensible closeout (BLOCKING)` joins the `PRD{N}` family from §6 and is
reported the same way: stable `PRD6-{seq}` id, Findings Detail row, Immediate Actions entry.

Two rules bound this check in the other direction:

1. **An improvement you noticed is not a reason to reopen the spec.** Capture it as a Recommended
   Improvement (non-blocking) or route it to `/devspark-specify`. The mere availability of better
   work is never a blocking finding — that is the expansion loop this gate exists to stop.
2. **A genuine defect still reopens the work.** If you find something that independently meets the
   Blocking Defect bar — a failed requirement, a violated invariant, a correctness defect, a
   regression introduced by this diff — raise it under its own code at its own severity. A
   documented classification does not immunize a real defect; it only immunizes a *disagreement of
   preference* with a classification the evidence supports.

**No closeout artifact at all** is a non-blocking note, not a finding — many routes (quickfix, small
quick-spec, pre-upgrade branches) legitimately never produced one. Judge the PR on its durable delta
as §6 already requires. Never require the author to go run a command to satisfy this section.

### 7. Generate Review Report

Create comprehensive report at `/.devspark.work/pr-review/pr-{PR_NUMBER}.md`:

#### Handle Existing Reviews

If file already exists:

1. Read the existing file
2. Extract the previous commit SHA from metadata
3. Compare with current commit SHA from PR_CONTEXT
4. **If commit SHA is the same**:
   - Replace the entire file with updated review
   - Keep the original review date, update "Last Updated" date
5. **If commit SHA is different (PR was updated)**:

  Load all existing finding IDs and statuses. Diff only what changed since the previous reviewed commit. Carry forward unresolved findings with status `➡️ Carried`, update findings resolved by new commits to `✅ Resolved`, and add new findings with `🔴 Open`. Replace the Revision Log with the current review snapshot; Git retains earlier versions.

#### Report Forms

The report's length follows risk, so clean low-risk PRs do not pay for the full form. Use
`PR_CONTEXT.risk.report_form` (`short` when the rating is low, `standard` for medium, `full` for
high; default to `standard` when absent). A re-review keeps the form of the first review unless the
rating rose.

| Section | short | standard | full |
|---------|-------|----------|------|
| Frontmatter, Revision Log, Stats, Executive Summary | yes | yes | yes |
| Action Items and Findings Detail | yes | yes | yes |
| Constitution Alignment (failures and partials only) | yes | yes | yes |
| Changed Files (script table) | no | yes | yes |
| Testing | one line in Executive Summary | yes | yes |
| Behavioral Changes, Security | only when something was found | only when something was found | only when something was found |
| Deployment & Handoff Notes | only when non-empty | only when non-empty | only when non-empty |

In every form, the Approval Decision is the Recommendation and one sentence of reasoning. A
section that has nothing to report is omitted, never written as "None found." — except
`Immediate Actions`, which states "None found." so a reader can see blocking was checked.

#### Machine-Readable Frontmatter

The report opens with a YAML frontmatter block so tools can read outcomes without parsing prose.
Keep it consistent with the body on every write:

- `devspark_version` is the version that produced **this** review (preamble contract §12). Copy
  the script's top-level `DEVSPARK_VERSION`; write `unknown` when absent, never guess. A re-review
  stamps the version that ran the re-review, so progress across versions stays comparable.
- `reviewed_commit` is the full SHA (no backticks, no truncation); `revision` matches the latest
  Revision Log row.
- `findings` and `max_severity` equal the counts in that latest Revision Log row, as raised in that
  revision (a finding fixed later in the same session still counts). `max_severity` is `none` when
  every count other than `con` is zero.
- `risk_rating` and `risk_score` come from `PR_CONTEXT.risk` when present, so predicted risk can be
  compared with the findings above. If `PR_CONTEXT.risk` is null, omit `risk_score` and set
  `risk_rating` to your own assessment.
- You may raise the rating above the supplied one when evidence warrants it, and must then set
  `risk_escalated: true` and state the reason in the Executive Summary. Never lower it.

#### Report Structure

Use this exact format:

```markdown
---
pr: [NUMBER]
devspark_version: "[DEVSPARK_VERSION from script output, or `unknown`]"
reviewed_commit: [FULL_COMMIT_SHA]
revision: [N]
findings:
  critical: [N]
  high: [N]
  medium: [N]
  low: [N]
  con: [N]
max_severity: [none|low|medium|high|critical]
risk_rating: [low|medium|high]
risk_score: [N]
risk_escalated: [true|false]
---

# Pull Request Review: [PR_TITLE]

- **PR**: #[NUMBER] — [HEAD_BRANCH] → [BASE_BRANCH] — [@AUTHOR] — [OPEN/CLOSED/MERGED]
- **Reviewed Commit**: [COMMIT_SHA]
- **Review Date**: [YYYY-MM-DD HH:MM:SS UTC] (last updated [YYYY-MM-DD HH:MM:SS UTC])
- **Constitution Version**: [VERSION from constitution]

## Revision Log

| Rev | Commit | Date | Critical | High | Medium | Low | CON | Test Command | Result |
|-----|--------|------|----------|------|--------|-----|-----|--------------|--------|
| 1 | [SHA_SHORT] | [DATE] | [N] | [N] | [N] | [N] | [N] | [pytest scoped / skip if opted-out] | [pass/fail/skipped] |

*Add a row for each re-review. Keep the same test command across all revisions to prevent flaky baselines.*

## Stats

| Metric | Value |
|--------|-------|
| Files changed | [COUNT] |
| Commits | [COUNT] |
| Lines | +[ADDITIONS] −[DELETIONS] |
| Tiers (P0/P1/P2/P3) | [from `risk.tier_counts`] |
| Risk | [rating] ([risk_score]) — [factor names from `risk.factors`] |

## Executive Summary

**Overall Assessment**: [1-2 sentences. State any risk escalation and why.]

- Constitution: [PASS/FAIL] ([X]/[Y] principles checked) · Durable delta: [PASS/FAIL] ([X] open PRD findings)
- Security: [X] issues · Quality: [X] recommendations · Testing: [PASS/FAIL/skipped (opted-out)] · Docs: [PASS/FAIL/N/A]
- Constitution Improvements: [X] CON findings

## Action Items

*All findings ordered by severity. CRITICAL and HIGH items include broken code and the fix.*

### Immediate Actions (Blocking — must resolve before merge)

[If none, write "None found."]

- [ ] **C-01** `path/file.ext:45` — [One-line description]
  - **Broken code**: `[code snippet]`
  - **Fix**: `[corrected code snippet]`
- [ ] **H-01** `path/file.ext:67` — [One-line description]
  - **Broken code**: `[code snippet]`
  - **Fix**: `[corrected code snippet]`
- [ ] **PRD1-01** `path/file.ext:12` — [Code/knowledge contradiction] — **Fix**: [correct the code or the knowledge node]
- [ ] **PRD5-01** `path/file.ext:34` — [New behavior with no corresponding `.knowledge/` update] — **Fix**: [create/update the entity or flat doc]

### Recommended Improvements

- [ ] **M-01** `path/file.ext:89` — [One-line description]
- [ ] **L-01** `path/file.ext:123` — [Optional improvement, one line, no code block]

### Constitution Improvements (Non-blocking — feed into `/devspark-evolve-constitution`)

- [ ] **CON-01** — [Constitution section that needs updating]

## Findings Detail

*Stable IDs persist across re-reviews. Status updates instead of deleting.*

### Critical Issues (Blocking)

[If none, write "None found."]

| ID | Status | Principle | File:Line | Issue | Fix |
|----|--------|-----------|-----------|-------|-----|
| C-01 | 🔴 Open | [Name] | path/file.ext:45 | [Specific violation] | [Specific fix] |

### High Priority Issues

[If none, write "None found."]

| ID | Status | Principle | File:Line | Issue | Fix |
|----|--------|-----------|-----------|-------|-----|
| H-01 | 🔴 Open | [Name] | path/file.ext:67 | [Issue description] | [Fix] |

### Durable Delta Consistency Findings (Blocking)

*Every open `PRD{N}` finding from §6 belongs here, tracked like Critical/High issues, and every
open row also appears in Immediate Actions above. Stable IDs persist across re-reviews.*

[If none, write "None found."]

| ID | Status | Type | File:Line | Issue | Fix |
|----|--------|------|-----------|-------|-----|
| PRD1-01 | 🔴 Open | Code/knowledge contradiction | path/file.ext:12 | [Specific contradiction] | [Specific fix] |
| PRD2-01 | 🔴 Open | Unsupported knowledge claim | .knowledge/entities/x/architecture.md | [Claim not supported by diff] | [Correct the claim or the code] |
| PRD3-01 | 🔴 Open | Insufficient test depth | path/file.ext:89 | [Behavior change lacking coverage] | [Add/extend test] |
| PRD4-01 | 🔴 Open | Planning-identifier leak | path/file.ext:5 | [Spec/task/plan reference in durable content] | [Rewrite as self-contained truth] |
| PRD5-01 | 🔴 Open | Knowledge completeness | path/module/ | [New/changed durable behavior with no `.knowledge/` touch] | [Create/update entity or flat doc] |

### Medium Priority Suggestions

[If none, write "None found."]

| ID | Status | Principle | File:Line | Issue | Recommendation |
|----|--------|-----------|-----------|-------|----------------|
| M-01 | 🔴 Open | [Name] | path/file.ext:89 | [Suggestion] | [Improvement] |

### Low Priority Improvements

*One row per finding, one line each. No code blocks. Omit the subsection when there are none.*

| ID | Status | File:Line | Issue and recommendation |
|----|--------|-----------|--------------------------|
| L-01 | 🔴 Open | path/file.ext:123 | [Minor suggestion and the improvement] |

### Constitution Improvements

*Findings where the code may be correct but the constitution needs updating. Feed these into `/devspark-evolve-constitution`.*

[If none, write "None found."]

| ID | Status | Section | Observation | Suggested Amendment |
|----|--------|---------|-------------|---------------------|
| CON-01 | 🔴 Open | §[Section] | [What the code does that is better than what the constitution prescribes] | [Suggested wording for the constitution] |

## Constitution Alignment Details

*List only principles that failed or partially passed. Summarize the rest in one line.*

**Passing**: [N] of [Y] principles (N/A: [M])

| Principle | Status | Evidence | Notes |
|-----------|--------|----------|-------|
| [Principle 2] | ❌ Fail | src/api.ts:45 | [Why it fails] |
| [Principle 3] | ⚠️ Partial | Multiple files | [Partial compliance details] |

## Security

*Include only when a security concern was found. Name the concern (secrets, input validation, authz, injection, XSS, dependencies) and the file; otherwise the Executive Summary count is enough.*

## Testing

**Status**: [ADEQUATE | INADEQUATE | N/A] — [one line, or the reason test execution was skipped per constitution opt-out]

[Paste `risk.tables.test_files` verbatim.] Removed tests, if any:

- `[test_name]` — **[Justified/Unjustified]**: [reason] → [finding ID if unjustified]

*Unjustified removals are MEDIUM findings.*

## Changed Files

[Paste `risk.tables.changed_files` verbatim. Do not rewrite it or add a Findings column.]

## Behavioral Changes

*Silent behavioral changes detectable from diff analysis. Callers may break without test failures.*

[If none detected, omit this section.]

| Change | Before | After | Intentional? | Risk |
|--------|--------|-------|-------------|------|
| `[function()]` [what changed] | [before value/type] | [after value/type] | [Yes (PR description) / Unclear] | [Impact on callers] |

*A row marked `Intentional? Yes` cannot itself back a CRITICAL/HIGH finding — Finding Legitimacy Check Test 4 (§4C).*

## Deployment & Handoff Notes

*Items that failed the Finding Legitimacy Check (§4C) — Actionability, Ownership, basic
legitimacy, or an unestablished consumer trace. These are visible questions the review raised, not
findings against the author — no finding ID, no severity, and they do not block merge on their own.*

### Deployment Notes

[If none, write "None."]

- [What must be confirmed/verified/coordinated before or during deploy, and who owns confirming it]

### Handoff Notes

[If none, write "None."]

- [Remediation area] — owned by [DevOps / Product / upstream team] — [what's needed from them]

### Observations

*Useful facts that are not defects: behavior matching approved intent, unrelated inherited debt,
unconfirmed risks (labeled unconfirmed), and deployment information that does not affect approval.
No ID, no severity, no remediation required in this PR, and never a reason for REQUEST CHANGES.*

[If none, write "None."]

- [Observation — and why it is not a finding]

## Approval Decision

**Recommendation**: [✅ APPROVE | ⚠️ REQUEST CHANGES | ❌ REJECT] — [one sentence tracing it to the blocking findings, or stating none]

*Approval depends only on the durable delta, validation evidence, constitution, and unresolved findings.*

---

*Re-review after fixes: `/devspark-pr-review #[PR_NUMBER] re-review`. Address findings with `/devspark-address-pr-review {PR_ID}`; commit the review file on its own.*

---

## Current Review State

Replace this report on re-review. Keep stable finding IDs and current resolution outcomes; do not
append prior report bodies. Git and the hosting platform retain prior review history.

```text
End of report template
```

### 8. Create Review Directory

Ensure `/.devspark.work/pr-review/` directory exists:

- Check if directory exists
- Create `/.devspark.work/pr-review/` if it does not exist
- Set appropriate permissions

### 9. Write Review File

Write the generated report to `/.devspark.work/pr-review/pr-{PR_NUMBER}.md`:

- Use UTF-8 encoding
- Ensure proper line endings
- Make file readable

**Validate before reporting.** Run
`python scripts/pr-review-validate.py <review file> --repo-root . --range origin/{target_branch}...origin/{source_branch}`
(installed repos: `.devspark/scripts/pr-review-validate.py`). It checks the frontmatter against the
body, the recommendation against open blocking findings, and that risk was not lowered. If it
reports issues, fix exactly those in the file and run it once more; do not re-review. If Python or
the script is unavailable, skip this step silently.

### 10. Output Summary to User

Display concise summary to the user:

```text
✅ PR Review Complete!

📄 Review saved: /.devspark.work/pr-review/pr-{NUMBER}.md
🔍 Reviewed commit: {COMMIT_SHA}
📅 Review date: {DATETIME}

Executive Summary:
- [Status emoji] {COUNT} Critical issues
- [Status emoji] {COUNT} High priority
- [Status emoji] {COUNT} Medium priority
- [Status emoji] {COUNT} Low priority
- 🏛️ {COUNT} Constitution improvements

Recommendation: {APPROVE/REQUEST CHANGES/REJECT}

{If critical issues:}
Critical issues must be resolved before merge:
- C-01: {Brief description}
- C-02: {Brief description}

View full review: /.devspark.work/pr-review/pr-{NUMBER}.md
```

## Guidelines

### Constitution Authority

The constitution is **non-negotiable** and the **authoritative source** for all review criteria.

All findings must:

- Reference the specific constitution section (by principle name)
- Quote the exact constitution language (MUST/SHOULD/etc.)
- Explain how the code violates or complies with the principle
- Use the constitution's own terminology and standards

### Evidence-Based Feedback

Every issue must include:

- **Specific location**: File path and line number (not "multiple files" or "various places")
- **Code quote**: Actual code snippet showing the issue (2-5 lines of context)
- **Constitution reference**: Which principle is violated and why
- **Actionable recommendation**: Specific fix with example if possible

**Bad example**: "Code has issues with naming"
**Good example**: "src/api.ts:45 - Variable `x` violates naming principle 'Use descriptive names'. Rename to `userApiKey`."

### Review Objectivity

- Focus on facts, not opinions or style preferences
- Base all feedback on constitution principles
- Avoid subjective language ("ugly code", "bad design")
- If not in constitution, don't flag it (or mark as LOW priority observation)

### Approval Semantics

- Observations do not imply REQUEST CHANGES.
- Every REQUEST CHANGES decision must trace to at least one validated blocking finding — several
  interesting concerns without a legitimate blocker are not a blocking verdict.
- APPROVE may coexist with Low observations, deferred work, accepted limitations, and nonblocking
  handoff notes.

### Severity Guidelines

Use this scenario-to-severity mapping table to anchor classification. Severity reflects the
demonstrated consequence, materiality, current-PR responsibility, and confidence in the validated
evidence. When two tiers are plausible, choose the one the evidence supports; uncertainty calls for
more validation or lower confidence, never automatic escalation. Projects may extend this table via
their constitution's anti-pattern appendix.

| Finding Type | Severity | Rationale |
|---|---|---|
| Runtime crash on production path | CRITICAL | Immediate user impact |
| Data corruption / silent data loss | CRITICAL | Breaks data contracts |
| Auth bypass / credential exposure | CRITICAL | Compliance + security |
| Schema violation (frozen fields) | CRITICAL | Pipeline breakage |
| Code, tests, and current knowledge contradict each other | HIGH | Durable truth is unreliable |
| Runtime error on edge path | HIGH | Affects subset of users |
| Silent behavior change (defaults, types) | HIGH | Invisible regression |
| API contract violation (wrong status codes) | HIGH | Breaks consumers |
| Broken test infrastructure | HIGH | Blocks developer workflow |
| Test suite failures on changed test files | HIGH | Runtime errors missed by diff |
| Missing tests for new code | MEDIUM | Tech debt, not production risk |
| Unjustified test removal | MEDIUM | Coverage regression |
| Dead code introduced in PR | MEDIUM | Maintenance burden |
| Performance inefficiency | MEDIUM | Latency, not correctness |
| Review file co-mingled with code fixes | MEDIUM | Pollutes iteration diff |
| Stale TODO referencing merged work | LOW | Clutter |
| Style / naming / docs | LOW | Optional improvement |
| Constitution needs updating (not code) | CON | Governance improvement |
| Finding fails the Finding Legitimacy Check but was emitted anyway | MEDIUM (HIGH if repeated after a prior withdrawal) | Severity inflation costs reviewer credibility — mirrors gamed-fix discipline from the reviewer's side |

#### Severity Code Format

Every finding that references a constitution principle MUST include a severity code in the
format `§{section}.{LEVEL}` matching an entry in `.knowledge/governance/severity-registry.md`.

**Examples**: `§VI.HIGH` (platform parity), `§VII.MEDIUM` (review file commit discipline),
`§VIII.HIGH` (markdownlint CI block), `§I.SHOWSTOPPER` (backward compatibility violation)

For findings not mapped to any constitution section (e.g., security observations, code-quality
issues not covered by the constitution): emit the finding without a `§` code and flag it as
a `CON` candidate for `/devspark-evolve-constitution`.

Summary tiers:

- **CRITICAL**: Violates MUST principle, blocks functionality, security risk, breaks production
- **HIGH**: Violates SHOULD principle significantly, quality concerns, technical debt
- **MEDIUM**: Partial compliance, improvement opportunity, maintainability concern
- **LOW**: Style preference, minor optimization, optional enhancement
- **CON**: Constitution needs updating — the code may be correct but governance is lagging behind

### Graceful Error Handling

**If constitution missing**:

```text
❌ Cannot perform PR review - Constitution required

The project constitution defines the review criteria. Create one first:

1. Run: /devspark-constitution
2. Define your project's core principles
3. Then retry: /devspark-pr-review #{PR_NUMBER}

Learn more: <https://dev.azure.com/bswdev/HealthSource/_git/bsw.devspark>
```

**If PR not found**:

```text
❌ PR #{NUMBER} not found

Troubleshooting:
1. Check the PR number is correct
2. Verify GitHub CLI authentication: gh auth status
3. Confirm you have repository access

If issue persists, provide PR number explicitly:
/devspark-pr-review #123
```

**If GitHub CLI not installed**:

```text
❌ GitHub CLI (gh) required but not installed

Install GitHub CLI:
- macOS: brew install gh
- Windows: winget install --id GitHub.cli
- Linux: use your distribution's package manager (e.g. apt install gh)

After installation, authenticate:
gh auth login

Then retry: /devspark-pr-review
```

### Positive Feedback

If PR is excellent:

- Acknowledge good practices
- Call out strengths specifically
- Provide enthusiastic approval
- Example: "Excellent PR! Comprehensive tests, clear documentation, follows all constitution principles. Strong work! ✅"

## Context

$ARGUMENTS

## Shared Review Resolution Contract Output

When emitting findings (review observations, issues, recommendations), structure each entry to include the shared resolution contract fields so downstream tools (/devspark-address-pr-review, telemetry, release) can act on them deterministically:

```yaml
findings:
  - finding_id: <stable-id-unique-within-this-command-output>   # e.g., analyze-001, clarify-002
    severity: critical | high | medium | low
    description: <1-3 sentence problem statement>
    recommended_action: <machine-actionable next step>
    execution_mode: auto | selective | manual
    status: open                                                  # set to `resolved` after remediation
    outcome: ""                                                  # populated post-resolution by address-pr-review
```

inding_id MUST be stable across re-runs when the underlying issue is unchanged. xecution_mode MUST be one of: `auto` (safe to apply automatically), `selective` (apply with reviewer approval), `manual` (requires human implementation). The `status` and `outcome` fields are written by `/devspark-address-pr-review` (FR-028).
