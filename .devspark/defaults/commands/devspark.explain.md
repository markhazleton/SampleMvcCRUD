---
source: "BSW.DevSpark — © 2026 Baylor Scott & White Health. Source: https://bsw-devspark.bswhive.com"
description: Trace "how is X done?" against current code and .knowledge/, verifying sync/accuracy/completeness and updating or drafting knowledge as needed
handoffs:
  - label: Repo-Wide Audit
    agent: devspark-site-audit
    prompt: Run a full audit now that this topic's knowledge is in sync
  - label: Sweep Unrelated Stale Content
    agent: devspark-release
    prompt: Clean up unrelated stale work products or knowledge noticed during this trace
  - label: Spec the Missing Behavior
    agent: devspark-specify
    prompt: This topic revealed genuinely unimplemented behavior, not just undocumented behavior
  - label: Fix a Confirmed Code Defect
    agent: devspark-quickfix
    prompt: Explain confirmed a code defect while tracing this topic; fix it as a bounded quickfix
scripts:
  sh: .devspark/scripts/bash/explain-context.sh $ARGUMENTS --json
  ps: .devspark/scripts/powershell/explain-context.ps1 $ARGUMENTS -Json
---

## User Input

```text
$ARGUMENTS
```

You **MUST** consider the user input before proceeding (if not empty). The input is a free-text
topic or question, e.g. "how is authentication done" or "explain the release packaging flow".

## Purpose

Answer "how is X done?" grounded in current code, while verifying that the `.knowledge/` content
covering that topic — an existing entity/flat doc, or the absence of one — stays in sync, accurate,
and complete with the code. Reuses `/devspark-site-audit`'s DELTA/KNOW finding taxonomy, scoped to
this one topic instead of the whole repository.

Load and obey the shared preamble contract core at `/.devspark/templates/command-preamble-contract.md`
(installed repos) or `templates/command-preamble-contract.md` (source repos) before step 1. Preamble
modules: `convergence`.

## Lifecycle Position

Ad-hoc utility, alongside `/devspark-release` and `/devspark-site-audit` — not a step in the
`specify → plan → tasks → analyze → critic → implement → pr-review` gate chain. Usable any time a
developer or an agent needs a grounded answer instead of guessing, and doubles as a lightweight,
on-demand knowledge-sync check for one topic.

**Scope**: documents *existing* functionality only. Never creates a `.devspark.work/` planning
record and never implements new code — if the trace reveals genuinely unimplemented behavior, hand
off to `/devspark-specify` instead of drafting knowledge for code that doesn't exist.

## Dual-Audience Contract

Every run produces exactly three sections, in this order, so the same run serves a human reader
and a chaining agent without maintaining two representations that could drift apart:

- **`## Answer`** — human prose explaining how the topic works today. Every nontrivial claim is
  cited with a `path:line` reference. This is the direct answer to the question asked.
- **`## Findings`** — the DELTA/KNOW-coded table (id, severity, evidence, remediation), scoped to
  this topic, in the same format `/devspark-site-audit` uses. If resolution is `in-sync`, collapse
  this to one line ("No findings — matched knowledge verified against code") instead of an empty
  table with headers.
- **`## Agent Summary`** — a fenced JSON block, generated last and derived only from what the
  Answer/Findings already state (never an independent claim): `{topic, resolution, matched_ids[],
finding_ids[], applied, trace_saved, trace_path}` where `resolution` is one of `in-sync`,
  `corrected`, `drafted-new`, `proposal-pending`, `no-match-found`.

## Options

| Flag        | Behavior                                                                                                                           |
| ----------- | ---------------------------------------------------------------------------------------------------------------------------------- |
| no flag     | Interactive: each optional write is a separate question (see Outline steps 7-9)                                                    |
| `--dry-run` | Always propose-only; never ask to write, never write — for CI or agent-chained contexts with no interactive confirmation available |

## Outline

1. Run `{SCRIPT}`, parse JSON: `TOPIC`, `KEYWORDS`, `INDEX_FRESH`, `MATCHED_KNOWLEDGE[]`,
   `MATCHED_ENTITIES[]`, `CANDIDATE_CODE_HITS[]`, `SUMMARY`.
