# Preamble Module: Orchestration Discipline

<!-- BSW.DevSpark — © 2026 Baylor Scott & White Health. Source: https://bsw-devspark.bswhive.com -->

Part of the Command Preamble Contract (`command-preamble-contract.md`). Section numbers are stable identities that other documents cite. Commands load this module when the contract's module table lists them as consumers.

## 11. Orchestration Discipline (how many commands a developer must know)

> **Minimize required developer orchestration. DevSpark may use rich internal lifecycle states and specialist capabilities, but the normal developer path exposes only meaningful human decision boundaries.**
>
> **A new lifecycle responsibility does not automatically justify a new mandatory command.**

Internal rigor is supposed to *reduce* a developer's cognitive load, not transfer it to them as orchestration ceremony. A framework that answers every new responsibility with a new required prompt eventually requires a workflow cheat sheet to complete ordinary work, and at that point it has traded adoption for tidiness.

**The test for a new user-facing command.** Before adding one, answer: *does meaningful human judgment or authorization need to occur between these phases?* If yes, the boundary is real and deserves an entry point. If no, orchestrate it automatically inside the command that already owns the preceding phase.

Worked examples: evidence → closeout is mechanical (automatic, inside `/devspark-verify`); Analyze → Critic → Judge is mechanical (automatic, inside `/devspark-planning-review`, one developer step); closeout → PR is a real boundary (the developer inspects the decision before opening a PR); a proposal becoming authoritative Knowledge requires a human; pushing, tagging, or publishing requires human authorization.

**The converse also binds — do not over-automate.** Simplification is not an excuse for an opaque command that silently specifies, plans, implements, accepts limitations, revises hypotheses, approves, and publishes. The target is: **automate mechanical transitions; expose consequential decisions.**

**Capabilities and entry points are different things.** A new internal state does not need to surface as a command. Where an advanced or recovery entry point is genuinely useful, it is documented as optional and shares one implementation with its automatic caller — a simple default path plus explicit advanced commands, never two implementations of the same rules that can drift apart. The durable statement of this principle, with its command classification, lives in `.knowledge/governance/devspark-philosophy.md` and `.knowledge/governance/devspark-prompt-mapping.md`.
