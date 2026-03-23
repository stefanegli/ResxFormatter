namespace ResxFormatter
{
    public class ConfigurableResxFormatter
    {
        public ConfigurableResxFormatter(ILog log)
        {
            this.Log = log;
        }

        public bool IsActive { get; private set; }

        public bool IsFileChanged { get; private set; }

        private ILog Log { get; }

        /// <summary>
        /// Returns true if the given file was modified.
        /// </summary>
        public void Run(string resxPath)
        {
            this.Run(resxPath, true);
        }

        public void Run(string resxPath, bool writeChanges)
        {
            this.IsFileChanged = false;
            var settings = new ResxEditorConfigSettings(this.Log, resxPath);
            this.IsActive = settings.IsActive;
            if (!settings.IsActive)
            {
                return;
            }

            var formatter = new ResxFormatter(settings, this.Log);
            formatter.Run(resxPath, writeChanges);
            this.IsFileChanged = formatter.IsFileChanged;
        }
    }
}
