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
            const string file = "Resource1.resx";

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
            const string file = "NoModificationNeeded.resx";

            // Act
            formatter.Run(file);

            // Assert
            Check.That(Sha256(file)).Equals("10CFCF38BC20667B5163CBE310DD09AAB821A653DFCA04CC720F0A6F3349FD21");
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
