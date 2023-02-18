# Road map

- [ ] Drop support for VS 2017 and 2019?

Features that have a checkmark are complete and available for
download in the
[CI build](http://vsixgallery.com/extension/ResxFormatter.61507132-4401-47b1-9950-575e43b964c6/).

# Change log

These are the changes to each version that has been released
on the official Visual Studio extension gallery.

## 3.1
- [x] Fix problem with duplicate keys in [EditorConfig](https://editorconfig.org/) files (cf. issue #10)

## 3.0
- [x] Menu / Command for formatting all files in solution (directory)
- [x] Formatting rules can only be configured by [EditorConfig](https://editorconfig.org/) file (no configuration == no effect)
- [x] Support different EditorConfig settings for sub folders of solution
- [x] Support different file extensions (standard [EditorConfig](https://editorconfig.org/) functionality)
- [x] Support different comparers (cf. issue #9)
- [x] XSD Schema element can be removed from resx files
- [x] Changes to default settings are now visible (bold font)
- [x] BugFix: Extension does not load with VS 2017 (cf. issue #7)

## 2.0
- [x] Support for Visual Studio 2017, 2019 and **2022** 
- [x] "FixResxWriter" setting works without restart of Visual Studio

## 1.2
- [x] BugFix: Reload settings now work as advertised (cf. issue #4)

## 1.1
- [x] Formatting rules can be configured in an [EditorConfig](https://editorconfig.org/) file
- [x] BugFix: Use "ordinal sort" to ensure consistent results regardless of regional settings (cf. issue #2)

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

