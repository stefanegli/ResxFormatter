---
name: resxfmt-cli
description: Use the bundled platform-specific, framework-dependent single-file ResxFormatter `resxfmt` .NET CLI to check, preview, and format `.resx` resource files according to EditorConfig; interpret statuses and exit codes; configure formatting rules; integrate checks into CI; or diagnose skipped, unchanged, and failed files. Use when Codex needs to operate the packaged Windows or Linux x64 CLI, a `resxfmt` executable on PATH, the ResxFormatter source repository, or `dotnet run`.
---

# ResxFormatter CLI

Use `resxfmt` conservatively: preview first, preserve the user's EditorConfig policy, apply only requested changes, and verify the result.

## Workflow

1. Inspect the target paths, repository status, and applicable `.editorconfig` before writing.
2. Resolve the command:
   - On Windows x64, prefer `assets/cli/win-x64/resxfmt.exe` relative to this `SKILL.md`.
   - On Linux x64, prefer `assets/cli/linux-x64/resxfmt` relative to this `SKILL.md`; run `chmod u+x` on it first when its executable bit was not preserved during ZIP extraction.
   - Otherwise use an explicit executable or `resxfmt` already on `PATH`.
   - In the ResxFormatter source repository, use an existing repo-local artifact when appropriate.
   - Otherwise run the CLI project with `dotnet run --project ResxFormatter.Cli/ResxFormatter.Cli.csproj -- <arguments>`.
   - Build or publish only when no usable command exists.
3. Preview the exact scope with `--check`. Add `--recursive` only when nested directories belong in scope.
4. Interpret exit code `1` from `--check` as pending formatting changes, not an execution failure. Treat exit code `2` as a usage, path, access, or formatting failure.
5. Stop after the preview when the user requested a check, audit, or dry run.
6. When the user requested formatting, rerun the same targets and recursion choice without `--check` or `--dry-run`.
7. Verify the write by rerunning `--check` on the identical scope. Inspect `git diff --stat` and the relevant diff when working in a repository.
8. Report updated, unchanged, skipped, and failed results. Clearly identify any files left unformatted.

## Guardrails

- Resolve the packaged executable to an absolute path before changing the working directory to the target repository.
- Require the .NET 10 runtime; the packaged single files are framework-dependent, not self-contained.
- Do not execute a binary for the wrong operating system or architecture. Fall back to `resxfmt` on `PATH` or the source project when no matching packaged runtime identifier exists.
- Keep `PublishReadyToRun` disabled when rebuilding the skill package.
- Do not add or change `.editorconfig` merely to make a skipped file active unless the user requested configuration changes. Explain the missing policy instead.
- Do not assume directory arguments recurse; recursion requires `--recursive`.
- Use `--` before a dash-prefixed path.
- Keep explicit file and directory arguments intact between preview, write, and verification.
- Use `--verbose` to diagnose malformed files or EditorConfig parsing, but expect more detailed output and no compact summary.
- Avoid formatting unrelated `.resx` files in a dirty worktree.
- Do not hand-edit generated `.resx` ordering when the CLI can apply the repository's configured policy.

## Reference

Read [references/cli-reference.md](references/cli-reference.md) for exact commands, settings, output statuses, exit codes, packaged and source-repository paths, and CI patterns.
