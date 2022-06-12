namespace ResxFormatter
{
    public class Settings : ISettings
    {
        private readonly ResxWriterFix resxWriterFix = new ResxWriterFix();
        private ConfigurationSource configurationSource = ConfigurationSource.VisualStudio;

        private bool removeDocumentationComment = true;

        public Settings(ISettingsHost settingsHost)
        {
            this.SettingsHost = settingsHost;
        }

        public ConfigurationSource ConfigurationSource
        {
            get => this.configurationSource;
            set
            {
                if (this.configurationSource != value)
                {
                    var isEditorConfig = value == ConfigurationSource.EditorConfig;
                    this.SettingsHost.SetReadOnly(nameof(this.SortEntries), isEditorConfig);
                    this.SettingsHost.SetReadOnly(nameof(this.RemoveDocumentationComment), isEditorConfig);
                    this.SettingsHost.SetReadOnly(nameof(this.FixResxWriter), isEditorConfig && !this.RemoveDocumentationComment);

                    if (isEditorConfig && !this.RemoveDocumentationComment && this.FixResxWriter)
                    {
                        this.FixResxWriter = false;
                    }
                }

                this.configurationSource = value;
            }
        }

        public bool FixResxWriter
        {
            get => this.resxWriterFix.IsActive;
            set
            {
                if (this.resxWriterFix.IsActive != value)
                {
                    var isEditorConfig = this.ConfigurationSource == ConfigurationSource.EditorConfig;
                    this.SettingsHost.SetReadOnly(nameof(this.FixResxWriter), isEditorConfig && !this.RemoveDocumentationComment);

                    if (isEditorConfig && value && !this.RemoveDocumentationComment)
                    {
                        return;
                    }
                }

                this.resxWriterFix.IsActive = value;
            }
        }

        public ReloadMode ReloadFile { get; set; } = ReloadMode.AfterModification;

        public bool RemoveDesignerComments { get; set; } = true;

        public bool RemoveDocumentationComment
        {
            get => this.removeDocumentationComment; set
            {
                if (this.removeDocumentationComment != value)
                {
                    var isEditorConfig = this.ConfigurationSource == ConfigurationSource.EditorConfig;
                    if (isEditorConfig && !value)
                    {
                        this.FixResxWriter = false;
                        this.SettingsHost.SetReadOnly(nameof(this.FixResxWriter), true);
                    }
                }

                this.removeDocumentationComment = value;
            }
        }

        public bool SortEntries { get; set; } = true;

        private ISettingsHost SettingsHost { get; }
    }
}