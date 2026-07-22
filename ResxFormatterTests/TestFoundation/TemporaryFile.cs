namespace ResxFormatterTests.TestFoundation
{
    using System;
    using System.IO;

    internal sealed class TemporaryFile : IDisposable
    {
        private TemporaryFile(string directory, string path)
        {
            this.DirectoryPath = directory;
            this.Path = path;
        }

        public string DirectoryPath { get; }
        public string Path { get; }

        public static TemporaryFile Create(string contents, string extension = ".resx")
        {
            var directory = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "ResxFormatterTests", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(directory);
            var path = System.IO.Path.Combine(directory, "input" + extension);
            File.WriteAllText(path, contents);
            return new TemporaryFile(directory, path);
        }

        public static TemporaryFile Copy(string sourcePath)
        {
            var extension = System.IO.Path.GetExtension(sourcePath);
            var directory = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "ResxFormatterTests", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(directory);
            var path = System.IO.Path.Combine(directory, "input" + extension);
            File.Copy(sourcePath, path);
            return new TemporaryFile(directory, path);
        }

        public void Dispose()
        {
            if (Directory.Exists(this.DirectoryPath))
            {
                Directory.Delete(this.DirectoryPath, true);
            }
        }
    }
}
