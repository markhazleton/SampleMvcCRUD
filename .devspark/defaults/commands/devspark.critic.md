---
source: "BSW.DevSpark — © 2026 Baylor Scott & White Health. Source: https://bsw-devspark.bswhive.com"
description: Challenge whether an internally sound plan is wrong about the real system it will run in — unimplementable conditions, absent operational controls, and unexamined assumptions about code, dependencies, and failure.
handoffs:
  - label: Fix Spec Showstoppers
    agent: devspark-specify
    prompt: Revise spec to resolve vague requirements, missing edge cases, or undefined trust boundaries
    send: true
  - label: Fix Critical Issues
    agent: devspark-plan
    prompt: Revise plan to address critical architectural risks
    send: true
  - label: Update Tasks
    agent: devspark-tasks
    prompt: Regenerate tasks with missing operational items
    send: true
scripts:
  sh: .devspark/scripts/bash/check-prerequisites.sh --json --include-tasks
  ps: .devspark/scripts/powershell/check-prerequisites.ps1 -Json -IncludeTasks
---

## User Input

```text
$ARGUMENTS
```

You **MUST** consider the user input before proceeding (if not empty).

## Overview

Load and obey the shared preamble contract core at `/.devspark/templates/command-preamble-contract.md` (installed repos) or `templates/command-preamble-contract.md` (source repos) before step 1. Preamble modules: `finding-fix`, `convergence`, `provenance`.

Load and obey the shared assurance contract at `/.devspark/templates/assurance-contract.md` (installed repos) or `templates/assurance-contract.md` (source repos). It is the canonical source for lane ownership, deduplication, finding quality, verdict derivation, bounded passes, and applicability. This command implements that contract; it does not restate it.

> **Critic exists to find the places where a coherent plan is wrong about the system it will run in — unimplementable conditions, absent operational controls, and unexamined assumptions about code, dependencies, and failure — early enough that the plan can still change cheaply.**

Critic is the **open-world** half of planning assurance. `/devspark-analyze` establishes that the artifacts are internally sound; Critic challenges that internally sound plan against repository and operating reality. The two are complementary, not two names for reviewing the work.

Critic **owns**: requirements resting on false assumptions about existing code; conditions the baseline can never produce; behavior removed as "redundant" that is a sole required path; hidden coupling to existing consumers; missing rollback/disable/kill-switch posture; missing observability needed to operate the proposed behavior; unexamined external-dependency behavior; persistent-data limits; state-machine failure modes; compatibility assumptions; concurrency and async hazards visible from the design; NFR achievability; `context_resolved` **sufficiency**; architectural assumptions unsupported by current source.

Critic does **NOT** own, and MUST NOT emit normal findings for (assurance-contract.md §4.1):

| Not Critic's | Owner |
| ------------ | ----- |
| Artifact consistency, duplication, terminology drift | `/devspark-analyze` |
| Requirement ↔ task coverage, traceability, task bookkeeping and completion state | `/devspark-analyze` |
| Missing task anchors; tasks citing requirement ids that do not exist | `/devspark-analyze` |
| Requirement *wording* precision (vague adjectives, placeholders) | `/devspark-analyze` |
| Rationale Summary completeness, spec↔plan Core-Problem drift | `/devspark-analyze` |
| Documentation drift | `/devspark-analyze` |
| `context_resolved` entries that simply fail to resolve (**validity**) | `/devspark-analyze` |
| Implementation correctness, runtime proof | `/devspark-verify` |
| Diff review, post-implementation code quality | `/devspark-pr-review` |

Noticing one of these anyway is normal. Route it under `routed_findings:` per assurance-contract.md §4.2 — never as a Critic risk finding.

### What this command is NOT

Critic findings are **hypotheses**, not proven defects. This command does **not** replace SAST, DAST, profiling, dependency scanning, code review, integration testing, or formal verification. A clean critic report means "no obvious traps in the artifacts," not "the system is safe." Teams must still run real testing.

Critic is **planning-time** assurance. It is not post-implementation code review, and it never reviews the work performed to resolve its own earlier findings (assurance-contract.md §3.2, §8).

## Operating Constraints

Non-destructive. Do **not** edit `spec.md`, `plan.md`, `tasks.md`, or source files. The only allowed write is refreshing the gate artifact at `FEATURE_DIR/gates/critic.md`.

Read YAML frontmatter from `spec.md` (or the quickfix record when `FEATURE_SPEC` resolves to `.devspark.work/quickfixes/<branch>.md`). Treat `classification`, `risk_level`, `risk_profile`, `change_type`, `archetype`, and `required_gates` as authoritative when present.

**Mindset**: Default — assume **limited team experience** with the stack, **optimistic estimates**, and **incomplete edge-case understanding**. If `/.knowledge/governance/constitution.md` declares `**Team Experience**: expert` in its Governance section, calibrate instead to: assume deep stack familiarity — de-emphasize beginner-trap findings (basic setup mistakes, well-known footguns already covered by linting/tests) and weight findings toward the sophisticated, cross-cutting risks a seasoned team is more likely to miss (subtle race conditions, cross-service contract drift, adversarial edge cases). `mixed` blends both: neither suppress nor over-index on beginner traps. Absence of the field, or any other value, means the default pessimistic mindset applies unchanged.

**Constitution Authority**: `/.knowledge/governance/constitution.md` is **non-negotiable**. Constitution violations are automatically SHOWSTOPPER.

