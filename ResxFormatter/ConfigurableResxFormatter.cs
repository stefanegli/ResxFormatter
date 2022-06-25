namespace ResxFormatter
{
    public class ConfigurableResxFormatter
    {
        public ConfigurableResxFormatter(ILog log)
        {
            this.Log = log;
        }

        public bool IsFileChanged { get; private set; }

        private ILog Log { get; }

        /// <summary>
        /// Returns true if the given file was modified.
        /// </summary>
        public void Run(string resxPath)
        {
            var settings = new ResxEditorConfigSettings(resxPath);
            if (!settings.IsActive)
            {
                return;
            }

            var formatter = new ResxFormatter(settings, this.Log);
            formatter.Run(resxPath);
            this.IsFileChanged = formatter.IsFileChanged;
        }
    }
}