# ResxFormatter

Optimizes resx files after saving: removes schema and comments (in particular the 3 KB documentation that is included in every resx file) and sorts entries alphabetically so that they look like this:

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

Use only with a source control system and at your own risk. See the [change log](https://github.com/stefanegli/ResxFormatter/blob/master/CHANGELOG.md) for changes and road map.

# Getting started

Configuration requires EditorConfig files. Refer to the [documentation](https://github.com/stefanegli/ResxFormatter#settings) for details.

# Contributing

Please use the [issue tracker](https://github.com/stefanegli/ResxFormatter/issues) for submitting bug reports or feature requests.

# License

[MIT License](https://github.com/stefanegli/ResxFormatter/blob/master/LICENSE)
