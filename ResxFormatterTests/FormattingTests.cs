namespace ResxFormatterTests
{
    using NFluent;
    using ResxFormatter;
    using System.IO;
    using System.Security.Cryptography;
    using System.Text;
    using Xunit;

    public class FormattingTests
    {
        [Fact]
        public void Additional_xml_comments_are_kept()
        {
            // Arrange
            var formatter = new ResxFormatter(new FakeLog());
            const string file = "_files\\AdditionalXmlComments.resx";

            // Act
            formatter.Run(file);

            // Assert
            // TODO xml comments should retain their original position
            Check.That(Sha256(file)).Equals("7A527237DE60E3C9308022A2EAC846427F5F55B021438EBD59CE3D480D3EDBCB");
        }

        [Fact]
        public void Comment_is_removed_even_if_no_sorting_is_required()
        {
            // Arrange
            var formatter = new ResxFormatter(new FakeLog());
            const string file = "_files\\AlreadySorted.resx";

            // Act
            formatter.Run(file);

            // Assert
            Check.That(Sha256(file)).Equals("C63EB8803EA8CCEC3A4A3B81B07E12A074CCF9093EEA50DF935F6624388BD117");
        }

        [Fact]
        public void Data_nodes_appear_after_meta_data_nodes()
        {
            // Arrange
            var formatter = new ResxFormatter(new FakeLog());
            const string file = "_files\\Mixed.resx";

            // Act
            formatter.Run(file);

            // Assert
            // TODO xml comments should retain their original position
            Check.That(Sha256(file)).Equals("14D1520823028EB6505DFF8591CDF43B76128EA7C0F13A7EEC250EE66A49B445");
        }

        [Fact]
        public void Entries_are_sorted_alphabetically()
        {
            // Arrange
            var formatter = new ResxFormatter(new FakeLog());
            const string file = "_files\\Resource1.resx";

            // Act
            formatter.Run(file);

            // Assert
            Check.That(Sha256(file)).Equals("6F32E0754A956B70FB7C09E29C47CAA38039A365FBE498A68DA73E698DA5D5DF");
        }

        [Fact]
        public void File_remains_untouched_if_no_modification_is_necessary()
        {
            // Arrange
            var formatter = new ResxFormatter(new FakeLog());
            const string file = "_files\\NoModificationNeeded.resx";

            // Act
            formatter.Run(file);

            // Assert
            Check.That(Sha256(file)).Equals("10CFCF38BC20667B5163CBE310DD09AAB821A653DFCA04CC720F0A6F3349FD21");
        }

        [Fact]
        public void Invalid_resx_files_are_not_touched()
        {
            // Arrange
            var formatter = new ResxFormatter(new FakeLog());
            const string file = "_files\\InvalidResx.resx";

            // Act
            formatter.Run(file);

            // Assert
            // TODO xml comments should retain their original position
            Check.That(Sha256(file)).Equals("45DC5F4936DEA6FB3AC19465900F3237A2AA1A60C86D7113CADDD4B41C9805D0");
        }

        [Fact]
        public void Meta_data_is_sorted_too()
        {
            // Arrange
            var formatter = new ResxFormatter(new FakeLog());
            const string file = "_files\\MetaData.resx";

            // Act
            formatter.Run(file);

            // Assert
            // TODO xml comments should retain their original position
            Check.That(Sha256(file)).Equals("4B04A955A37723EC38CF5C5F45B279F95DBABBB1A0CC38A90C9B079D5BAFF7B2");
        }

        [Fact]
        public void Plain_xml_files_are_not_touched()
        {
            // Arrange
            var formatter = new ResxFormatter(new FakeLog());
            const string file = "_files\\Plain.xml";

            // Act
            formatter.Run(file);

            // Assert
            // TODO xml comments should retain their original position
            Check.That(Sha256(file)).Equals("9A56A89CF34541F785EE4F93A3BC754A6EB5632F4F6ABA3427A555BBD119827E");
        }

        [Fact]
        public void Resx_comment_nodes_are_kept()
        {
            // Arrange
            var formatter = new ResxFormatter(new FakeLog());
            const string file = "_files\\WithResxComments.resx";

            // Act
            formatter.Run(file);

            // Assert
            Check.That(Sha256(file)).Equals("FA02F7FCCC956A3424EF4F30CF9ED2D097C0B74EBEFC4ABE27FCE56D0FF3B910");
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
    }
}
