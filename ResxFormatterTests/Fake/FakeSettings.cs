namespace ResxFormatterTests.Fake
{
    using ResxFormatter;

    public class FakeSettings : IFormatSettings
    {
        public bool RemoveDocumentationComment { get; set; }
        public bool SortEntries { get; set; }
    }
}