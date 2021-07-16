namespace ResxFormatter.VisualStudio
{
    public  class VsDocument
    {
        public VsDocument(uint cookie, string path)
        {
            this.Cookie = cookie;
            this.Path = path;
        }

        public uint Cookie { get; }
        public bool IsResx => this.Path.ToUpperInvariant().EndsWith(".RESX");
        public string Path { get; }

    }
}
