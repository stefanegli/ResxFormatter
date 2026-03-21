namespace ResxFormatter
{
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

    public interface ISettings
    {
        FixMode FixResxWriterMode { get; }
        ReloadMode ReloadFile { get; }
    }
}
