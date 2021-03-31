namespace ResxFormatterTests
{
    using NFluent;
    using ResxFormatter;
    using ResxFormatterTests.TestFoundation;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading;
    using Xunit;

    public class FormattingTests
    {
        [Theory]
        [ClassData(typeof(ResxTestData))]
        public void Files_are_processed_correctly(string message, string fileName, string culture, ISettings settings)
        {
            // Arrange
            string baseFileName = Path.GetFileNameWithoutExtension(fileName);
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

        internal class ResxTestData : TheoryDataBase<string, string, string, ISettings>
        {
            public override IEnumerable<(string, string, string, ISettings)> Create()
            {
                var @default = new FakeSettings
                {
                    SortEntries = true,
                    RemoveDocumentationComment = true
                };

                yield return ("Culture should not impact sorting", "InvariantCulture.resx", "et", @default);
                yield return ("Additional xml comments are kept.", "AdditionalXmlComments.resx", null, @default);
                yield return ("Comment is removed even if no sorting is required.", "AlreadySorted.resx", null, @default);
                yield return ("Data nodes appear after meta data nodes.", "Mixed.resx", null, @default);
                yield return ("Entries are sorted alphabetically.", "Sort.resx", null, @default);
                yield return ("File remains untouched if no modification is necessary.", "NoModificationNeeded.resx", null, @default);
                // TODO xml comments should retain their original position
                yield return ("Invalid resx files are not touched.", "InvalidResx.resx", null, @default);
                yield return ("Meta data is sorted too.", "MetaData.resx", null, @default);
                yield return ("Plain xml files are not touched.", "Plain.xml", null, @default);
                yield return ("Comment nodes are kept.", "WithResxComments.resx", null, @default);

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

            private class FakeSettings : ISettings
            {
                public bool FixResxWriter => false;
                public ReloadMode ReloadFile => ReloadMode.Off;
                public bool RemoveDocumentationComment { get; set; }
                public bool SortEntries { get; set; }
            }
        }
    }
}