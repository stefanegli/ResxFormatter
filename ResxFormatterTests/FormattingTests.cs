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
        public void Entries_are_sorted_alphabetically()
        {
            // Arrange
            var formatter = new ResxFormatter();
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
            var formatter = new ResxFormatter();
            const string file = "_files\\NoModificationNeeded.resx";

            // Act
            formatter.Run(file);

            // Assert
            Check.That(Sha256(file)).Equals("10CFCF38BC20667B5163CBE310DD09AAB821A653DFCA04CC720F0A6F3349FD21");
        }


        [Fact]
        public void Comment_is_removed_even_if_no_sorting_is_required()
        {
            // Arrange
            var formatter = new ResxFormatter();
            const string file = "_files\\AlreadySorted.resx";

            // Act
            formatter.Run(file);

            // Assert
            Check.That(Sha256(file)).Equals("C63EB8803EA8CCEC3A4A3B81B07E12A074CCF9093EEA50DF935F6624388BD117");
        }

        [Fact]
        public void Resx_comment_nodes_are_kept()
        {
            // Arrange
            var formatter = new ResxFormatter();
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
