namespace ResxFormatter
{
    using System;

    public enum ReloadMode
    {
        Off,
        AfterModification,
        Always
    }

    public interface IFormatSettings
    {
        StringComparer Comparer { get; }
        bool RemoveDocumentationComment { get; }
        bool RemoveXsdSchema { get; }
        bool SortEntries { get; }
    }

    public interface ISettings
    {
        bool FixResxWriter { get; }
        ReloadMode ReloadFile { get; }
    }
}