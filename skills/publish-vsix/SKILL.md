---
name: publish-vsix
description: Publish a ResxFormatter VSIX release by reading the latest public AppVeyor build version, verifying that the successful build matches the current Git commit, creating the corresponding numeric release tag, and pushing only that tag to the repository's GitHub remote. Use when asked to publish, release, or tag the latest ResxFormatter AppVeyor build.
---

# Publish ResxFormatter VSIX

Use the bundled script so the AppVeyor version and Git commit checks remain deterministic.

## Workflow

1. Inspect `git status --short` and the `github` remote without changing them.
2. Preview the release from the repository root:

   ```powershell
   powershell.exe -NoProfile -ExecutionPolicy Bypass -File skills/publish-vsix/scripts/Publish-Vsix.ps1 -WhatIf
   ```

3. Verify the preview names the expected AppVeyor build, commit, version, tag, and GitHub remote.
4. If the user explicitly requested publication, run the same command without `-WhatIf`. Otherwise, report the preview and ask before creating or pushing the tag.
5. Report the pushed tag. Explain that its GitHub push triggers AppVeyor's tagged build, which publishes the VSIX when CI succeeds.

## Guardrails

- Use the version returned by the AppVeyor API; never increment or manufacture a version.
- Require the latest build to be successful, untagged, and built from the current `HEAD`.
- Require a clean worktree and a GitHub-hosted `github` remote.
- Never delete, move, overwrite, or force-push a tag.
- Push only `refs/tags/<tag>`; do not push a branch or unrelated commits.
- Treat an already-tagged latest build as already released and stop.
