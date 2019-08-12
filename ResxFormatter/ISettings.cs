namespace ResxFormatter
{
    public interface ISettings
    {
        bool ReloadFileAutomatically { get; }
        bool RemoveDocumentationComment { get; }
        bool SortEntries { get; }
    }
}