## Outline

Multi-app scope and script resolution are defined by the shared preamble contract.

### 1. Initialize Analysis Context

**Planning Review hand-off.** When invoked by `/devspark-planning-review`, the invocation carries an already-resolved `FEATURE_DIR`, a `DEVSPARK_FEATURE` value, and a `PLANNING_REVIEW_RUN_ID`. Use them: do not re-resolve the feature from the branch name, run any script with `DEVSPARK_FEATURE` set to the handed-off value, and stamp the run id into the gate header as `planning_review.run_id`. Run the script below only when no hand-off was given.

Run `{SCRIPT}` once from repo root, parse JSON for FEATURE_DIR and AVAILABLE_DOCS. Derive:

- SPEC = FEATURE_DIR/spec.md
- PLAN = FEATURE_DIR/plan.md (optional)
- TASKS = FEATURE_DIR/tasks.md (optional)
- CONSTITUTION = /.knowledge/governance/constitution.md

**Tiered artifact requirements** — do NOT abort on missing plan/tasks; degrade gracefully:

| Artifacts present   | Critique scope                                                                   | Degradation label |
| ------------------- | -------------------------------------------------------------------------------- | ----------------- |
| quickfix record only (no `spec.md` for this branch) | Record-level: WHAT/WHY/HOW soundness, Affected Components blast radius, rollback risk, unresolved `## Gate Acknowledgements` | `QUICKFIX-ONLY`    |
| spec only           | Spec-level: rationale, requirement completeness, scope hazards, trust boundaries | `SPEC-ONLY`       |
| spec + plan         | Above + architecture, stack, data model, deployment risks                        | `SPEC+PLAN`       |
| spec + plan + tasks | Full critique including task sequencing & operational gaps                       | `FULL`            |

