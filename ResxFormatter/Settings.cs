﻿namespace ResxFormatter
{
    public class Settings : ISettings
    {
        public bool ReloadFileAutomatically { get; set; }
        public bool RemoveDocumentationComment { get; set; }
        public bool SortEntries { get; set; }
    }
}