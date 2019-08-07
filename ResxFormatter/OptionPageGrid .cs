namespace ResxFormatter
{
    using Microsoft.VisualStudio.Shell;
    using System.ComponentModel;

    internal class OptionPageGrid : DialogPage
    {
        internal const string GeneralCategory = "General";

        [Category(GeneralCategory)]
        [DisplayName("Reload file after saving")]
        [Description("")]
        public bool ReloadFileAutomatically { get; set; } = true;
    }
}
