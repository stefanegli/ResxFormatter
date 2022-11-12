namespace ResxFormatterTests.Fake
{
    using ResxFormatter;

    using System;

    public class FakeSettings : IFormatSettings
    {
        public StringComparer Comparer { get; set; } = StringComparer.Ordinal;
        public bool RemoveDocumentationComment { get; set; }
        public bool RemoveXsdSchema { get; set; }
        public bool SortEntries { get; set; }
    }
}