2. **KNOW1 gate**: if `INDEX_FRESH` is false, regenerate via
   `python3 scripts/build_knowledge_index.py --repo-root .` and re-run step 1 once before
   continuing — never trust a stale index for the findings below.
3. Branch on `MATCHED_KNOWLEDGE` / `MATCHED_ENTITIES`:
   - **Has matches** → for each matched node, verify against the code at its
     `source_of_truth`/`appliesTo` paths (already resolved with existence + last-commit-date in
     `source_status`), producing topic-scoped findings using site-audit's exact codes:
     - `DELTA1`/`DELTA2` — code and knowledge contradict each other.
     - `DELTA3` — the described behavior lacks proportionate test coverage. Check `tests/` (and
       any test dirs) for coverage of the matched code itself, not only what the doc happens to
       cite — this is broader than DELTA5 below.
     - `DELTA5` — if the doc cites `verified_by: execution` or a `type: test` evidence entry,
       re-run that test now; a failure or missing test is the finding.
     - `KNOW2` — `last_verified` is older than the newest `source_status[].last_commit_date`.
     - `KNOW3` — a `source_of_truth`/`appliesTo` path's `exists` is false.
     - `KNOW4` — a matched `governance-decision`'s `constrains` has an entity whose
       `constrained_by_reciprocal` is false, or a matched entity's `constrained_by` doesn't
       resolve to a real decision.
     - Retained-baseline claims — a matched node's `claim_states[]` (present whenever the node's
       `source_of_truth` uses the object form with a pinned `digest`/`baseline`) reports each
       claim's own `status`: `verified-current`, `drifted`, or `pinned-unverified`. Treat `drifted`
       or `pinned-unverified` the same as `KNOW2`/`KNOW3` for finding purposes.
   - **No match, or a matched entity has `missing_layers` covering this topic** → report it as a
     KNOW6-equivalent observation. It is not a defect and creates no work. Only if the developer
     asks for a draft, use `CANDIDATE_CODE_HITS` plus direct code reading to draft the missing
     content — an entity/layer doc per `.knowledge/tooling/developer-guide.md`'s "Adding a new
     entity" steps, or a flat doc per `.knowledge/tooling/content-lifecycle.md`'s "Where does my
     new content go?" table. Draft only; never fabricate a claim, a `source_of_truth` path, or a
     test citation without evidence in `CANDIDATE_CODE_HITS` or direct code reading.
   - **Clarify before drafting when needed** → if the evidence does not establish the owning
     knowledge node, the correct layer/type, the intended scope, or whether a code delta is
     intentional, ask targeted clarification questions before proposing a durable update. Do not
     guess between competing nodes or turn an unresolved question into current truth.
   - **Classify every finding by the convergence rule** (preamble §10, `closeout-contract.md` §2/§4):
     - A correctness defect (code and knowledge contradict, or a cited test fails) is a **Blocking
       Defect**: report it with evidence. Explain does not fix it.
     - A gap that hits no expansion trigger (stale `last_verified`, a missing node or layer) is
       **Deferred Work** or a **Learning**: report it, and nothing more.
     - The absence of a Knowledge node is only an observation unless an existing requirement
       explicitly requires one.
     - Discovery never authorizes promotion (`closeout-contract.md` §8).
   - **Optional Knowledge update** → when the developer asks for a Knowledge correction or draft, the
     proposal MUST include only evidence from durable code and tests, and MUST remove or correct
     stale claims. Write it only after its own explicit confirmation (step 7) and rebuild the index;
     never put a spec, plan, task, quickfix, `.devspark.work/`, or `.archive/` reference into
     durable knowledge.