Abort only if `spec.md` is missing AND no quickfix record exists for this branch under `.devspark.work/quickfixes/` (FEATURE_SPEC resolves there automatically — see the shared preamble's script resolution). Print the degradation label in the Executive Summary so partial reviews aren't mistaken for full ones.

For single quotes in args (e.g. "I'm Groot"), use `'I'\''m Groot'` or double-quote.

### 2. Determine Applicability

Critic applies on the basis of **observable change shape**, per assurance-contract.md §11. Authored `risk_level` is NOT the run/skip switch — it may modulate depth once applicability is established, and nothing else.

Read the spec/plan/tasks and the touched surface, then test for these five shapes:

| Signal | Applies when the change… |
| ------ | ------------------------ |
| `persistent-data-shape` | changes the shape, meaning, or controlled vocabulary of persisted data |
| `cross-boundary-interaction` | adds or alters interaction with an external service or another subsystem |
| `state-lifecycle` | changes state-machine transitions, lifecycle stages, or terminal-path behavior |
| `nondeterministic-behavior` | depends on model, heuristic, or other nondeterministic output |
| `new-entry-point` | introduces a new publicly reachable entry point or write path |

At least one signal → `applicable: true`. None → `applicable: false`; typical negative controls are a pure symbol rename, a local mechanical refactor with no behavioral surface, a documentation-only change, or formatting.

`applicable: false` does **not** mean Critic refuses to run. When the developer invoked `/devspark-critic` explicitly, record `invoked: explicit`, state in one line that no change-shape signal was detected, and continue with a correspondingly narrow review rather than a full adversarial sweep.

Record the decision in the gate header exactly as assurance-contract.md §11.3 specifies — `applicable`, `signals`, `invoked`, and `reason` when not applicable. One line of reason; applicability is a routing decision, not another risk essay.

### 3. Detect Repository Archetype

Determine the repo archetype (drives which risk categories apply). Read in priority order:

1. `archetype` field in spec.md frontmatter (if present)
2. `archetype` or `platforms` declared in constitution
3. File-system signals (heuristics below)

Archetype values:
`web-service | library | cli | mobile-app | desktop-app | data-pipeline | ml-training | llm-application | infrastructure | embedded | browser-extension | game | monorepo | documentation-site`

**Heuristic signals** (non-exhaustive):

- `web-service`: Dockerfile + framework imports (FastAPI/Express/Spring/ASP.NET/Rails), `openapi.*`, k8s manifests
- `library`: `pyproject.toml` without `[project.scripts]`, npm `"main"`/`"exports"` without server entry, `Cargo.toml` `[lib]`, published-package metadata
- `cli`: `[project.scripts]`, `bin` in `package.json`, `Cargo.toml` `[[bin]]`, Cobra/Click/Typer imports
- `mobile-app`: `android/`, `ios/`, `pubspec.yaml`, `*.xcodeproj`, React Native/Expo, MAUI
- `desktop-app`: Electron, Tauri, WPF, Qt, GTK, Avalonia
- `data-pipeline`: Airflow/Dagster/Prefect/Luigi/dbt, Spark/Beam jobs, `dags/` dir
- `ml-training`: PyTorch/TF/JAX training loops, `train.py`, MLflow/W&B, notebooks under `experiments/`
- `llm-application`: prompt/template directories (`prompts/`, `templates/prompts/`), LLM SDK imports (`openai`, `langchain`, `pydantic-ai`, `semantic-kernel`, `litellm`), eval/judge/referee workflows, model-routing or inference orchestration code
- `infrastructure`: Terraform/Pulumi/Bicep/CloudFormation/Ansible without app code
- `embedded`: PlatformIO, Zephyr, ESP-IDF, `*.ino`, linker scripts, `no_std` Rust
- `browser-extension`: `manifest.json` with `manifest_version`
- `game`: Unity/Unreal/Godot project files
- `monorepo`: Nx/Turborepo/Lerna/pnpm workspaces/Bazel with ≥3 sibling packages
- `documentation-site`: MkDocs/Docusaurus/Hugo/Jekyll without runtime code

Print the detected archetype in the report header. If undetectable, default to `web-service` and record the inference in the header. Per assurance-contract.md §6.1, a missing or ambiguous archetype is **resolved and reported, never emitted as a finding**. If the ambiguity genuinely prevents meaningful review, say so in the Executive Summary and narrow the declared scope — that is a scope statement, not a defect.

### 4. Detect Technology Stack

From plan.md (or repo manifests if plan absent): Language, Framework, Storage, Infra/Runtime, Concurrency model. Used to load matching checklists in §8.

### 5. Detect Context Mode & Risk Profile

**Context mode** (drives emphasis): `greenfield | brownfield | migration | hotfix`

Read `change_type` from spec.md frontmatter. If missing, infer from file deltas (mostly-new = greenfield; modified existing = brownfield; "migrate"/"port"/"cutover" language = migration; tiny scope + bug ID = hotfix) and record the inference in the header — never as a finding.

| Mode       | Emphasis                                                                    |
| ---------- | --------------------------------------------------------------------------- |
| greenfield | Full adversarial baseline                                                   |
| brownfield | Regression risk, backward compat, blast radius, rollback ease               |
| migration  | Dual-write hazards, cutover, data parity, idempotent replay                 |
| hotfix     | Minimum-viable scrutiny: "fix without making it worse"; preserve invariants |

**Risk profile** (drives severity scaling): `experimental | internal | customer-facing | revenue-critical | safety-critical | regulated`

Read `risk_profile` from spec.md frontmatter; fall back to constitution; default to `internal` and record the inference in the header — never as a finding (assurance-contract.md §6.1).

`risk_level` and `risk_profile` scale severity only. Neither decides whether Critic runs; that is §2.

**Severity scaling rule** (deterministic). Each finding has a *base severity* from the category registry. Apply this shift to derive *effective severity*:

| Profile          | Shift                                                                  |
| ---------------- | ---------------------------------------------------------------------- |
| experimental     | −1 (CRITICAL → HIGH; HIGH → MEDIUM; floor at LOW)                      |
| internal         | 0                                                                      |
| customer-facing  | 0                                                                      |
| revenue-critical | +1 (HIGH → CRITICAL; CRITICAL → SHOWSTOPPER)                           |
| safety-critical  | +1 AND treated as constitution violation → SHOWSTOPPER floor           |
| regulated        | +1 for any finding touching data handling, audit, retention, or access |

If `risk_profile` is missing, default to `internal` and mark it `inferred` in the report header. Never emit a finding for the missing field.

Print mode and profile in the report header, each marked `declared` or `inferred`.

### 6. Load Artifacts with Risk Lens

Extract failure-prone elements (skip sections whose artifact is absent):

**spec.md**: unrealistic perf/scale targets; vague security requirements; third-party lock-in / API limits; missing edge cases; data-consistency naivety; undefined trust boundaries.

**plan.md**: bleeding-edge stack choices; over/under-engineering; persistent-storage access patterns without performance analysis; safe-release strategy gaps; testing strategy gaps.

**tasks.md**: hidden contention in parallel tasks; optimistic estimates; missing operational tasks (monitoring, alerting, backups, DR, runbooks). Task *sequencing and dependency validity* belongs to `/devspark-analyze` — route it, do not emit it.

**Current knowledge and tests**: identify affected `.knowledge/` nodes through `appliesTo` and verify
the plan includes changes to current truth and proportionate executable coverage. Treat omitted
knowledge updates for changed behavior as HIGH and omitted tests according to runtime risk. Never
create feature, requirement, task, decision, or rationale nodes from planning artifacts.

When `qa-acceptance.md` exists, challenge it as open-world evidence: a scenario that depends on internals rather than observable behavior, duplication of an existing durable test without justification, an append-only regression disposition where `update` or `retire` fits, coverage disproportionate to the risk, and evidence that would not actually prove the requirement it traces to. Trace validity is Analyze's lane; do not emit it here.

#### 6a. Baseline Evidence Pass (required before any claim about existing behavior)

Per assurance-contract.md §3.1, Critic MUST inspect the relevant existing code, tests, and `.knowledge/` before asserting anything about current system behavior. For each requirement that depends on the baseline behaving a particular way, locate the code that establishes that behavior and ask:

- Can the condition this requirement depends on **actually occur** in the current system, or does some existing path always prevent it?
- Is behavior being removed as "redundant" in fact the **sole** path that satisfies a requirement, guard, or safety case?
- Do existing consumers depend on the shape, timing, or vocabulary this change alters?
- Does the proposed design assume an interface, field, or guarantee that current source does not provide?

State findings as a contradiction between a plan statement and an inspected source fact, citing the file and symbol:

```text
Plan states:      <X> will be absent at this point.
Baseline proves:  <path:symbol> always populates <X>.
Therefore:        the condition the requirement depends on cannot occur.
```

A claim about the baseline with no cited evidence MUST NOT be emitted as a finding. If the baseline cannot be inspected, record it as a verification obligation (§9a) instead.

This pass reads the **baseline being modified**, never the implementation being written. If implementation of this route has materially begun, do not trace it — route observations about the new delta to `/devspark-verify` or `/devspark-pr-review` (assurance-contract.md §3.2).

The Plan's `expected_change_surface`, when present, is optional orientation for where to look first. It does not limit what you examine: a risk outside the listed paths is still yours to find.

**Context Resolution sufficiency** (judgment-based, not a hard stop — `/devspark-analyze` already
confirmed every entry *resolves*; this asks whether the resolved set is *complete*): does
`plan.md`'s `## Context Resolution` list look like it's missing something the delta obviously
touches — an entity or flat Knowledge document referenced in spec.md's requirements or data-model.md
with no corresponding `context_resolved` entry, a `governance/decisions/` doc whose `constrains` list
names an entity this delta changes but that decision was never resolved, or a relation one hop away
from a resolved item that looks directly relevant. Judge the set on what it covers, never on which
kinds it contains — a set made entirely of flat documents can be perfectly sufficient.

Emit a sufficiency finding only when the omission is **demonstrably material**: name the specific
missing item, and state the design consequence of having planned without it. "The list looks short",
"consider adding more context", or an omission with no stated consequence are not findings. When
useful, re-run Context Projection (`/devspark-context-projection --seed <id>`) against the same seeds
`/devspark-plan` used, purely as a diagnostic cross-check — its candidate list is a hint for this
judgment call, not a second authoritative source; an omitted candidate is never an automatic finding.
An empty section on a delta that clearly touches existing knowledge is itself a finding (severity per
§9). This is the natural place to ask "what did design time miss" — escalation past one hop during
`/devspark-implement` on this same feature is a signal this check should have caught the gap here.

### 7. Risk Category Registry (archetype-gated)

Apply only categories whose `archetypes` set includes the detected archetype (or `*`). Each category has a *base severity ceiling*; the §5 profile shift adjusts findings up or down.

| Category                    | Archetypes                                                               | Base ceiling |
| --------------------------- | ------------------------------------------------------------------------ | ------------ |
| `dependency_supply_chain`   | \*                                                                       | CRITICAL     |
| `testing_strategy`          | \*                                                                       | CRITICAL     |
| `secrets_handling`          | \*                                                                       | SHOWSTOPPER  |
| `trust_boundaries`          | \*                                                                       | SHOWSTOPPER  |
| `error_handling_resilience` | \*                                                                       | CRITICAL     |
| `concurrency_async`         | web-service, cli, desktop-app, mobile-app, data-pipeline, game, embedded | CRITICAL     |
| `scale_bottlenecks`         | web-service, data-pipeline, ml-training, llm-application                 | CRITICAL     |
| `auth_authz`                | web-service, mobile-app, desktop-app, browser-extension, infrastructure  | SHOWSTOPPER  |
| `input_validation`          | web-service, cli, mobile-app, desktop-app, browser-extension             | SHOWSTOPPER  |
| `observability`             | web-service, data-pipeline, infrastructure, mobile-app, llm-application  | CRITICAL     |
| `deployment_rollback`       | web-service, mobile-app, desktop-app, infrastructure, browser-extension  | CRITICAL     |
| `data_loss_continuity`      | web-service, data-pipeline, infrastructure, mobile-app, desktop-app      | SHOWSTOPPER  |
| `regulatory_privacy`        | web-service, mobile-app, data-pipeline, llm-application                  | SHOWSTOPPER  |
| `llm_eval_guardrails`       | llm-application                                                           | SHOWSTOPPER  |
| `llm_prompt_integrity`      | llm-application                                                           | SHOWSTOPPER  |
| `api_compatibility`         | library, cli                                                             | CRITICAL     |
| `cli_ergonomics`            | cli                                                                      | HIGH         |
| `mobile_platform`           | mobile-app                                                               | CRITICAL     |
| `pipeline_semantics`        | data-pipeline                                                            | SHOWSTOPPER  |
| `ml_reproducibility`        | ml-training                                                              | CRITICAL     |
| `iac_blast_radius`          | infrastructure                                                           | SHOWSTOPPER  |
| `embedded_safety`           | embedded                                                                 | SHOWSTOPPER  |
| `binary_size_perf`          | mobile-app, embedded, browser-extension, game                            | HIGH         |
| `offline_sync`              | mobile-app, desktop-app                                                  | CRITICAL     |
| `monorepo_coupling`         | monorepo                                                                 | HIGH         |

**Category cue sheets** (capability-level, stack-agnostic):

- `dependency_supply_chain`: unpinned versions; transitive vulns unmonitored; abandoned packages; license incompatibility; build reproducibility.
- `testing_strategy`: no failure-mode tests; missing fixtures; no contract tests at boundaries; no load/perf tests where scale matters.
- `secrets_handling`: hardcoded secrets; secrets in env without vault/KMS; no rotation; secrets in logs.
- `trust_boundaries`: enforcement on privileged operations missing or implicit; ambient authority; missing tenant isolation.
- `error_handling_resilience`: swallowed errors; non-idempotent retries; missing timeouts/backoff; no circuit-breaker equivalent where appropriate.
- `concurrency_async`: blocking I/O on async runtimes; pool exhaustion; race conditions; deadlocks; task/thread/goroutine leaks; resource cleanup paths.
- `scale_bottlenecks`: unbounded result sets (endpoints, queries, iterators, streams); N+1 access patterns; no caching on hot paths; persistent-storage access patterns without index/partition analysis; no horizontal-scaling path.
- `auth_authz`: trust boundary enforcement on privileged operations; rate limiting on auth surfaces; session/credential lifecycle.
- `input_validation`: untrusted input crossing trust boundaries unvalidated; injection vectors (SQL, command, template, deserialization); size/shape limits.
- `observability`: no structured logs/metrics/traces; no liveness/readiness signal appropriate to the runtime; no alert thresholds; no error tracking.
- `deployment_rollback`: no safe-release strategy for this artifact type; no migration plan; no rollback procedure; no staging/prod parity; no graceful shutdown. This concern stays CRITICAL and pre-merge: a `stage: post-deploy` deferral never waives a missing rollback or migration plan, and the plan itself must exist before merge.
- `data_loss_continuity`: no backup/restore; destructive ops without confirmation; missing transactional boundaries; no DR plan.
- `regulatory_privacy`: PII handling unspecified; retention/residency unaddressed; missing audit logging.
- `api_compatibility`: undocumented public surface; missing semver discipline; no deprecation policy; breaking-change detection absent in CI.
- `cli_ergonomics`: undefined exit codes; ignored signals (SIGINT/SIGTERM/SIGPIPE); stdout/stderr/stdin discipline; non-portable paths/encoding; no `--help`/`--version` contract.
- `mobile_platform`: battery / background-execution limits; store-policy compliance; OS-version fragmentation; permissions model; deep-link/intent handling.
- `pipeline_semantics`: idempotency; exactly-once vs at-least-once semantics; backfill strategy; schema evolution; late/out-of-order data; watermark policy.
- `ml_reproducibility`: seeds; env/data pinning; train/serve skew; data leakage; eval contamination; model/artifact versioning.
- `llm_eval_guardrails`: no eval-suite or holdout coverage for prompt/model changes; no judge/referee calibration; no pass/fail threshold defined for behavior-changing updates.
- `llm_prompt_integrity`: prompt-injection surfaces unmitigated; PHI/PII handling in prompts undefined; output-validator/guardrail absent; model/deployment blast radius unbounded; no cost-per-call regression watch.
- `iac_blast_radius`: drift detection; state file locking & backups; secret rotation; cost runaway; change scope review; destroy guards.
- `embedded_safety`: memory bounds; watchdog; OTA safety & rollback; power-loss recovery; real-time deadlines; flash wear.
- `binary_size_perf`: artifact size budgets; cold-start; startup memory; asset compression.
- `offline_sync`: conflict resolution; offline queue durability; clock skew; partial-sync recovery.
- `monorepo_coupling`: cross-package import violations; version skew; CI graph correctness; release coordination.

**Universal failure-mode lens** (apply if no specific cue fires):

`resource_leaks | error_swallowing | race_conditions | unbounded_growth | missing_timeouts | missing_input_validation | trust_boundary_violations | non_idempotent_retries | silent_data_corruption`

### 8. Load Stack/Archetype Checklists (Externalized)

Load all matching checklists from disk. Resolve each from `/.devspark/templates/risk-checklists/`
(installed repos) or `templates/risk-checklists/` (source repos), with
`.devspark.work/risk-checklists/` taking precedence over both when a team overrides a file of the
same name. Installed repos keep framework templates under `.devspark/templates/` — there is no
`risk-checklists/` directory at the `.devspark/` root, and anything found there is a legacy leftover:

- `{stack}.md` (e.g., `python-fastapi.md`, `node-express.md`, `go-gin.md`, `java-spring.md`, `dotnet-aspnet.md`)
- `{archetype}.md` (e.g., `library.md`, `cli.md`, `data-pipeline.md`, `llm-application.md`)

If none exist at any of those paths, derive risks from first principles using the universal
failure-mode lens above. Note in the report output: *"No stack/archetype checklists found — consider
seeding from prior critic runs."* Stock seeds ship with BSW.DevSpark; teams override by placing
files of the same name in `.devspark.work/risk-checklists/`.

Every named tool in a checklist must be paired with the underlying capability so the model can translate across ecosystems. Pattern:

> "[Capability needed] — e.g., [Tool A in stack X], [Tool B in stack Y], [Tool C in stack Z]"

Example: "Long-running work without a durable queue with retry semantics (e.g., Celery, RQ, Sidekiq, BullMQ, Hangfire)."

### 9. Severity Classification (base, before §5 shift)

- **SHOWSTOPPER**: production outage, data loss, security breach, safety incident, or constitution violation.
- **CRITICAL**: major user-facing issue or costly rework (no rollback, no observability, unbounded growth, broken backward compat for a published library).
- **HIGH**: technical debt or operational burden (missing structured logging, hardcoded configs, missing type checks).
- **MEDIUM**: development friction or minor issues.

Both base and effective severity must appear in each finding.

**Behavioral-intent phrasing (command-preamble-contract.md §9.1)**: when a finding cites a structural smell, complexity/length metric, or lint rule, phrase its `description` and `recommended_action` around the behavioral intent the metric is a proxy for — not the bare number — and populate the finding's `intent_cue`. A finding written as "cyclomatic complexity 11 > 5" invites a gamed fix (branches shuffled into helpers, count lowered, behavior unchanged); the same finding written as "this handler does more than one thing — extract the <X> responsibility" drives a genuine one. A published eval measured genuine-fix rates of ~5.6% for bare-metric phrasing vs. ~83% when intent was stated first.

### 9a. Consolidate, Qualify, and Bound

Run this pass over the candidate findings **before** writing the report. It is what separates a short list of material defects from a long list of plausible observations.

**Deduplicate across capabilities** (assurance-contract.md §5.1). Discard any candidate whose substantive concern is already explicitly recognized and appropriately handled by `spec.md`, `plan.md`, `tasks.md`, or an open `/devspark-analyze` finding. Reference it in narrative if useful; never mint a second id to restate it.

**Deduplicate within Critic** (assurance-contract.md §5.2). One root concern yields **one** finding. Do not serialize a root cause into a chain of consequences:

```text
WRONG   risk → telemetry consequence → fallback consequence → task consequence   (4 ids)
RIGHT   one finding: root cause, impact, evidence, required decision             (1 id)
```

Split only when consequences require **independent decisions** by different owners, and then record the shared `root_cause:` on each.

**Apply the quality bar** (assurance-contract.md §6). Every remaining finding must answer: what is wrong; why it is material to *this* change; what evidence supports it; why Critic owns it; and what decision, mitigation, or proof resolves it. If all five cannot be answered, do not emit it. Generic advice — "consider scalability", "ensure observability", "think about security" — fails by construction.

**Least-complex mitigation first** (Minimum Sufficient Change, command-preamble-contract.md §13).

> **A Critic finding identifies a risk. It does not prescribe architecture.**
>
> **Critic should prefer the least-complex mitigation capable of preserving the requirement or invariant.**

For every actionable finding, reason in this order before suggesting anything new: **Risk** → **least-complex viable mitigation** → **why it is or is not sufficient** → **additional machinery, only if the lighter option cannot preserve the requirement or invariant**. Prefer, lightest first: accept a bounded limitation; an operational procedure or human review; delete or simplify the behavior; reuse an existing capability; deterministic validation or a test; and only then new machinery (state, configuration, persistence, cache, endpoint, background process). Record the lightest viable option in `least_complex_mitigation` and its kind in `mitigation_kind`. Never say "risk, therefore add a mechanism". Correctness, security, safety, data integrity, and explicit contracts are not optional simplifications. Critic still does not decide scope: Judge does.

**Route what is not Critic's** (assurance-contract.md §4.2). Move any candidate in another capability's lane into `routed_findings:`. It does not count as a Critic finding and never affects the verdict.

**Convert uncertainty into proof, not into defects** (assurance-contract.md §7). When something cannot be known until execution, do not escalate it into a pseudo-defect. Record it as a verification obligation so `/devspark-verify` can settle it:

```yaml
verification_obligations:
  - assumption: <what is being assumed>
    why_material: <what breaks if it is false>
    proposed_proof: <the verify mode or test that would settle it>
```

**Respect the pass boundary** (assurance-contract.md §8). If `FEATURE_DIR/gates/critic.md` already exists for this route, this run is a **recheck**: confirm the disposition of the previous findings. Do not open a new open-ended discovery pass, and do not review the work performed to resolve the earlier findings. A new observation noticed during a recheck is classified under `closeout-contract.md` §4 and expands the route only if it hits one of the five expansion triggers in `closeout-contract.md` §2.

**Measure the run by material unique findings**, not by how much could plausibly be mentioned.

### 10. Produce Risk Assessment Report

Output Markdown with this structure (omit empty sections — do not emit empty headers):

````markdown
```yaml
gate: critic
devspark_version: "<installed version, or `unknown`>"
generated: "<ISO-8601 timestamp of this run>"
planning_review:
  run_id: "<PLANNING_REVIEW_RUN_ID from the hand-off; omit this block when not invoked by /devspark-planning-review>"
status: pass | warn | fail
blocking: true | false
severity: info | warning | error | showstopper
summary: "<concise outcome>"
reviewed_artifacts:
  # One entry per artifact this review actually read (spec.md, plan.md, tasks.md, etc.).
  # hash = `git hash-object <path>` (falls back to a sha256 of the file if git is unavailable).
  # Downstream gate consumers (e.g. /devspark-implement) recompute this hash to detect drift
  # since the review ran -- an open SHOWSTOPPER/CRITICAL against a changed artifact is STALE.
  - path: spec.md
    hash: "<git-hash-object-output>"
```

## Technical Risk Assessment

**Analysis Date:** [ISO timestamp]
**Scope:** [SPEC-ONLY | SPEC+PLAN | FULL]
**Pass:** [DISCOVERY | RECHECK]
**Applicable:** [true | false] — signals: [list, or "none"]; invoked: [automatic | explicit]
**Detected Archetype:** [archetype] (declared | inferred)
**Detected Stack:** [language] + [framework] + [storage]
**Context Mode:** [greenfield | brownfield | migration | hotfix] (declared | inferred)
**Risk Profile:** [experimental | internal | customer-facing | revenue-critical | safety-critical | regulated] (declared | inferred)
**Risk Posture:** [RED | YELLOW | GREEN]

Inferred values are reported here and never emitted as findings (assurance-contract.md §6.1).

### Executive Summary

[2–3 sentences: verdict + degradation note if not FULL]

### Findings (source of truth)

```yaml
findings:
  - finding_id: critic-001
    owner: critic
    category: <registry-category>
    archetype_applicable: true
    location: <spec.md#section | plan.md#L42 | tasks.md#T07>
    description: <1–3 sentences>
    evidence: <REQUIRED when this finding claims anything about current system behavior: the file and symbol inspected that establishes the claim, per assurance-contract.md §3.1. Empty string when the finding is purely about the proposed design.>
    root_cause: <short stable label naming the shared cause when this finding shares one with another finding or with a remediation, e.g. "unbounded-bulk-change"; empty string when no causal relationship is supported by evidence. Do not fabricate causal certainty — an empty value is legal and Judge does not depend on it.>
    intent_cue: <REQUIRED when this finding cites a structural smell, complexity/length metric, or lint rule: one sentence naming the behavioral intent the metric is a proxy for, per command-preamble-contract.md §9.1 (e.g. "this function should do one thing — what is it?"). Empty string otherwise.>
    base_severity: showstopper | critical | high | medium | low
    effective_severity: showstopper | critical | high | medium | low
    classification: blocking-defect | accepted-limitation | deferred-work | learning
    least_complex_mitigation: <the lightest viable mitigation that preserves the requirement or invariant, and why it is or is not sufficient; empty string only when the finding is purely informational>
    mitigation_kind: accept | operational-procedure | delete | simplify | reuse | validation | new-machinery
    recommended_action: <machine-actionable next step; names new machinery only when least_complex_mitigation explains why the lighter option is insufficient>
    execution_mode: auto | selective | manual
    status: open
    outcome: ""

# Observations belonging to another assurance capability (assurance-contract.md §4.2).
# These are NOT Critic findings: they never affect the verdict and are addressed by
# re-running the owning capability. Omit the block when empty.
routed_findings:
  - routed_id: critic-routed-001
    owner: analyze | verify | pr-review
    description: <what was noticed>
    rationale: <why that lane owns it>

# Assumptions that cannot be settled before execution (assurance-contract.md §7).
# These are obligations for /devspark-verify, not defects. Omit the block when empty.
verification_obligations:
  - assumption: <what is being assumed>
    why_material: <what breaks if it is false>
    proposed_proof: <the verify mode or test that would settle it>
```

`classification` uses the canonical convergence vocabulary from `closeout-contract.md` §4. Severity and blocking are **separate dimensions**: a SHOWSTOPPER finding may be an Accepted Limitation and not block; a MEDIUM finding that violates a required invariant blocks.

### Showstoppers

_(Render rows from findings where `effective_severity: showstopper`. Drop section if none.)_

| ID  | Category | Location | Risk | Likely Impact | Mitigation |
| --- | -------- | -------- | ---- | ------------- | ---------- |

### Critical

_(Render from `critical`. Drop if empty.)_

| ID  | Category | Location | Risk | Likely Impact | Action |
| --- | -------- | -------- | ---- | ------------- | ------ |

### High

_(Render from `high`. Drop if empty.)_

| ID  | Category | Location | Issue | Impact | Suggestion |
| --- | -------- | -------- | ----- | ------ | ---------- |

### Missing Critical Tasks

_(FULL scope only. Omit empty buckets. Documentation is **not** a Critic bucket — documentation
drift belongs to `/devspark-analyze`; route it.)_

- **Observability / Operations / Testing / Security:** […]

### Questionable Assumptions

1. **[Assumption]** → Failure mode: […]

### Dependency Risk Assessment

| Dependency | Concern | Alternative |
| ---------- | ------- | ----------- |

### Estimated Technical Debt at Launch

- **Code / Operational / Testing Debt:** […]

### Metrics

- Showstopper / Critical / High counts (effective severity)
- Material unique findings
- Findings by category
- Routed findings and verification obligations (counts only)
- Missing operational tasks (FULL scope only)

**VERDICT:** STOP | CONDITIONAL | PROCEED

The verdict is **derived**, never authored (assurance-contract.md §9):

- **STOP** — at least one open finding is classified `blocking-defect`.
- **CONDITIONAL** — no blocking defect, but at least one open finding needs a consequential human decision or authorization before implementation.
- **PROCEED** — neither of the above.

No other verdict word is permitted. `GO`, `PASS`, `MOSTLY READY`, and `PROCEED AFTER …` are contract
violations. `PROCEED` alongside an open blocking defect, or `blocking: false` alongside required
pre-implementation actions, is a self-contradiction — recompute before writing the gate artifact.

**Required Actions Before Implementation:**

1. […]

**Recommended Risk Mitigations:**

- […]
````

Every table row MUST correspond to a `finding_id` in the YAML `findings:` list — the YAML list is the single source of truth. Tables are projections, never additive. `routed_findings` and `verification_obligations` are never rendered as findings tables.

### 11. Persist Gate Artifact

Before writing the report, compute `reviewed_artifacts`: for each artifact actually read in this review (typically `spec.md`, and `plan.md`/`tasks.md` when scope is SPEC+PLAN or FULL), run `git hash-object <path>` and record the `path`/`hash` pair. This is a mechanical step (a hash, not a judgment) -- do not skip it even when findings are empty.

For each supported planning artifact, also call `scripts/judge.py semantic-hash --path <path>`
(installed: `.devspark/scripts/judge.py`) and record an available `sem1:` value as the additive
`semantic_hash` on that reviewed entry. Retain raw hashes for every inspected source/test/Knowledge
input. Missing helper/interpreter support or an unavailable fingerprint omits this optional field;
it never justifies invented metadata or a review failure. Stamp only the newly produced gate.

After producing the report:

- Ensure `FEATURE_DIR/gates/` exists.
- Save report as `FEATURE_DIR/gates/critic.md` (replace, do not append).
- The gate YAML block is authoritative for downstream commands (`/devspark-tasks`, `/devspark-implement`, `/devspark-create-pr`, `/devspark-pr-review`, `/devspark-address-pr-review`).
- `reviewed_artifacts` hashes are what let `/devspark-implement`'s gate pre-flight detect a stale review (artifact changed since this gate ran) instead of trusting a point-in-time verdict forever.

## Guidelines

### Adversarial Mindset

- **Murphy's Law**: If it can fail, it will fail.
- **Challenge optimism**: Estimates are optimistic; dependencies hide.
- **Real-world bias**: Prioritize incidents you've seen in production.
- **No benefit of doubt**: Vague = wrong; missing = will bite you.
- **Be brutally honest**: Sugar-coating helps nobody.

### Focus Areas (priority order, modulated by archetype)

1. Constitution violations (always SHOWSTOPPER)
2. Trust boundary / authz / secrets exposure
3. Data loss & non-idempotent / silent corruption
4. Concurrency / resource leaks / unbounded growth
5. Missing observability appropriate to runtime
6. Stack/archetype-specific traps from loaded checklists
7. Missing operational tasks (FULL scope only)

### Ignore

- Code style preferences (if linter configured)
- Documentation drift and documentation-format inconsistencies (route to `/devspark-analyze`)
- **Low-probability AND low-impact** issues *(low-probability/high-impact events must still be surfaced)*
- Micro-optimizations without proven bottlenecks
- Any observation that would restate a concern the spec, plan, or tasks already handles

### Output Quality

- **Be specific**: "Blocking sync DB driver on async runtime starves the event loop" — not "database concerns."
- **Cite locations**: spec/plan/task section or line.
- **Cite baseline evidence**: name the file and symbol behind any claim about how the system behaves today.
- **Quantify impact**: "N+1 access in list path → 1000+ round trips per 1000 items."
- **Suggest fixes**: Concrete remediation, not complaints — the least-complex mitigation first, new machinery only with a reason the lighter option fails.
- **Translate across stacks**: Pair every named tool with its capability.
- **Challenge NFRs concretely**: name the specific target, the mechanism that makes it unreachable, and what would have to change — never "this may not scale."

### Interaction Policy

Critic having run is not by itself a reason to interrupt the developer (assurance-contract.md §10):

| State | Behavior |
| ----- | -------- |
| `PROCEED`, no open consequential findings | Report and continue |
| Non-blocking findings, already classified | Concise summary and continue |
| An unresolved consequential human judgment | Ask exactly that question |
| `STOP`, or a blocker needing authority or a scope decision | Stop |

### Context Efficiency

- Minimal high-signal tokens; progressive disclosure.
- Limit each table to ≤30 rows; summarize overflow into a `findings_overflow` count.
- Deterministic: same inputs → same `finding_id`s and counts.

### Analysis Rules

- **NEVER modify files** other than the gate artifact.
- **NEVER hallucinate missing sections**; report absence accurately.
- Prioritize showstoppers.
- Report zero issues gracefully with a positive summary.

## Key Differences from /devspark-analyze

The canonical boundary lives in `assurance-contract.md` §2–§4. This table is a summary of it, not a second definition.

| Aspect       | /devspark-analyze                      | /devspark-critic                         |
| ------------ | -------------------------------------- | ---------------------------------------- |
| Core question | Are the planning artifacts internally sound? | Is this sound plan wrong about the real system? |
| World        | Closed — the authored artifacts        | Open — artifacts plus code, tests, knowledge |
| Mindset      | Neutral validator                      | Adversarial skeptic                      |
| Focus        | Alignment across artifacts             | Production failure modes                 |
| Evidence     | The artifacts themselves               | Inspected source and current knowledge   |
| Severity     | Quality issues                         | Business impact                          |
| Output       | Remediation suggestions                | Derived STOP / CONDITIONAL / PROCEED     |
| Constitution | CRITICAL violations                    | SHOWSTOPPER violations                   |

## Backward Compatibility

If spec.md lacks `archetype`, `risk_profile`, or `change_type` frontmatter, the command still runs with sensible defaults (`web-service`, `internal`, `greenfield`). The inferred values are reported in the gate header and marked `inferred`. Per assurance-contract.md §6.1, missing metadata is **never** emitted as a finding — earlier versions emitted `archetype-ambiguity`, `missing-risk-profile`, and `missing-change-type` as HIGH findings; those are removed. Consumers that previously read them should read the gate header instead.

The `findings:` list gains `owner`, `evidence`, `root_cause`, and `classification`; the optional `routed_findings:` and `verification_obligations:` blocks are additive and omitted when empty. Existing consumers that read `finding_id`, severity, `status`, and `outcome` are unaffected.

## Context

{ARGS}

## Shared Review Resolution Contract

Findings use the shared resolution contract so downstream tools (`/devspark-address-pr-review`, telemetry, release) can act deterministically.

- `finding_id` MUST be stable across re-runs when the underlying issue is unchanged.
- `execution_mode` MUST be one of: `auto` (safe to apply automatically), `selective` (apply with reviewer approval), `manual` (requires human implementation).
- `status` and `outcome` are written by `/devspark-address-pr-review` (FR-028).
