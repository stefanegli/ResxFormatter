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
        public void Files_are_processed_correctly(ISettings settings, string message, string fileName, string culture)
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
            Check.That(actual).Equals(expected);
        }

        internal class ResxTestData : TheoryDataBase<ISettings, string, string, string>
        {
            public override IEnumerable<(ISettings, string, string, string)> Create()
            {
                var @default = new FakeSettings
                {
                    SortEntries = true,
                    RemoveDocumentationComment = true
                };

                yield return (@default, "Culture", "InvariantCulture.resx", "et");
                yield return (@default, "Additional xml comments are kept.", "AdditionalXmlComments.resx", null);
                yield return (@default, "Comment is removed even if no sorting is required.", "AlreadySorted.resx", null);
                yield return (@default, "Data nodes appear after meta data nodes.", "Mixed.resx", null);
                yield return (@default, "Entries are sorted alphabetically.", "Sort.resx", null);
                yield return (@default, "File remains untouched if no modification is necessary.", "NoModificationNeeded.resx", null);
                // TODO xml comments should retain their original position
                yield return (@default, "Invalid resx files are not touched.", "InvalidResx.resx", null);
                yield return (@default, "Meta data is sorted too.", "MetaData.resx", null);
                yield return (@default, "Plain xml files are not touched.", "Plain.xml", null);
                yield return (@default, "Comment nodes are kept.", "WithResxComments.resx", null);

                var noSort = new FakeSettings
                {
                    SortEntries = false,
                    RemoveDocumentationComment = true
                };

                yield return (noSort, "Entries are only sorted if 'sort' setting is active.", "DoNotSort.resx", null);

                var keepComment = new FakeSettings
                {
                    SortEntries = true,
                    RemoveDocumentationComment = false
                };

                yield return (keepComment, "Documentation is only removed if 'doc' setting is active.", "KeepComments.resx", null);

                var doNothing = new FakeSettings
                {
                    SortEntries = false,
                    RemoveDocumentationComment = false
                };

                yield return (doNothing, "Documentation is only removed if 'doc' setting is active.", "DoNothing.resx", null);
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