---
source: "BSW.DevSpark — © 2026 Baylor Scott & White Health. Source: https://bsw-devspark.bswhive.com"
description: Register a new application in the multi-app repository registry with guided metadata collection and automatic scaffolding.
---

## User Input

```text
$ARGUMENTS
```

You **MUST** consider the user input before proceeding (if not empty).

## Outline

Register a new application in the BSW.DevSpark multi-app registry at `.devspark.work/devspark.registry.json`.

1. **Collect application metadata** from the user input or interactively:
   - `id`: Unique, lowercase, path-safe identifier (e.g., `payments-api`)
   - `name`: Human-readable application name
   - `path`: Relative path from repo root (e.g., `apps/payments-api`)
   - `kind`: Application type (e.g., `runtime-api`, `web-client`, `web-admin`, `library`, `qa-harness`)
   - `purpose`: One-line description of the application's role
   - `runtime`: Technology/framework (e.g., `dotnet`, `react`, `node`)
   - `owner`: Team or individual responsible
   - `criticality`: `high`, `medium`, or `low`
   - `inherits`: List of profile names to inherit (must exist in registry)
   - `dependsOn`: List of app IDs this app depends on (must exist in registry)

2. **Validate inputs**:
   - Check that the `id` is not already registered (fail with duplicate error if so)
   - Check that all `inherits` profile references exist in the registry
   - Check that all `dependsOn` app references exist in the registry
   - Check that the `path` does not conflict with existing registered app paths

3. **Update the registry**:
   - Add the new application entry to the `apps` array in `.devspark.work/devspark.registry.json`
   - Ensure the registry passes full validation after the addition

4. **Scaffold the application roots** (always performed):
   - Create `{path}/.devspark.work/` for temporary app-scoped plans and operational work products
   - Create `{path}/.knowledge/` for app-scoped current truth
   - Create `{path}/.knowledge/guides/` for published app documentation
   - Do not create archive, decision-history, rationale-history, or completed-plan directories
   - Do NOT create or modify `.devspark/`

5. **Report results**:
   - Show the new registry entry
   - Confirm scaffolded directories
   - Print scope summary

## Constraints

- If the registry file does not exist, create it with `version: 1`, `mode: "multi-app"`, empty `profiles`, and the new app as the first entry
- If the `id` already exists, fail with a clear duplicate error and do not modify the registry
- The command MUST NOT modify `.devspark/` (ownership boundary)

## Removing an Application

To reverse a registration, edit `.devspark.work/devspark.registry.json` directly and remove the app's entry (and any profile references that point only to it), or ask the agent to run `/devspark-add-application` in reverse ("remove app `<app-id>` from the registry").

This removes only the registry entry. It reports, but never deletes, the app's `.devspark.work/`,
`.knowledge/`, or `.knowledge/guides/` roots. It also refuses to remove an app that another registered
app still lists in `dependsOn`.
