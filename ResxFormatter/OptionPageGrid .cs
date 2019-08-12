namespace ResxFormatter
{
    using Microsoft.VisualStudio.Shell;
    using System.ComponentModel;

    internal class OptionPageGrid : DialogPage, ISettings
    {
        internal const string FormattingCategory = "Formatting";
        internal const string GeneralCategory = "General";

        [Category(GeneralCategory)]
        [DisplayName("Reload file after saving")]
        [Description("Determines whether or not file is closed and re-opened if changes were my made by the extension so that they become immediately visible.")]
        public bool ReloadFileAutomatically { get; set; } = true;

        [Category(FormattingCategory)]
        [DisplayName("Remove documentation comment")]
        [Description("Determines whether or not the resx 'documentation' is removed.")]
        public bool RemoveDocumentationComment { get; set; } = true;

        [Category(FormattingCategory)]
        [DisplayName("Sort resource entries")]
        [Description("Determines whether or not resource entries are sorted alphabetically.")]
        public bool SortEntries { get; set; } = true;
    }
}
