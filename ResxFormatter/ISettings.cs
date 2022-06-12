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

    public interface IFormatSettings
    {
        bool RemoveDesignerComments { get; }
        bool RemoveDocumentationComment { get; }
        bool SortEntries { get; }
    }

    public interface ISettings : IFormatSettings
    {
        ConfigurationSource ConfigurationSource { get; }
        bool FixResxWriter { get; }
        ReloadMode ReloadFile { get; }
    }
}