4. **Retained-baseline confirmation**: for each matched node with a non-empty `claim_states[]`
   (step 3's Retained-baseline claims bullet), handle every entry whose `status` is `drifted` or
   `pinned-unverified` one claim at a time — never batch-confirm multiple claims with one prompt:
   - Show the claim's `diff` exactly as returned (a `unified_diff` for text/token profiles, or the
     `ranges` list for `binary-v1`) — this is the only evidence the human sees before confirming,
     so never summarize or paraphrase it away.
   - Ask one explicit confirmation per claim: "Rotate this claim's baseline to the current content
     of `<path>`? [Y/n]" (or, for a never-before-verified `pinned-unverified` claim with no drift,
     "Confirm the pinned content of `<path>` is correct as-is? [Y/n]").
   - Yes → run
     `python3 scripts/explain-context.py --rotate-claim <doc_path> --claim-path <path> --claim-profile <profile> [--claim-region <region>]`
     (the doc path and every `--claim-*` value come verbatim from the matched node's `path` and the
     confirmed claim entry — never guess or reconstruct them), then re-run step 1 once so the
     Answer/Findings below reflect the newly verified state.
   - No → leave the claim's status unresolved (`resolution: proposal-pending`); do not call
     `--rotate-claim` and do not otherwise edit the document's `source_of_truth`/`verification`.
   - Never call `--rotate-claim` without having just shown that exact claim's diff and received an
     explicit yes in this same turn.
5. Compose `## Answer` from steps 1–4's evidence — cite `path:line` for every nontrivial claim, do
   not restate the Findings mechanics here.
6. Compose `## Findings` (see Dual-Audience Contract above for the collapse-when-clean rule).
7. Explain is read-only by default. Each optional action below needs its own explicit approval,
   asked as a separate question; never combine two actions, or an action and a trace save, into one
   yes/no. Under `--dry-run`, ask none of them.
   - **Knowledge update** — only when the developer asked for one and a concrete remediation exists
     (a corrected doc body, a bumped `last_verified`, or a new doc's full content). Show the full
     proposed content/diff, then ask "Apply this Knowledge update? [y/N]".
     - Yes → write the file(s), run `python3 scripts/build_knowledge_index.py --repo-root .`, and
       re-verify the specific finding now resolves before reporting success.
     - No → leave as a proposal only (`resolution: proposal-pending`); write nothing.
   - **Code defect** — Explain reports confirmed code defects and offers the Quickfix handoff
     (`/devspark-quickfix`). It does not modify application code itself.
   - **Follow-up work** — never started or scheduled by Explain; name it in the next-step footer.
8. Under `--dry-run`, never write anything: report `trace_saved: false` unconditionally and skip
   step 9.
9. Otherwise ask separately: "Save this trace to
   `.devspark.work/explain/<topic-slug>-<date>.md`? [y/N]" (never under `.knowledge/` — this is a
   work product, not durable truth). No answer means no. Write only on explicit confirmation; the
   saved file contains the same Answer/Findings/Agent Summary shown inline. Saved traces are
   ordinary `.devspark.work/` artifacts and are swept by `/devspark-release`'s normal retention
   rules like any other work product.
10. Emit `## Agent Summary` per the Dual-Audience Contract.
11. Preamble §6 next-step footer, choosing among the handoffs declared above based on what this
    run found (repo-wide follow-up, unrelated stale content noticed, or genuinely missing code).

## Constraints

- Never invents a `source_of_truth` path or test citation without evidence from
  `CANDIDATE_CODE_HITS` or direct code reading — same rule as site-audit's KNOW6.
- Never writes a knowledge file or a trace report without the explicit confirmation in steps 7–9.
- A finding discovered by Explain is reported and classified, but is not added to the current work
  unless the developer explicitly promotes it.
- Never rotates a retained-baseline claim (`--rotate-claim`) without first showing that exact
  claim's diff and receiving an explicit confirmation in the same turn (step 4).
- Always checks `last_verified`/path existence even when the matched content "looks right" —
  confidence is not evidence.
- Reuses `scripts/build_knowledge_index.py` for all frontmatter/git-log/index logic; never
  duplicates it (constitution Principle VI: single source of truth across languages).
- Never creates a `.devspark.work/specs/` or `.devspark.work/quickfixes/` planning record — a
  genuinely missing behavior is a signal to hand off to `/devspark-specify`, not something this
  command implements itself.
