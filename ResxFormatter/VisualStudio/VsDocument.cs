namespace ResxFormatter.VisualStudio
{ 
    using Microsoft.VisualStudio.Shell;
    using Microsoft.VisualStudio.Shell.Interop;

    public  class VsDocument
    {
        public VsDocument(RunningDocumentTable documents, uint cookie, string path)
        {
            this.Documents = documents;
            this.Cookie = cookie;
            this.Path = path;
        }

        public uint Cookie { get; }
        public bool IsResx => this.Path.ToUpperInvariant().EndsWith(".RESX");
        public string Path { get; }

        private RunningDocumentTable Documents { get; }

        public void Reload() 
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            this.Documents.CloseDocument(__FRAMECLOSE.FRAMECLOSE_NoSave, this.Cookie);
            VsShellUtilities.OpenDocument(ServiceProvider.GlobalProvider, this.Path);
        }
    }
}
