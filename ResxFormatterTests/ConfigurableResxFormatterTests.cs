namespace ResxFormatterTests
{
    using NFluent;

    using ResxFormatter;

    using ResxFormatterTests.Fake;
    using ResxFormatterTests.TestFoundation;

    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading;

    using Xunit;

    public class ConfigurableResxFormatterTests : IDisposable
    {
        private readonly List<string> temporaryFiles = new List<string>();

        [Fact]
        public void Alternate_sort_method_can_be_configured()
        {
            // Arrange
            (var actualFile2, var expectedFile2) = this.PrepareFile(@"_editor\sort", "Sort");

            var formatter = new ConfigurableResxFormatter(new FakeLog());
            Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-Us");

            // Act
            formatter.Run(actualFile2);

            // Assert
            var actual1 = TextNormalization.NormalizeLineEndings(File.ReadAllText(actualFile2));
            var expected1 = TextNormalization.NormalizeLineEndings(File.ReadAllText(expectedFile2));
            Check.WithCustomMessage("sorting applied correctly").That(actual1).Equals(expected1);
        }

        [Fact]
        public void Different_file_extensions_can_be_processed()
        {
            // Arrange
            (var actualFile1, var expectedFile1) = this.PrepareFile(@"_editor\filetype", "Sort", "abc");

            var formatter = new ConfigurableResxFormatter(new FakeLog());
            Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-Us");

            // Act
            formatter.Run(actualFile1);

            // Assert
            var actual1 = TextNormalization.NormalizeLineEndings(File.ReadAllText(actualFile1));
            var expected1 = TextNormalization.NormalizeLineEndings(File.ReadAllText(expectedFile1));
            Check.WithCustomMessage("config1 is applied correctly").That(actual1).Equals(expected1);
        }

        [Fact]
        public void EditorConfig_files_can_be_specified_per_folder()
        {
            // Arrange
            (var actualFile1, var expectedFile1) = this.PrepareFile(@"_editor\config1", "Sort");
            (var actualFile2, var expectedFile2) = this.PrepareFile(@"_editor\config2", "Sort");

            var formatter = new ConfigurableResxFormatter(new FakeLog());
            Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-Us");

            // Act
            formatter.Run(actualFile1);
            formatter.Run(actualFile2);

            // Assert
            var actual1 = TextNormalization.NormalizeLineEndings(File.ReadAllText(actualFile1));
            var expected1 = TextNormalization.NormalizeLineEndings(File.ReadAllText(expectedFile1));
            Check.WithCustomMessage("config1 is applied correctly").That(actual1).Equals(expected1);

            var actual2 = TextNormalization.NormalizeLineEndings(File.ReadAllText(actualFile2));
            var expected2 = TextNormalization.NormalizeLineEndings(File.ReadAllText(expectedFile2));
            Check.WithCustomMessage("config2 is applied correctly").That(actual2).Equals(expected2);
        }

        [Fact]
        public void Resx_comment_and_schema_are_inserted_if_necessary()
        {
            // Arrange
            (var actualFile2, var expectedFile2) = this.PrepareFile(@"_editor\insertCommentAndSchema", "Sort");

            var formatter = new ConfigurableResxFormatter(new FakeLog());
            Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-Us");

            // Act
            formatter.Run(actualFile2);

            // Assert
            var actual1 = TextNormalization.NormalizeLineEndings(File.ReadAllText(actualFile2));
            var expected1 = TextNormalization.NormalizeLineEndings(File.ReadAllText(expectedFile2));
            Check.WithCustomMessage("resx comment is inserted correctly").That(actual1).Equals(expected1);
        }

        [Fact]
        public void Resx_comment_is_inserted_if_necessary()
        {
            // Arrange
            (var actualFile2, var expectedFile2) = this.PrepareFile(@"_editor\insertComment", "Sort");

            var formatter = new ConfigurableResxFormatter(new FakeLog());
            Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-Us");

            // Act
            formatter.Run(actualFile2);

            // Assert
            var actual1 = TextNormalization.NormalizeLineEndings(File.ReadAllText(actualFile2));
            var expected1 = TextNormalization.NormalizeLineEndings(File.ReadAllText(expectedFile2));
            Check.WithCustomMessage("resx comment is inserted correctly").That(actual1).Equals(expected1);
        }

        [Fact]
        public void Xsd_schema_can_be_removed()
        {
            // Arrange
            (var actualFile2, var expectedFile2) = this.PrepareFile(@"_editor\removeXsdSchema", "Schema");

            var formatter = new ConfigurableResxFormatter(new FakeLog());

            // Act
            formatter.Run(actualFile2);

            // Assert
            var actual1 = TextNormalization.NormalizeLineEndings(File.ReadAllText(actualFile2));
            var expected1 = TextNormalization.NormalizeLineEndings(File.ReadAllText(expectedFile2));
            Check.WithCustomMessage("schema is removed correctly").That(actual1).Equals(expected1);
        }

        [Fact]
        public void Formatter_reports_inactive_if_EditorConfig_does_not_enable_it()
        {
            // Arrange
            (var actualFile, var expectedFile) = this.PrepareFile(@"_editor\inactive", "Sort");
            var formatter = new ConfigurableResxFormatter(new FakeLog());

            // Act
            formatter.Run(actualFile);

            // Assert
            var actual = TextNormalization.NormalizeLineEndings(File.ReadAllText(actualFile));
            var expected = TextNormalization.NormalizeLineEndings(File.ReadAllText(expectedFile));
            Check.WithCustomMessage("file should remain unchanged when formatter is inactive").That(actual).Equals(expected);
            Check.WithCustomMessage("formatter should report inactive").That(formatter.IsActive).IsFalse();
            Check.WithCustomMessage("inactive formatter should not report file changes").That(formatter.IsFileChanged).IsFalse();
        }

        [Fact]
        public void Dry_run_detects_changes_without_writing_the_file()
        {
            // Arrange
            (var actualFile, var expectedFile) = this.PrepareFile(@"_editor\sort", "Sort");
            var formatter = new ConfigurableResxFormatter(new FakeLog());

            // Act
            formatter.Run(actualFile, false);

            // Assert
            var actual = File.ReadAllText(actualFile);
            var expected = File.ReadAllText(expectedFile);
            Check.WithCustomMessage("dry-run should not write changes to disk").That(actual).IsNotEqualTo(expected);
            Check.WithCustomMessage("formatter should report active for configured files").That(formatter.IsActive).IsTrue();
            Check.WithCustomMessage("dry-run should still detect pending updates").That(formatter.IsFileChanged).IsTrue();
        }

        public void Dispose()
        {
            foreach (var file in this.temporaryFiles)
            {
                File.Delete(file);
            }
        }

        private (string actual, string expected) PrepareFile(string folder, string baseFileName, string extension = "resx")
        {
            var sourceFile = Path.Combine(folder, $"{baseFileName}.{extension}");
            var actualFile = Path.Combine(folder, $"{baseFileName}-actual-{Guid.NewGuid():N}.{extension}");
            var expectedFile = Path.Combine(folder, $"{baseFileName}-expected.{extension}");

            File.Copy(sourceFile, actualFile);
            this.temporaryFiles.Add(actualFile);
            return (actualFile, expectedFile);
        }
    }
}
