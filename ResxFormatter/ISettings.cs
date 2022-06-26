namespace ResxFormatter
{
    public enum ReloadMode
    {
        Off,
        AfterModification,
        Always
    }

    public interface IFormatSettings
    {
        bool RemoveDocumentationComment { get; }
        bool SortEntries { get; }
    }

    public interface ISettings : IFormatSettings
    {
        bool FixResxWriter { get; }
        ReloadMode ReloadFile { get; }
    }
}