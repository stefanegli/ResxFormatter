# `resxfmt` CLI reference

## Command

```text
resxfmt [options] [<path> ...]
```

With no path, process the current directory. A directory is non-recursive unless `--recursive` is present. Accept only `.resx` file targets; deduplicate overlapping targets.

## Tool installation

The skill does not contain an executable. Install the .NET 10 SDK, which also supplies the required runtime, then install the global tool:

```powershell
dotnet tool install --global PetchNaka.ResxFormatter.Cli
resxfmt --version
```

Installing or updating a global tool changes the user's environment. Explain the requirement and obtain permission before doing so on the user's behalf.

Update an existing installation with:

```powershell
dotnet tool update --global PetchNaka.ResxFormatter.Cli
```

Prefer `resxfmt` on `PATH`. If a new installation is not immediately visible, the standard global-tool locations are `%USERPROFILE%\.dotnet\tools` on Windows and `$HOME/.dotnet/tools` on Linux and macOS. Add the appropriate directory to `PATH` or start a new shell rather than copying the tool executable.

When developing in the ResxFormatter source repository, run the project directly when using the checked-out source is more appropriate than the installed release:

```powershell
dotnet run --project ResxFormatter.Cli/ResxFormatter.Cli.csproj -- --check --recursive .
```

## Options

| Option | Effect |
| --- | --- |
| `-r`, `--recursive` | Recurse into directory targets. |
| `-v`, `--verbose` | Show detailed per-file logging and exception details. |
| `-n`, `--dry-run` | Preview without writing; return `0` even when changes are pending. |
| `--check` | Preview without writing; return `1` when any active file would change. |
| `-h`, `--help`, `/?` | Print help and return `0`. |
| `-V`, `--version` | Print the version and return `0`. |
| `--` | Stop option parsing so dash-prefixed paths can be targeted. |

## Safe command patterns

Check one file:

```powershell
resxfmt --check .\Resources\Strings.resx
```

Check every `.resx` file beneath the current directory:

```powershell
resxfmt --check --recursive .
```

Preview changes without making pending changes fail the command:

```powershell
resxfmt --dry-run --recursive .
```

Format and then verify the same scope:

```powershell
resxfmt --recursive .
resxfmt --check --recursive .
```

Target a dash-prefixed file:

```powershell
resxfmt -- --input.resx
```

## EditorConfig policy

Formatting is active for a file only when at least one supported setting applies through EditorConfig:

```ini
[*.resx]
resx_formatter_sort_entries=true
resx_formatter_remove_xsd_schema=true
resx_formatter_remove_documentation_comment=true
resx_formatter_sort_comparer=OrdinalIgnoreCase
```

Supported comparers are:

- `InvariantCulture`
- `InvariantCultureIgnoreCase`
- `OrdinalIgnoreCase`
- `Ordinal`

The default and fallback comparer is `Ordinal`. The comparer setting has an effect only when sorting is enabled. Supported boolean settings enable their behavior only for the exact value `true`.

The three behavior settings are independent. Do not insert the full example blindly; preserve the repository's intended schema, documentation-comment, and ordering policy.

## Statuses

| Status | Meaning |
| --- | --- |
| `updated` | The file changed on disk. |
| `would-update` | The active file would change, but dry-run/check prevented a write. |
| `unchanged` | Formatting is active and the file already matches policy. |
| `skipped` | No supported ResxFormatter setting applies to the file. |
| `failed` | The file could not be formatted, commonly because it is malformed or inaccessible. |

Paths in output are relative to the current working directory when possible. Without `--verbose`, the CLI also prints a compact summary. With `--verbose`, rely on per-file output and diagnostics.

## Exit codes

| Code | Meaning |
| --- | --- |
| `0` | Successful execution; with `--check`, no active file needs changes. |
| `1` | `--check` found at least one file that would change. |
| `2` | Unknown option, invalid/non-`.resx`/missing path, access error, or formatting failure. |

`No .resx files found.` can return `0` when the searched scope is valid but empty, or `2` when path errors also occurred. Inspect standard error before treating an empty result as success.

## CI

Install the tool, then use `--check --recursive` to enforce formatting without modifying the checkout:

```powershell
dotnet tool install --global PetchNaka.ResxFormatter.Cli
resxfmt --check --recursive .
```

Pin the package with `--version <version>` when reproducible CI builds require it. Treat exit `1` as a formatting-policy violation and exit `2` as an execution/configuration failure. Preserve CLI output in the job log so pending or failed paths are visible.
