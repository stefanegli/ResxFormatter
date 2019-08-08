namespace ResxFormatterTests
{
    using NFluent;
    using ResxFormatter;
    using ResxFormatterTests.TestFoundation;
    using System.Collections.Generic;
    using System.IO;
    using System.Security.Cryptography;
    using System.Text;
    using Xunit;

    public class FormattingTests
    {
        [Theory]
        [ClassData(typeof(ResxTestData))]
        public void Files_are_processed_correctly(string message, string fileName, string expectedHash)
        {
            // Arrange
            var formatter = new ResxFormatter(new FakeLog());
            var file = $"_files\\{fileName}";

            // Act
            formatter.Run(file);

            // Assert
            Check.WithCustomMessage(message).That(Sha256(file)).Equals(expectedHash);
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

        internal class ResxTestData : TheoryDataBase<string, string, string>
        {
            public override IEnumerable<(string, string, string)> Create()
            {
                yield return ("Additional xml comments are kept.", "AdditionalXmlComments.resx", "7A527237DE60E3C9308022A2EAC846427F5F55B021438EBD59CE3D480D3EDBCB");
                yield return ("Comment is removed even if no sorting is required.", "AlreadySorted.resx", "C63EB8803EA8CCEC3A4A3B81B07E12A074CCF9093EEA50DF935F6624388BD117");
                yield return ("Data nodes appear after meta data nodes.", "Mixed.resx", "14D1520823028EB6505DFF8591CDF43B76128EA7C0F13A7EEC250EE66A49B445");
                yield return ("Entries are sorted alphabetically.", "Resource1.resx", "6F32E0754A956B70FB7C09E29C47CAA38039A365FBE498A68DA73E698DA5D5DF");
                yield return ("File remains untouched if no modification is necessary.", "NoModificationNeeded.resx", "10CFCF38BC20667B5163CBE310DD09AAB821A653DFCA04CC720F0A6F3349FD21");
                // TODO xml comments should retain their original position
                yield return ("Invalid resx files are not touched.", "InvalidResx.resx", "45DC5F4936DEA6FB3AC19465900F3237A2AA1A60C86D7113CADDD4B41C9805D0");
                yield return ("Meta data is sorted too.", "MetaData.resx", "4B04A955A37723EC38CF5C5F45B279F95DBABBB1A0CC38A90C9B079D5BAFF7B2");
                yield return ("Plain xml files are not touched.", "Plain.xml", "9A56A89CF34541F785EE4F93A3BC754A6EB5632F4F6ABA3427A555BBD119827E");
                yield return ("Comment nodes are kept.", "WithResxComments.resx", "FA02F7FCCC956A3424EF4F30CF9ED2D097C0B74EBEFC4ABE27FCE56D0FF3B910");
            }
        }
    }
}
