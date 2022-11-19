namespace ResxFormatter
{
    using System;

    public enum FixMode
    {
        Off,
        RemoveComment,
        RemoveCommentAndSchema
    }

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
        FixMode FixResxWriterMode { get; }
        ReloadMode ReloadFile { get; }
    }
}