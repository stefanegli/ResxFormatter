namespace ResxFormatterTests
{
    using NFluent;

    using ResxFormatter;

    using System.IO;
    using System.Resources;
    using System.Threading;

    using Xunit;

    public class ResxWriterFixTests
    {
        public ResxWriterFixTests()
        {
            Log.Current.IsActive = false;
        }

        [Fact]
        public void Comment__returns_string_as_expected()
        {
            // Arrange
            var text = "  \n<!-- abc  -->\n efg";
            // Act
            var result = ResxWriterFix.Comment(text);

            // Assert
            Check.That(result).Equals("  \n<!-- abc  -->");
        }

        [Fact]
        public void CommentContent__returns_string_as_expected()
        {
            // Arrange
            var text = "  \n<!-- abc  -->\n efg12";
            // Act
            var result = ResxWriterFix.CommentContent(text);

            // Assert
            Check.That(result).Equals(" abc  ");
        }

        [Fact]
        public void Schema__returns_string_as_expected()
        {
            // Arrange
            var text = "  \n<!-- abc  -->\n<schema /> ";
            // Act
            var result = ResxWriterFix.Schema(text);

            // Assert
            Check.That(result).Equals("<schema />");
        }

        [Fact]
        public void The_fix_can_be_turned_on_and_off_again()
        {
            using var fix = new ResxWriterFix();

            // Arrange
            var original = @".\_files\ResxWriterFix-original.resx";
            var removeComment = @".\_files\ResxWriterFix-removeComment.resx";
            var removeCommentAndSchema = @".\_files\ResxWriterFix-removeCommentAndSchema.resx";
            var inactive = @".\_files\ResxWriterFix-inactive.resx";

            File.Delete(original);
            File.Delete(removeComment);
            File.Delete(removeCommentAndSchema);
            File.Delete(inactive);

            // Act
            using (var resx = new ResXResourceWriter(original))
            {
                resx.AddResource("a", "x");
            }

            fix.Mode = FixMode.RemoveComment;
            using (var resx = new ResXResourceWriter(removeComment))
            {
                resx.AddResource("a", "x");
            }

            fix.Mode = FixMode.RemoveCommentAndSchema;
            Thread.Sleep(10);
            using (var resx = new ResXResourceWriter(removeCommentAndSchema))
            {
                resx.AddResource("a", "x");
            }

            fix.Mode = FixMode.Off;
            Thread.Sleep(10);
            using (var resx = new ResXResourceWriter(inactive))
            {
                resx.AddResource("a", "x");
            }

            // Assert
            var originalLines = File.ReadAllLines(original);
            var removeCommentLines = File.ReadAllLines(removeComment);
            var removeCommentAndSchemaLines = File.ReadAllLines(removeCommentAndSchema);
            var inactiveLines = File.ReadAllLines(inactive);

            Check.That(fix.Mode).Equals(FixMode.Off);
            Check.WithCustomMessage("removeComment").That(removeCommentLines).Equals(File.ReadAllLines(@".\_files\ResxWriterFix-removeComment-expected.resx"));
            Check.WithCustomMessage("removeCommentAndSchema").That(removeCommentAndSchemaLines).Equals(File.ReadAllLines(@".\_files\ResxWriterFix-removeCommentAndSchema-expected.resx"));
            Check.WithCustomMessage("inactive").That(inactiveLines).Equals(File.ReadAllLines(@".\_files\ResxWriterFix-inactive-expected.resx"));
            Check.WithCustomMessage("original").That(originalLines).Equals(inactiveLines);
        }
    }
}