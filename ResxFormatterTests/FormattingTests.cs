namespace ResxFormatterTests
{
    using NFluent;
    using ResxFormatter;
    using ResxFormatterTests.TestFoundation;
    using System.Collections.Generic;
    using System.IO;
    using System.Security.Cryptography;
    using System.Text;
    using System.Threading;
    using Xunit;

    public class FormattingTests
    {
        [Theory]
        [ClassData(typeof(ResxTestData))]
        public void Files_are_processed_correctly(ISettings settings, string message, string fileName, string culture, string expectedHash)
        {
            // Arrange
            var tempFileName = $"_files\\{Path.GetFileNameWithoutExtension(fileName)}-actual.resx";
            var expectedFileName = $"_files\\{Path.GetFileNameWithoutExtension(fileName)}-expected.resx";

            var formatter = new ResxFormatter(settings, new FakeLog());
            var file = $"_files\\{fileName}";
            File.Copy(file, tempFileName, true);

            var activeCulture = culture ?? "en-Us";
            Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo(activeCulture);

            // Act
            formatter.Run(tempFileName);

            // Assert
            Check.WithCustomMessage(message + $" Result File: {tempFileName}").That(Sha256(tempFileName)).Equals(expectedHash);

            var actualLines = File.ReadAllLines(tempFileName);
            var expectedLines = File.ReadAllLines(expectedFileName);
            Check.That(actualLines).Equals(expectedLines);
        }

        private string Sha256(string path)
        {
            using (var stream = File.OpenRead(path))
            using (var sha = new SHA256Managed())
            {
                var result = new StringBuilder();
                byte[] hash = sha.ComputeHash(stream);
                foreach (byte hashByte in hash)
                {
                    result.Append(hashByte.ToString("X2"));
                }

                return result.ToString();
            }
        }

        internal class ResxTestData : TheoryDataBase<ISettings, string, string, string, string>
        {
            public override IEnumerable<(ISettings, string, string, string, string)> Create()
            {
                var @default = new FakeSettings
                {
                    SortEntries = true,
                    RemoveDocumentationComment = true
                };

                yield return (@default, "Culture", "InvariantCulture.resx", "et", "2B44847918C2EE7F2D7803BA130F6B5E13AEA28C32758A3E0BA2BEF7F51DF5A1");
                yield return (@default, "Additional xml comments are kept.", "AdditionalXmlComments.resx", null, "7A527237DE60E3C9308022A2EAC846427F5F55B021438EBD59CE3D480D3EDBCB");
                yield return (@default, "Comment is removed even if no sorting is required.", "AlreadySorted.resx", null, "C63EB8803EA8CCEC3A4A3B81B07E12A074CCF9093EEA50DF935F6624388BD117");
                yield return (@default, "Data nodes appear after meta data nodes.", "Mixed.resx", null, "14D1520823028EB6505DFF8591CDF43B76128EA7C0F13A7EEC250EE66A49B445");
                yield return (@default, "Entries are sorted alphabetically.", "Sort.resx", null, "6F32E0754A956B70FB7C09E29C47CAA38039A365FBE498A68DA73E698DA5D5DF");
                yield return (@default, "File remains untouched if no modification is necessary.", "NoModificationNeeded.resx", null, "10CFCF38BC20667B5163CBE310DD09AAB821A653DFCA04CC720F0A6F3349FD21");
                // TODO xml comments should retain their original position
                yield return (@default, "Invalid resx files are not touched.", "InvalidResx.resx", null, "45DC5F4936DEA6FB3AC19465900F3237A2AA1A60C86D7113CADDD4B41C9805D0");
                yield return (@default, "Meta data is sorted too.", "MetaData.resx", null, "4B04A955A37723EC38CF5C5F45B279F95DBABBB1A0CC38A90C9B079D5BAFF7B2");
                yield return (@default, "Plain xml files are not touched.", "Plain.xml", null, "9A56A89CF34541F785EE4F93A3BC754A6EB5632F4F6ABA3427A555BBD119827E");
                yield return (@default, "Comment nodes are kept.", "WithResxComments.resx", null, "FA02F7FCCC956A3424EF4F30CF9ED2D097C0B74EBEFC4ABE27FCE56D0FF3B910");

                var noSort = new FakeSettings
                {
                    SortEntries = false,
                    RemoveDocumentationComment = true
                };

                yield return (noSort, "Entries are only sorted if 'sort' setting is active.", "DoNotSort.resx", null, "2C4EFA8D11C3B6AE7308FFB182B1794E6D61106B5771B120B06C559C0D1E52DC");

                var keepComment = new FakeSettings
                {
                    SortEntries = true,
                    RemoveDocumentationComment = false
                };

                yield return (keepComment, "Documentation is only removed if 'doc' setting is active.", "KeepComments.resx", null, "E47BF6D02610D33256E4A0E913669CF4A29BB9ED0FEB8DC43628277E960092B8");

                var doNothing = new FakeSettings
                {
                    SortEntries = false,
                    RemoveDocumentationComment = false
                };

                yield return (doNothing, "Documentation is only removed if 'doc' setting is active.", "DoNothing.resx", null, "D9643216D1B2B631BF12C0A3742C570AD460DB634636FB58FD9B98D8565A61F4");
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