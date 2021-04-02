namespace ResxFormatter
{
    public enum ConfigurationSource
    {
        VisualStudio,
        EditorConfig
    }

    public enum ReloadMode
    {
        Off,
        AfterModification,
        Always
    }

    public interface ISettings
    {
        bool FixResxWriter { get; }
        ReloadMode ReloadFile { get; }
        bool RemoveDocumentationComment { get; }
        bool SortEntries { get; }
    }
}