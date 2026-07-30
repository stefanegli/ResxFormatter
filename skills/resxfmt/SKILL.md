---
name: resxfmt
description: Use the PetchNaka.ResxFormatter.Cli `resxfmt` .NET global tool to check, preview, or format `.resx` files according to EditorConfig; install or update the tool; interpret results and exit codes; configure formatting policy; or integrate checks into CI. Use for `resxfmt`, `.resx` formatting, or the ResxFormatter source repository.
---

# ResxFormatter CLI

Preview before writing, preserve EditorConfig policy, and verify every change.

## Workflow

1. Inspect the target paths, repository status, and applicable `.editorconfig` before writing.
2. Resolve the command:
   - Prefer `resxfmt` on `PATH`.
   - If missing, explain that installing `PetchNaka.ResxFormatter.Cli` requires the .NET 10 SDK. Obtain approval before running `dotnet tool install --global PetchNaka.ResxFormatter.Cli`.
   - Offer updates with `dotnet tool update --global PetchNaka.ResxFormatter.Cli`; never update silently.
   - In this source repository, use `dotnet run --project ResxFormatter.Cli/ResxFormatter.Cli.csproj -- <arguments>` when appropriate.
3. Run `resxfmt --check` on the exact scope. Add `--recursive` only when required. Treat exit `1` as pending changes and exit `2` as failure.
4. Stop for check-only requests. For formatting requests, rerun the same scope without `--check` or `--dry-run`, then verify with `--check`.
5. Inspect the relevant diff and report updated, unchanged, skipped, and failed files.

## Guardrails

- Do not install or update a global tool without the user's approval.
- Do not change `.editorconfig` merely to activate skipped files unless requested.
- Preserve the same paths and recursion choice for preview, write, and verification.
- Use `--` before a dash-prefixed path.
- Avoid unrelated `.resx` files in a dirty worktree.

## Reference

Read [references/cli-reference.md](references/cli-reference.md) when exact commands, PATH handling, options, settings, statuses, exit codes, or CI patterns are needed.
