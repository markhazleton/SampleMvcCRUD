# BSW.DevSpark Agent Skills

<!-- BSW.DevSpark — © 2026 Baylor Scott & White Health. Source: https://bsw-devspark.bswhive.com -->

This directory contains BSW.DevSpark's portable Agent Skills — capability packages
that comply with the open Agent Skills specification and can run in any
skills-compatible client without BSW.DevSpark installed.

BSW.DevSpark treats skills as portable capability packages inside a governed
lifecycle orchestration system. Commands invoke skills; BSW.DevSpark governs the
lifecycle around them.

```text
command -> adapter -> skill -> context scripts -> agent reasoning -> artifact
```

---

## Dual-Surface Model

BSW.DevSpark exposes two surfaces for the same capability:

| Surface | Location | Who uses it |
| ------- | -------- | ----------- |
| Slash-command | `templates/commands/` | BSW.DevSpark users inside a BSW.DevSpark-enabled repo |
| Agent Skill | `templates/skills/` | Any skills-compatible client, with or without BSW.DevSpark |

The command performs BSW.DevSpark-specific lifecycle work (routing, branch creation,
artifact placement, gating). The skill performs the portable reasoning. The
adapter contract defines the handoff.

---

## Skills in This Directory

| Skill | Version | Description |
| ----- | ------- | ----------- |
| [write-spec](write-spec/SKILL.md) | 0.1.0 | Draft feature specifications, requirements documents, user stories, acceptance criteria, and validation-ready specs |

---

## Key Documents

- **[ADAPTER-contract.md](ADAPTER-contract.md)** — How BSW.DevSpark commands
  invoke skills: discovery path, input variables, output placement,
  responsibility split.
- **[SKILL-validation-contract.md](SKILL-validation-contract.md)** — Rules
  every `SKILL.md` must satisfy: upstream frontmatter rules, BSW.DevSpark addendum,
  exit-code contract, repair rules.
- **[references/devspark-skills-guide.md](references/devspark-skills-guide.md)**
  — Step-by-step contributor guide for adding new BSW.DevSpark skills. Start here
  when building a second skill.

---

## Validation

Run skill validation from the repository root:

```bash
devspark skills list
devspark skills validate
devspark skills validate templates/skills/write-spec
```

See `SKILL-validation-contract.md` for the exit-code contract and diagnostic
format.
