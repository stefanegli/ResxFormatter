namespace ResxFormatter.Cli.Tests
{
    using System;
    using System.IO;
    using Xunit;

    public class ProgramTests
    {
        [Fact]
        public void Unknown_option_is_a_usage_error()
        {
            var result = Run("--not-an-option");

            Assert.Equal(2, result.ExitCode);
            Assert.Contains("Unknown option", result.Error);
            Assert.Contains("Usage: resxfmt", result.Error);
        }

        [Fact]
        public void Invalid_path_syntax_is_reported_instead_of_crashing()
        {
            var result = Run("invalid\0path");

            Assert.Equal(2, result.ExitCode);
            Assert.Contains("Unable to access path", result.Error);
            Assert.Contains("No .resx files found.", result.Output);
        }

        [Fact]
        public void Path_error_remains_an_error_when_another_file_succeeds()
        {
            using (var directory = new TemporaryDirectory())
            {
                var file = directory.WriteResx("valid.resx", "b", "a");
                var wrongExtension = directory.Write("not-resx.txt", "not a resource file");
                directory.EnableFormatting();
                var missing = Path.Combine(directory.Path, "missing.resx");

                var result = Run(file, missing, wrongExtension);

                Assert.Equal(2, result.ExitCode);
                Assert.Contains("Path not found", result.Error);
                Assert.Contains("Path is not a .resx file", result.Error);
                Assert.Equal(new[] { "a", "b" }, TemporaryDirectory.ReadNames(file));
            }
        }

        [Fact]
        public void Check_reports_pending_changes_without_writing()
        {
            using (var directory = new TemporaryDirectory())
            {
                var file = directory.WriteResx("check.resx", "b", "a");
                directory.EnableFormatting();
                var original = File.ReadAllText(file);

                var result = Run("--check", file);

                Assert.Equal(1, result.ExitCode);
                Assert.Equal(original, File.ReadAllText(file));
                Assert.Contains("would-update", result.Output);
            }
        }

        [Fact]
        public void Dry_run_succeeds_without_writing()
        {
            using (var directory = new TemporaryDirectory())
            {
                var file = directory.WriteResx("dry.resx", "b", "a");
                directory.EnableFormatting();
                var original = File.ReadAllText(file);

                var result = Run("--dry-run", file);

                Assert.Equal(0, result.ExitCode);
                Assert.Equal(original, File.ReadAllText(file));
                Assert.Contains("Would update 1", result.Output);
            }
        }

        [Fact]
        public void Duplicate_targets_are_processed_once()
        {
            using (var directory = new TemporaryDirectory())
            {
                var file = directory.WriteResx("duplicate.resx", "b", "a");
                directory.EnableFormatting();

                var result = Run(file, file, directory.Path);
                var secondResult = Run(file);

                Assert.Equal(0, result.ExitCode);
                Assert.Contains("Processed 1 file(s)", result.Output);
                Assert.Equal(0, secondResult.ExitCode);
                Assert.Contains("unchanged", secondResult.Output);
            }
        }

        [Fact]
        public void Recursive_option_is_required_for_nested_files()
        {
            using (var directory = new TemporaryDirectory())
            {
                directory.EnableFormatting();
                var nestedDirectory = Directory.CreateDirectory(Path.Combine(directory.Path, "nested")).FullName;
                var nestedFile = directory.WriteResx(Path.Combine("nested", "nested.resx"), "b", "a");

                var nonRecursive = Run(directory.Path);
                var recursive = Run("--recursive", directory.Path);

                Assert.Equal(0, nonRecursive.ExitCode);
                Assert.Contains("No .resx files found.", nonRecursive.Output);
                Assert.Equal(0, recursive.ExitCode);
                Assert.Equal(new[] { "a", "b" }, TemporaryDirectory.ReadNames(nestedFile));
                Assert.True(Directory.Exists(nestedDirectory));
            }
        }

        [Fact]
        public void Inactive_and_malformed_files_have_distinct_outcomes()
        {
            using (var inactiveDirectory = new TemporaryDirectory())
            using (var malformedDirectory = new TemporaryDirectory())
            {
                var inactive = inactiveDirectory.WriteResx("inactive.resx", "b", "a");
                var malformed = malformedDirectory.Write("malformed.resx", "<root>");
                malformedDirectory.EnableFormatting();

                var inactiveResult = Run(inactive);
                var malformedResult = Run("--verbose", malformed);

                Assert.Equal(0, inactiveResult.ExitCode);
                Assert.Contains("skipped", inactiveResult.Output);
                Assert.Equal(2, malformedResult.ExitCode);
                Assert.Contains("failed", malformedResult.Output);
                Assert.DoesNotContain("Processed", malformedResult.Output);
                Assert.Contains("System.Xml.XmlException", malformedResult.Error);
                Assert.Equal("<root>", File.ReadAllText(malformed));
            }
        }

        [Fact]
        public void Option_terminator_allows_a_dash_prefixed_file_name()
        {
            using (var directory = new TemporaryDirectory())
            {
                directory.EnableFormatting();
                var file = directory.WriteResx("-input.resx", "b", "a");
                var originalDirectory = Environment.CurrentDirectory;
                try
                {
                    Environment.CurrentDirectory = directory.Path;
                    var result = Run("--", "-input.resx");

                    Assert.Equal(0, result.ExitCode);
                    Assert.Equal(new[] { "a", "b" }, TemporaryDirectory.ReadNames(file));
                }
                finally
                {
                    Environment.CurrentDirectory = originalDirectory;
                }
            }
        }

        private static (int ExitCode, string Output, string Error) Run(params string[] args)
        {
            var originalOut = Console.Out;
            var originalError = Console.Error;
            using (var output = new StringWriter())
            using (var error = new StringWriter())
            {
                try
                {
                    Console.SetOut(output);
                    Console.SetError(error);
                    var exitCode = Program.Main(args);
                    return (exitCode, output.ToString(), error.ToString());
                }
                finally
                {
                    Console.SetOut(originalOut);
                    Console.SetError(originalError);
                }
            }
        }

        private sealed class TemporaryDirectory : IDisposable
        {
            public TemporaryDirectory()
            {
                this.Path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "ResxFormatterCliTests", Guid.NewGuid().ToString("N"));
                Directory.CreateDirectory(this.Path);
            }

            public string Path { get; }

            public void EnableFormatting()
            {
                this.Write(".editorconfig", "root = true\r\n\r\n[*.resx]\r\nresx_formatter_sort_entries=true\r\nresx_formatter_remove_xsd_schema=true\r\nresx_formatter_remove_documentation_comment=true\r\n");
            }

            public static string[] ReadNames(string path)
            {
                var document = System.Xml.Linq.XDocument.Load(path);
                return System.Linq.Enumerable.ToArray(
                    System.Linq.Enumerable.Select(document.Root.Elements("data"), element => (string)element.Attribute("name")));
            }

            public string Write(string relativePath, string contents)
            {
                var path = System.IO.Path.Combine(this.Path, relativePath);
                Directory.CreateDirectory(System.IO.Path.GetDirectoryName(path));
                File.WriteAllText(path, contents);
                return path;
            }

            public string WriteResx(string relativePath, params string[] names)
            {
                var entries = string.Join("", Array.ConvertAll(names, name => $"<data name=\"{name}\"><value>{name}</value></data>"));
                return this.Write(relativePath, "<?xml version=\"1.0\" encoding=\"utf-8\"?><root><resheader name=\"resmimetype\"><value>text/microsoft-resx</value></resheader>" + entries + "</root>");
            }

            public void Dispose()
            {
                if (Directory.Exists(this.Path))
                {
                    Directory.Delete(this.Path, true);
                }
            }
        }
    }
}
