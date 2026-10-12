# Preamble Module: Convergence Discipline

<!-- BSW.DevSpark — © 2026 Baylor Scott & White Health. Source: https://bsw-devspark.bswhive.com -->

Part of the Command Preamble Contract (`command-preamble-contract.md`). Section numbers are stable identities that other documents cite. Commands load this module when the contract's module table lists them as consumers.

## 10. Convergence Discipline (finding classification)

Generating findings is easy; recognizing that work is finished is not. A command that treats every new observation as new work inside the active route creates a loop — verify, discover, implement, verify again — in which the specification silently expands and never converges.

> **A spec is complete when its stated objective has been resolved and every remaining finding has been classified — not when every observation generated during the work has been eliminated.**
>
> **Verification may discover work, but discovery alone does not expand the current specification.**

A finding creates mandatory work **inside the active route** only when it demonstrates one of five **expansion triggers**: failure of the stated objective, an unresolved requirement, a violated invariant, a correctness defect, or a regression caused by this change. Every other finding is classified — **Accepted Limitation**, **Deferred Work**, or **Learning / Changed Assumption** — and the route converges.

This binds every command that produces findings (`/devspark-analyze`, `/devspark-critic`, `/devspark-verify`, `/devspark-checklist`, `/devspark-explain`, `/devspark-site-audit`, `/devspark-pr-review`) and every command that consumes them (`/devspark-implement`, `/devspark-create-pr`). A finding command still reports everything it sees — reporting is not expansion. What it MUST NOT do is convert a non-triggering observation into required work on its own authority, or imply the route is unfinished because observations remain.

**Timing matters.** Before implementation begins, a gate that reshapes the spec is *authoring*, not expansion — `/devspark-analyze` and `/devspark-critic` exist precisely to change the plan while changing it is still cheap, and a finding that tightens a requirement there is the system working. The expansion rule binds from the moment implementation starts: after that point, a non-triggering observation is classified and carried, not folded into the route.

Authoring is still not automatic. Before implementation, a finding enters the plan only through Planning Review's Judge dispositions (`judge-contract.md` §11): a finding is evidence, not automatically work, and a risk does not prescribe its remediation. Discovery alone does not expand current scope in the authoring phase either.

### Human-authorized bounded correction

Do not confuse a changed file surface with a changed objective. During implementation, a small,
understood correction may be necessary to complete the already-authorized objective while crossing
an expected-file or task boundary. Handle it proportionally:

1. **Already authorized**: implement the minimum correction and run the applicable checks; do not
   ask for approval that the existing decision already grants.
2. **Adjacent discovery**: capture or report it as deferred work. Do not implement it without
   explicit human authorization.
3. **Bounded correction**: state the defect, smallest correction, affected files, and verification
   implications. Ask the human before changing scope. After explicit authorization, record the
   decision and bounded delta in the existing feature artifacts, then continue that feature.
   Do not create a new spec, branch, full planning bundle, or fresh Planning Review solely because
   the file surface grew; use existing freshness evidence to decide whether focused review is due.
4. **Material change**: stop when the correction changes the objective, architecture, domain or
   safety invariant, security posture, data contract, or another consequential approved decision.
   Route it through the existing authoring or review decision that actually applies.

An authorization is not a waiver: it does not satisfy a failed requirement, make a failing gate
pass, authorize work beyond its stated boundary, or override the constitution. Reconcile only the
affected existing records and tasks. Run all required verification and obey existing freshness
contracts; if they require a focused recheck or rerun, perform that action without regenerating
unaffected planning artifacts. Do not infer that review evidence is invalid solely from a changed
implementation file list; use the applicable freshness contract and recorded evidence.

The rule is symmetric, and the second half matters as much as the first: a finding that **does** hit an expansion trigger is a Blocking Defect and cannot be classified away. Classification is how a route converges honestly, not how a failed requirement is retired quietly.

The four classifications, the acceptance-criterion kinds (`requirement` / `invariant` / `hypothesis` / `target`) that decide which findings can block, the experimental rule for a disproven hypothesis, the `gates/closeout.md` artifact, and the rule that scope expands only on explicit developer intent are defined in `/.devspark/templates/closeout-contract.md` (installed repos) or `templates/closeout-contract.md` (source repos).

**Closeout is a phase, not a command.** `/devspark-verify` runs it automatically as its second half and emits the completion decision in the same invocation; `/devspark-closeout` is an advanced reassessment entry point, and no command may present it as a routine next step. See §11.

If the project constitution declares a convergence principle, a gate command that flags an unclassified finding at completion, a laundered requirement, or an unjustified mid-route expansion SHOULD cite that principle ID, per the same escalation hook as §9.2.
