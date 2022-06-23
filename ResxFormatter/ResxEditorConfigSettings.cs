using System;

namespace ResxFormatter
{
    internal class ResxEditorConfigSettings : IFormatSettings
    {
        public ResxEditorConfigSettings(string targetFile = "dummy.resx")
        {
            var isActive = false;
            try
            {
                var parser = new EditorConfig.Core.EditorConfigParser();
                var settings = parser.Parse(targetFile).Properties;
                if (settings.TryGetValue("resx_formatter_sort_entries", out var sortEntries))
                {
                    isActive = true;
                    this.SortEntries = IsEnabled(sortEntries);
                }

                if (settings.TryGetValue("resx_formatter_remove_documentation_comment", out var removeComment))
                {
                    isActive = true;
                    this.RemoveDocumentationComment = IsEnabled(removeComment);
                }
            }
            catch (Exception ex)
            {
                Log.Current.WriteLine("Failed to parse EditorConfig file:\n" + ex.ToString());
            }

            this.IsActive = isActive;

            bool IsEnabled(string setting) => "true" == setting;
        }

        public bool IsActive { get; }
        public bool RemoveDocumentationComment { get; }
        public bool SortEntries { get; }
    }
}