# `resxfmt` CLI reference

## Command

```text
resxfmt [options] [<path> ...]
```

With no path, process the current directory. A directory is non-recursive unless `--recursive` is present. Accept only `.resx` file targets; deduplicate overlapping targets.

The packaged skill stores the complete framework-dependent CLI payload here:

```text
<skill-directory>\assets\cli\resxfmt.exe
```

Resolve that executable to an absolute path, then invoke it while the current working directory is the repository containing the target files. Keep the adjacent files in `assets\cli` together.

When running from the ResxFormatter source repository without a directly invocable executable:

```powershell
dotnet run --project ResxFormatter.Cli/ResxFormatter.Cli.csproj -- --check --recursive .
```

Build or publish from the repository root with the .NET 10 SDK:

```powershell
dotnet build ResxFormatter.Cli/ResxFormatter.Cli.csproj --nologo
dotnet publish ResxFormatter.Cli/ResxFormatter.Cli.csproj -c Release --nologo
```

Known Windows repository artifacts:

- Debug build: `artifacts/bin/ResxFormatter.Cli/debug/resxfmt.exe`
- Release publish: `artifacts/publish/ResxFormatter.Cli/release/resxfmt.exe`

Prefer the packaged executable. Prefer `dotnet run` over guessing a repository artifact path when the build configuration or platform differs.

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

Use `--check --recursive` to enforce formatting without modifying the checkout:

```powershell
resxfmt --check --recursive .
```

Treat exit `1` as a formatting-policy violation and exit `2` as an execution/configuration failure. Preserve CLI output in the job log so pending or failed paths are visible.
