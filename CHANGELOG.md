# Road map

- [ ] Settings / Options:
  - [ ] List of file extensions that should be processed
  - [ ] Formatting rules can only be configured by EditorConfig file (no EditorConfig == no effect)
  - [ ] Support different EditorConfig settings for sub folders of solution


Features that have a checkmark are complete and available for
download in the
[CI build](http://vsixgallery.com/extension/ResxFormatter.61507132-4401-47b1-9950-575e43b964c6/).

# Change log

These are the changes to each version that has been released
on the official Visual Studio extension gallery.

## 2.0
- [x] Support for Visual Studio 2017, 2019 and **2022** 
- [x] "FixResxWriter" setting works without restart of Visual Studio

## 1.2
- [x] BugFix: Reload settings now work as advertised (cf. issue #4)

## 1.1
- [x] Formatting rules can be configured in an [EditorConfig](https://editorconfig.org/) file
- [x] BugFix: Use "ordinal sort" to ensure consistent results regardless of regional settings (cf. issue #2)

| :warning: Extension settings for formatting rules are deprecated and will be removed in a future version. Use EditorConfig files instead. |
| ---- |

## 1.0
- [x] Settings / Options: 
  - [x] Configure automatic file reload after saving: Off, AfterModification, Always
  - [x] Enable / disable 'Remove documentation comment'
  - [x] Enable / disable sorting of resx entries
  - [x] Experimantal setting for "fixing" the ResxWriter directly

## 0.9

- [x] Automatically Reload File after saving. Unfortunately the experience is not super nice since I need to close / reopen the file.
- [x] Logging in Output Pane
- [x] Sorting of "MetaData" entries.


## 0.8

- [x] Bugfixes


## 0.7

- [x] Basic functionality: Remove "comment" and sort entries in resx files

