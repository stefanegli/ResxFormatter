namespace ResxFormatterTests
{
    using NFluent;

    using ResxFormatter;

    using ResxFormatterTests.Fake;

    using System.IO;
    using System.Threading;

    using Xunit;

    public class ConfigurableResxFormatterTests
    {
        public ConfigurableResxFormatterTests()
        {
            Log.Current.IsActive = false;
        }

        [Fact]
        public void Alternate_sort_method_can_be_configured()
        {
            // Arrange
            (var actualFile2, var expectedFile2) = FormattingTests.prepareFile(@"_editor\sort", "Sort");

            var formatter = new ConfigurableResxFormatter(new FakeLog());
            Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-Us");

            // Act
            formatter.Run(actualFile2);

            // Assert
            var actual1 = File.ReadAllText(actualFile2);
            var expected1 = File.ReadAllText(expectedFile2);
            Check.WithCustomMessage("sorting applied correctly").That(actual1).Equals(expected1);
        }

        [Fact]
        public void Different_file_extensions_can_be_processed()
        {
            // Arrange
            (var actualFile1, var expectedFile1) = FormattingTests.prepareFile(@"_editor\filetype", "Sort", "abc");

            var formatter = new ConfigurableResxFormatter(new FakeLog());
            Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-Us");

            // Act
            formatter.Run(actualFile1);

            // Assert
            var actual1 = File.ReadAllText(actualFile1);
            var expected1 = File.ReadAllText(expectedFile1);
            Check.WithCustomMessage("config1 is applied correctly").That(actual1).Equals(expected1);
        }

        [Fact]
        public void EditorConfig_files_can_be_specified_per_folder()
        {
            // Arrange
            (var actualFile1, var expectedFile1) = FormattingTests.prepareFile(@"_editor\config1", "Sort");
            (var actualFile2, var expectedFile2) = FormattingTests.prepareFile(@"_editor\config2", "Sort");

            var formatter = new ConfigurableResxFormatter(new FakeLog());
            Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-Us");

            // Act
            formatter.Run(actualFile1);
            formatter.Run(actualFile2);

            // Assert
            var actual1 = File.ReadAllText(actualFile1);
            var expected1 = File.ReadAllText(expectedFile1);
            Check.WithCustomMessage("config1 is applied correctly").That(actual1).Equals(expected1);

            var actual2 = File.ReadAllText(actualFile2);
            var expected2 = File.ReadAllText(expectedFile2);
            Check.WithCustomMessage("config2 is applied correctly").That(actual2).Equals(expected2);
        }

        [Fact]
        public void Resx_comment_and_schema_are_inserted_if_necessary()
        {
            // Arrange
            (var actualFile2, var expectedFile2) = FormattingTests.prepareFile(@"_editor\insertCommentAndSchema", "Sort");

            var formatter = new ConfigurableResxFormatter(new FakeLog());
            Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-Us");

            // Act
            formatter.Run(actualFile2);

            // Assert
            var actual1 = File.ReadAllText(actualFile2);
            var expected1 = File.ReadAllText(expectedFile2);
            Check.WithCustomMessage("resx comment is inserted correctly").That(actual1).Equals(expected1);
        }

        [Fact]
        public void Resx_comment_is_inserted_if_necessary()
        {
            // Arrange
            (var actualFile2, var expectedFile2) = FormattingTests.prepareFile(@"_editor\insertComment", "Sort");

            var formatter = new ConfigurableResxFormatter(new FakeLog());
            Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-Us");

            // Act
            formatter.Run(actualFile2);

            // Assert
            var actual1 = File.ReadAllText(actualFile2);
            var expected1 = File.ReadAllText(expectedFile2);
            Check.WithCustomMessage("resx comment is inserted correctly").That(actual1).Equals(expected1);
        }

        [Fact]
        public void Xsd_schema_can_be_removed()
        {
            // Arrange
            (var actualFile2, var expectedFile2) = FormattingTests.prepareFile(@"_editor\removeXsdSchema", "Schema");

            var formatter = new ConfigurableResxFormatter(new FakeLog());

            // Act
            formatter.Run(actualFile2);

            // Assert
            var actual1 = File.ReadAllText(actualFile2);
            var expected1 = File.ReadAllText(expectedFile2);
            Check.WithCustomMessage("schema is removed correctly").That(actual1).Equals(expected1);
        }
    }
}