namespace ResxFormatterTests
{
    using NFluent;

    using ResxFormatter;

    using ResxFormatterTests.Fake;
    using ResxFormatterTests.TestFoundation;

    using System.Collections.Generic;
    using System.IO;
    using System.Threading;

    using Xunit;

    public class FormattingTests
    {
        [Fact]
        public void Designer_files_are_updated_too()
        {
            // Arrange
            var settings = new FakeSettings()
            {
                SortEntries = true,
                RemoveDocumentationComment = true,
                RemoveDesignerComments = true,
            };

            (var actualFile, var expectedFile) = prepareFile(@"_files", "WithDesignerFile");
            (var actualDesignerFile, var expectedDesignerFile) = prepareFile(@"_files", "WithDesignerFile", "Designer.cs");

            var formatter = new ResxFormatter(settings, new FakeLog());
            Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-Us");

            // Act
            formatter.Run(actualFile);

            // Assert
            var actual1 = File.ReadAllText(actualFile);
            var expected1 = File.ReadAllText(expectedFile);
            Check.WithCustomMessage("resx is updated as expected").That(actual1).Equals(expected1);

            var actualDesigner = File.ReadAllText(actualDesignerFile);
            var expectedDesigner = File.ReadAllText(expectedDesignerFile);
            Check.WithCustomMessage("designer is updated as expected").That(actualDesigner).Equals(expectedDesigner);
        }

        [Fact]
        public void EditorConfig_files_can_be_specified_per_folder()
        {
            // Arrange
            var settings = new FakeSettings()
            {
                SortEntries = false,
                RemoveDocumentationComment = false,
            };

            (var actualFile1, var expectedFile1) = prepareFile(@"_editor\config1", "Sort");
            (var actualFile2, var expectedFile2) = prepareFile(@"_editor\config2", "Sort");

            var formatter = new ResxFormatter(settings, new FakeLog());
            Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-Us");

            // Act
            formatter.Run(actualFile1);
            formatter.Run(actualFile2);

            // Assert
            var actual1 = File.ReadAllText(actualFile1);
            var expected1 = File.ReadAllText(expectedFile1);
            // Check.WithCustomMessage("config1 is applied correctly").That(actual1).Equals(expected1);

            var actual2 = File.ReadAllText(actualFile2);
            var expected2 = File.ReadAllText(expectedFile2);
            // Check.WithCustomMessage("config2 is applied correctly").That(actual2).Equals(expected2);
        }

        [Theory]
        [ClassData(typeof(ResxTestData))]
        public void Files_are_processed_correctly(string message, string fileName, string culture, IFormatSettings settings)
        {
            // Arrange
            var baseFileName = Path.GetFileNameWithoutExtension(fileName);
            var actualFile = $"_files\\{baseFileName}-actual.resx";
            var expectedFile = $"_files\\{baseFileName}-expected.resx";

            var formatter = new ResxFormatter(settings, new FakeLog());
            var file = $"_files\\{fileName}";
            File.Copy(file, actualFile, true);

            var activeCulture = culture ?? "en-Us";
            Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo(activeCulture);

            // Act
            formatter.Run(actualFile);

            // Assert
            var actual = File.ReadAllText(actualFile);
            var expected = File.ReadAllText(expectedFile);
            Check.WithCustomMessage(message).That(actual).Equals(expected);
        }

        private static (string actual, string expected) prepareFile(string folder, string baseFileName, string extension = "resx")
        {
            var file = Path.Combine(folder, $"{baseFileName}.{extension}");
            var actualFile = Path.Combine(folder, $"{baseFileName}-actual.{extension}");
            var expectedFile = Path.Combine(folder, $"{baseFileName}-expected.{extension}");

            File.Copy(file, actualFile, true);
            return (actualFile, expectedFile);
        }

        internal class ResxTestData : TheoryDataBase<string, string, string, IFormatSettings>
        {
            public override IEnumerable<(string, string, string, IFormatSettings)> Create()
            {
                var sortAndRemoveDocumentation = new FakeSettings
                {
                    SortEntries = true,
                    RemoveDocumentationComment = true
                };

                yield return ("Culture should not impact sorting", "InvariantCulture.resx", "et", sortAndRemoveDocumentation);
                yield return ("Additional xml comments are kept.", "AdditionalXmlComments.resx", null, sortAndRemoveDocumentation);
                yield return ("Comment is removed even if no sorting is required.", "AlreadySorted.resx", null, sortAndRemoveDocumentation);
                yield return ("Data nodes appear after meta data nodes.", "Mixed.resx", null, sortAndRemoveDocumentation);
                yield return ("Entries are sorted alphabetically.", "Sort.resx", null, sortAndRemoveDocumentation);
                yield return ("File remains untouched if no modification is necessary.", "NoModificationNeeded.resx", null, sortAndRemoveDocumentation);
                // TODO xml comments should retain their original position
                yield return ("Invalid resx files are not touched.", "InvalidResx.resx", null, sortAndRemoveDocumentation);
                yield return ("Meta data is sorted too.", "MetaData.resx", null, sortAndRemoveDocumentation);
                yield return ("Plain xml files are not touched.", "Plain.xml", null, sortAndRemoveDocumentation);
                yield return ("Comment nodes are kept.", "WithResxComments.resx", null, sortAndRemoveDocumentation);

                yield return ("Entries are only sorted if 'sort' setting is active.", "DoNotSort.resx", null, new FakeSettings
                {
                    SortEntries = false,
                    RemoveDocumentationComment = true
                });

                yield return ("Documentation is only removed if 'doc' setting is active.", "KeepComments.resx", null, new FakeSettings
                {
                    SortEntries = true,
                    RemoveDocumentationComment = false
                });

                yield return ("Documentation is only removed if 'doc' setting is active.", "DoNothing.resx", null, new FakeSettings
                {
                    SortEntries = false,
                    RemoveDocumentationComment = false
                });
            }
        }
    }
}