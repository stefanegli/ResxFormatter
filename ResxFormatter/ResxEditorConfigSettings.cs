namespace ResxFormatter
{
    internal class ResxEditorConfigSettings
    {
        public ResxEditorConfigSettings(string targetFile = "dummy.resx")
        {
            var parser = new EditorConfig.Core.EditorConfigParser();
            var settings = parser.Parse(targetFile).Properties;

            var isActive = false;
            if (settings.TryGetValue("resx_formatter_sort_entries", out string sortEntries))
            {
                isActive = true;
                this.SortEntries = IsEnabled(sortEntries);
            }

            if (settings.TryGetValue("resx_formatter_remove_documentation_comment", out string removeComment))
            {
                isActive = true;
                this.RemoveDocumentationComment = IsEnabled(removeComment);
            }

            this.IsActive = isActive;

            bool IsEnabled(string setting) => "true" == setting;
        }

        public bool IsActive { get; }
        public bool RemoveDocumentationComment { get; }
        public bool SortEntries { get; }
    }
}