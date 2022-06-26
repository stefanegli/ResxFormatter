namespace ResxFormatter
{
    public class Settings : ISettings
    {
        private readonly ResxWriterFix resxWriterFix = new ResxWriterFix();

        public bool FixResxWriter
        {
            get => this.resxWriterFix.IsActive;
            set => this.resxWriterFix.IsActive = value;
        }

        public ReloadMode ReloadFile { get; set; } = ReloadMode.AfterModification;

        public bool RemoveDocumentationComment { get; set; } = true;

        public bool SortEntries { get; set; } = true;
    }
}