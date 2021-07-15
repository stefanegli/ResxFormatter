# ResxFormatter
Optimizies resx files after saving: Removes comments (in particular the 3KB documentation that is included in every resx file) and sorts entries alphabetically. Use only with a source control system and at your own risk.

See the [change log](CHANGELOG.md) for changes and road map.

----
Download this extension from the [Visual Studio Marketplace](https://marketplace.visualstudio.com/items?itemName=stefan-egli.ResxFormatter)
or get the [CI build](http://vsixgallery.com/extension/ResxFormatter.61507132-4401-47b1-9950-575e43b964c6/).



[![Build status](https://ci.appveyor.com/api/projects/status/3fn0a5uhraovv6a3?svg=true)](https://ci.appveyor.com/project/stefanegli/resxformatter)


# Settings

## EditorConfig
It is recommended that you configure the formatting rules in the [EditorConfig](https://editorconfig.org/) file as follows:

```
[*.resx]
resx_formatter_sort_entries=true
resx_formatter_remove_documentation_comment=true
```

If one of these keys is set then the corresponding settings in the extension cannot be modified.

| :warning: Currently EditorConfig files in sub folders of the solution are ignored. |
| ---- |


## Visual Studio
A few things can be configured and probably you want to have this done as follows:

![Settings](ResxFormatter/_doc/Settings.png)

> Use the experimental setting with caution since it may have undesired side effects.

| :warning: Extension settings for formatting rules are deprecated and will be removed in a future version. Use EditorConfig files instead. |
| ---- |


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

