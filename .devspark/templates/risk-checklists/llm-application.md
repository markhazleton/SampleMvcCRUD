# LLM Application Risk Checklist

<!-- BSW.DevSpark — © 2026 Baylor Scott & White Health. Source: https://bsw-devspark.bswhive.com -->

Applies when archetype = `llm-application`. Pair with stack checklists as needed.

## Prompt & Input Safety

- [ ] **Prompt-injection surfaces are identified and bounded** — e.g., system prompt hardening, tool-call allowlists, retrieval content sanitization
- [ ] **PHI/PII handling in prompts is explicitly constrained** — e.g., redaction policy, denylist/validator before model invocation
- [ ] **Untrusted context is labeled and isolated from instruction channels** — e.g., role separation, quoted context blocks, parser boundaries

## Evaluation & Behavior Control

- [ ] **Behavior-changing prompt/model edits have eval coverage before merge** — e.g., offline eval suite, fixed benchmark prompts
- [ ] **Golden fixtures exist for deterministic prompt/template render outputs** — e.g., fixture snapshots for prompt assembly
- [ ] **Judge/referee grading criteria are versioned and reproducible** — e.g., frozen grader prompt, scoring rubric commit hash
- [ ] **Pass/fail thresholds are explicit for agreement/quality gates** — e.g., minimum agreement %, max regression budget

## Runtime Guardrails & Reliability

- [ ] **Output validators/guardrails enforce schema and policy constraints** — e.g., JSON schema validation, policy filters, safe fallback path
- [ ] **Model/deployment blast radius is controlled** — e.g., staged rollout, shadow mode, canary traffic split, kill switch
- [ ] **Non-determinism is handled in tests** — e.g., tolerance windows, seed/temperature policy, deterministic fixture mode where possible

## Cost & Observability

- [ ] **Token/cost regression monitoring exists for prompt/model changes** — e.g., cost-per-call baseline vs post-change threshold
- [ ] **Prompt/version/model identifiers are emitted in telemetry** — e.g., structured trace attributes for prompt version, model deployment
- [ ] **Failure taxonomy covers refusal, hallucination, tool misuse, and timeout paths** — e.g., categorized error metrics and alerting
