# ResxFormatter
Optimizes resx files after saving: Removes schema and comments (in particular the 3KB documentation that is included in every resx file) and sorts entries alphabetically so that they look like this:
```xml
<?xml version="1.0" encoding="utf-8"?>
<root>
  <schema />
  <resheader name="resmimetype">
    <value>text/microsoft-resx</value>
  </resheader>
  <resheader name="version">
    <value>2.0</value>
  </resheader>
  <resheader name="reader">
    <value>System.Resources.ResXResourceReader, System.Windows.Forms, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089</value>
  </resheader>
  <resheader name="writer">
    <value>System.Resources.ResXResourceWriter, System.Windows.Forms, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089</value>
  </resheader>
  <data name="a" xml:space="preserve">
    <value>LESS CLUTTER</value>
  </data>
  <data name="b" xml:space="preserve">
    <value>SORTED BY KEYS</value>
  </data>
</root>
```


Use only with a source control system and at your own risk. See the [change log](CHANGELOG.md) for changes and road map.

----
Download this extension from the [Visual Studio Marketplace](https://marketplace.visualstudio.com/items?itemName=stefan-egli.ResxFormatter)
or get the [CI build](http://vsixgallery.com/extension/ResxFormatter.61507132-4401-47b1-9950-575e43b964c6/).


[![Build status](https://ci.appveyor.com/api/projects/status/3fn0a5uhraovv6a3?svg=true)](https://ci.appveyor.com/project/stefanegli/resxformatter)


# Settings

## EditorConfig
Formatting rules are configured in the [EditorConfig](https://editorconfig.org/) file as follows:

```ini
[*.resx]
resx_formatter_sort_entries=true
resx_formatter_remove_xsd_schema=true
resx_formatter_remove_documentation_comment=true
resx_formatter_sort_comparer=OrdinalIgnoreCase
```

Comparer can be one of the following: _InvariantCulture, InvariantCultureIgnoreCase, OrdinalIgnoreCase, Ordinal_. The default value is _Ordinal_.

When the [EditorConfig Language Service](https://github.com/madskristensen/EditorConfigLanguage) version 1.18.35 or newer is installed, the ResxFormatter VSIX contributes these properties to its IntelliSense and validation. Restart Visual Studio after installing or updating either extension so that the custom schema is loaded.

| :information_source: You can format all resx files in the current solution folder via Extensions > ResxFormatter menu. |
| ---- |


## Visual Studio
A few things can be configured and probably you want to have this done as follows:

![Settings](ResxFormatter/_doc/Settings.png)

> Use the experimental setting with caution since it may have undesired side effects. It is also worth to note,
> that the extension may insert schema or documentation comment in order to match the desired effect of your EditorConfig settings.

# Agent Skill / CLI

Download `resxfmt-<version>.zip` from an [AppVeyor build](https://ci.appveyor.com/project/stefanegli/resxformatter) and extract its `resxfmt` directory into your Codex skills directory. The skill includes .NET 10 framework-dependent single-file executables for Windows x64 and Linux x64.

> [!NOTE]
> The packaged executables are not digitally signed.

Invoke the agent skill with:

```text
Use $resxfmt to check and format the .resx files in this repository.
```

Or call the packaged executable directly from the directory containing the files you want to process.

Windows:

```powershell
& "$env:USERPROFILE\.codex\skills\resxfmt\assets\cli\win-x64\resxfmt.exe" --check --recursive .
```

Linux:

```bash
chmod u+x "$HOME/.codex/skills/resxfmt/assets/cli/linux-x64/resxfmt"
"$HOME/.codex/skills/resxfmt/assets/cli/linux-x64/resxfmt" --check --recursive .
```

The command syntax is `resxfmt [options] [<path> ...]`. Use `--check` to detect required changes without writing, `--dry-run` to preview, `--recursive` to include subdirectories, and `--verbose` for detailed output. Formatting follows the applicable EditorConfig settings; without a path, the current directory is processed.


# Contributing
Please use the [issue tracker](https://github.com/stefanegli/ResxFormatter/issues) for submitting bug reports or feature requests.

# License
[MIT License](LICENSE)

## Third Party Licenses

| Library | License |
| ------- |---------|
| [EditorConfig .NET Core](https://github.com/editorconfig/editorconfig-core-net) | [MIT License](https://github.com/editorconfig/editorconfig-core-net/blob/master/LICENSE) |
| [xUnit](https://github.com/xunit/xunit) | [Apache License 2.0 / MIT License](https://github.com/xunit/xunit/blob/main/LICENSE) |
| [NFluent](https://github.com/tpierrain/NFluent) | [Apache License 2.0](https://github.com/tpierrain/NFluent/blob/master/LICENSE.txt) |
| [Community toolkit for Visual Studio extensions](https://github.com/VsixCommunity/Community.VisualStudio.Toolkit) | [Apache License 2.0](https://github.com/VsixCommunity/Community.VisualStudio.Toolkit/blob/master/LICENSE) |
