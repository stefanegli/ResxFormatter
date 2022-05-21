namespace ResxFormatterTests
{

    using ResxFormatter;

    public partial class FormattingTests
    {
        private class FakeSettings : IFormatSettings
        {
            public bool RemoveDocumentationComment { get; set; }
            public bool SortEntries { get; set; }
        }
    }
}