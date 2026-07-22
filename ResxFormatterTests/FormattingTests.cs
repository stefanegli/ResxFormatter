namespace ResxFormatterTests
{
    using NFluent;

    using ResxFormatter;

    using ResxFormatterTests.Fake;
    using ResxFormatterTests.TestFoundation;

    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Threading;

    using Xunit;

    public class FormattingTests
    {
        [Theory]
        [ClassData(typeof(ResxTestData))]
        public void Files_are_processed_correctly(string message, string fileName, string culture, IFormatSettings settings)
        {
            // Arrange
            var baseFileName = Path.GetFileNameWithoutExtension(fileName);
            var sourceFile = $"_files\\{fileName}";
            var expectedFile = $"_files\\{baseFileName}-expected.resx";
            using (var actualFile = TemporaryFile.Copy(sourceFile))
            {
                var formatter = new ResxFormatter(settings, new FakeLog());
                var originalCulture = Thread.CurrentThread.CurrentCulture;
                try
                {
                    Thread.CurrentThread.CurrentCulture = new CultureInfo(culture ?? "en-US");

                    // Act
                    formatter.Run(actualFile.Path);

                    // Assert
                    var actual = TextNormalization.NormalizeLineEndings(File.ReadAllText(actualFile.Path));
                    var expected = TextNormalization.NormalizeLineEndings(File.ReadAllText(expectedFile));
                    Check.WithCustomMessage(message).That(actual).Equals(expected);
                }
                finally
                {
                    Thread.CurrentThread.CurrentCulture = originalCulture;
                }
            }
        }

        internal class ResxTestData : TheoryDataBase<string, string, string, IFormatSettings>
        {
            public override IEnumerable<(string, string, string, IFormatSettings)> Create()
            {
                var sortAndRemoveDocumentation = new FakeSettings
                {
                    SortEntries = true,
                    RemoveXsdSchema = false,
                    RemoveDocumentationComment = true
                };

                yield return ("Culture should not impact sorting", "InvariantCulture.resx", "et", sortAndRemoveDocumentation);
                yield return ("Additional xml comments are kept.", "AdditionalXmlComments.resx", null, sortAndRemoveDocumentation);
                yield return ("Comment is removed even if no sorting is required.", "AlreadySorted.resx", null, sortAndRemoveDocumentation);
                yield return ("Data and metadata nodes are grouped and sorted.", "Mixed.resx", null, sortAndRemoveDocumentation);
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

                yield return ("No formatter option means no rewrite.", "DoNothing.resx", null, new FakeSettings
                {
                    SortEntries = false,
                    RemoveDocumentationComment = false
                });
            }
        }
    }
}
