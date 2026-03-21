namespace ResxFormatter
{
    using System;

    public interface IFormatSettings
    {
        StringComparer Comparer { get; }
        bool RemoveDocumentationComment { get; }
        bool RemoveXsdSchema { get; }
        bool SortEntries { get; }
    }
}
