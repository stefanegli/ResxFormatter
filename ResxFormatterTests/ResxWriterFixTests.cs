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
        public void The_fix_can_be_turned_on_and_off_again()
        {
            using var fix = new ResxWriterFix();

            // Arrange
            var original = @".\_files\ResxWriterFix-original.resx";
            var active = @".\_files\ResxWriterFix-active.resx";
            var inactive = @".\_files\ResxWriterFix-inactive.resx";

            File.Delete(original);
            File.Delete(active);
            File.Delete(inactive);

            // Act
            using (var resx = new ResXResourceWriter(original))
            {
                resx.AddResource("a", "x");
            }

            fix.IsActive = true;
            using (var resx = new ResXResourceWriter(active))
            {
                resx.AddResource("a", "x");
            }

            fix.IsActive = false;
            Thread.Sleep(10);
            using (var resx = new ResXResourceWriter(inactive))
            {
                resx.AddResource("a", "x");
            }

            // Assert
            var originalLines = File.ReadAllLines(original);
            var activeLines = File.ReadAllLines(active);
            var inactiveLines = File.ReadAllLines(inactive);

            Check.That(fix.IsActive).IsFalse();
            Check.WithCustomMessage("active").That(activeLines).Equals(File.ReadAllLines(@".\_files\ResxWriterFix-active-expected.resx"));
            Check.WithCustomMessage("inactive").That(inactiveLines).Equals(File.ReadAllLines(@".\_files\ResxWriterFix-inactive-expected.resx"));
            Check.WithCustomMessage("original").That(originalLines).Equals(inactiveLines);
        }
    }
}