namespace ResxFormatter
{
    using Microsoft.VisualStudio.Shell;

    using System;
    using System.ComponentModel;
    using System.Reflection;

    internal class OptionPageGrid : DialogPage, ISettings
    {
        internal const string ExperimentalCategory = "Experimental";
        internal const string FormattingCategory = "Formatting";
        internal const string GeneralCategory = "General";

        private ConfigurationSource configurationSource = ConfigurationSource.VisualStudio;

        [Category(FormattingCategory)]
        [DisplayName("Configured by")]
        [Description("Indicates how the formatting settings are controlled: By this dialog or an EditorConfig file.")]
        [ReadOnly(true)]
        public ConfigurationSource ConfigurationSource
        {
            get => this.configurationSource;
            set
            {
                if (value != this.configurationSource)
                {
                    this.SetFormattingReadonly(value == ConfigurationSource.EditorConfig);
                }

                this.configurationSource = value;
            }
        }

        [Category(ExperimentalCategory)]
        [DisplayName("Fix Resx Writer (Restart required)")]
        [Description("ATTENTION: Unwanted side effects possible: If enabled the ResXResourceWriter is tricked into not writing the 'documentation' comment. This is achived by modifying a static string field through reflection.")]
        [ReadOnly(false)]
        public bool FixResxWriter { get; set; }

        [Category(GeneralCategory)]
        [DisplayName("Reload file after saving")]
        [Description("Determines whether or not file is closed and re-opened if changes were my made by the extension so that they become immediately visible.")]
        [ReadOnly(false)]
        public ReloadMode ReloadFile { get; set; } = ReloadMode.AfterModification;

        [Category(FormattingCategory)]
        [DisplayName("Remove documentation comment")]
        [Description("Determines whether or not the resx 'documentation' is removed.")]
        [ReadOnly(false)]
        public bool RemoveDocumentationComment { get; set; } = true;

        [Category(FormattingCategory)]
        [DisplayName("Sort resource entries")]
        [Description("Determines whether or not resource entries are sorted alphabetically.")]
        [ReadOnly(false)]
        public bool SortEntries { get; set; } = true;

        private void SetFormattingReadonly(bool value)
        {
            try
            {
                var properties = TypeDescriptor.GetProperties(this);
                foreach (var p in properties)
                {
                    if (p is PropertyDescriptor property)
                    {
                        if (property.Name == nameof(this.SortEntries) || property.Name == nameof(this.RemoveDocumentationComment))
                        {
                            var readOnly = property.Attributes[typeof(ReadOnlyAttribute)];
                            if (readOnly is object)
                            {
                                var flags = BindingFlags.NonPublic | BindingFlags.Instance;
                                var field = readOnly.GetType().GetField("isReadOnly", flags);
                                field?.SetValue(readOnly, value, flags, null, null);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                var log = new Log();
                log.WriteLine(ex.ToString());
            }
        }
    }
}