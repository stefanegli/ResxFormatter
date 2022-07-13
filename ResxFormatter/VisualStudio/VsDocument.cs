namespace ResxFormatter.VisualStudio
{
    using Microsoft.VisualStudio.Shell;
    using Microsoft.VisualStudio.Shell.Interop;

    public class VsDocument
    {
        public VsDocument(RunningDocumentTable documents, uint cookie, string path)
        {
            this.Documents = documents;
            this.Cookie = cookie;
            this.Path = path;
        }

        public uint Cookie { get; }

        public string Path { get; }

        private RunningDocumentTable Documents { get; }

        public void Close()
        {
            this.Documents.CloseDocument(__FRAMECLOSE.FRAMECLOSE_NoSave, this.Cookie);
        }
    }
}