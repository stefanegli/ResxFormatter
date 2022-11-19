namespace ResxFormatter
{
    public class Settings : ISettings
    {
        private readonly ResxWriterFix resxWriterFix = new ResxWriterFix();

        public FixMode FixResxWriterMode
        {
            get => this.resxWriterFix.Mode;
            set => this.resxWriterFix.Mode = value;
        }

        public ReloadMode ReloadFile { get; set; } = ReloadMode.AfterModification;

        public FixMode RemoveDocumentationComment { get; set; } = FixMode.RemoveCommentAndSchema;

        public bool SortEntries { get; set; } = true;
    }
}