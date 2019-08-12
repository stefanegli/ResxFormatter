namespace ResxFormatter
{
    public interface ISettings
    {
        bool FixResxWriter { get; }
        bool ReloadFileAutomatically { get; }
        bool RemoveDocumentationComment { get; }
        bool SortEntries { get; }
    }
}
