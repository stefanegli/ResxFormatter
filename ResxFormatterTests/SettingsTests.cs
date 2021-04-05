namespace ResxFormatterTests
{
    using NFluent;

    using ResxFormatter;

    using System.Collections.Generic;

    using Xunit;

    public class SettingsTests
    {
        [Fact]
        public void EditorConfig_settings_disable_formatting_settings()
        {
            // Arrange
            var host = new FakeSettingsHost();
            var settings = new Settings(host)
            {
                SortEntries = true,
                RemoveDocumentationComment = true,
                ConfigurationSource = ConfigurationSource.VisualStudio
            };

            Check.That(settings.FixResxWriter).IsFalse();
            Check.That(host.IsReadOnly(nameof(settings.SortEntries))).IsFalse();
            Check.That(host.IsReadOnly(nameof(settings.FixResxWriter))).IsFalse();
            Check.That(host.IsReadOnly(nameof(settings.RemoveDocumentationComment))).IsFalse();

            // Act
            settings.ConfigurationSource = ConfigurationSource.EditorConfig;

            // Assert
            Check.That(settings.FixResxWriter).IsFalse();
            Check.That(host.IsReadOnly(nameof(settings.SortEntries))).IsTrue();
            Check.That(host.IsReadOnly(nameof(settings.FixResxWriter))).IsFalse();
            Check.That(host.IsReadOnly(nameof(settings.RemoveDocumentationComment))).IsTrue();
        }

        [Fact]
        public void Fix_ResxWritter_is_not_possible_if_comment_is_not_to_be_removed()
        {
            // Arrange
            var host = new FakeSettingsHost();
            var settings = new Settings(host)
            {
                SortEntries = true,
                RemoveDocumentationComment = false,
                ConfigurationSource = ConfigurationSource.VisualStudio
            };

            Check.That(settings.FixResxWriter).IsFalse();
            Check.That(host.IsReadOnly(nameof(settings.SortEntries))).IsFalse();
            Check.That(host.IsReadOnly(nameof(settings.FixResxWriter))).IsFalse();
            Check.That(host.IsReadOnly(nameof(settings.RemoveDocumentationComment))).IsFalse();

            // Act
            settings.ConfigurationSource = ConfigurationSource.EditorConfig;

            // Assert
            Check.That(settings.FixResxWriter).IsFalse();
            Check.That(host.IsReadOnly(nameof(settings.SortEntries))).IsTrue();
            Check.That(host.IsReadOnly(nameof(settings.FixResxWriter))).IsTrue();
            Check.That(host.IsReadOnly(nameof(settings.RemoveDocumentationComment))).IsTrue();
        }

        [Fact]
        public void Fix_ResxWritter_is_set_to_false_if_comment_is_not_to_be_removed()
        {
            // Arrange
            var host = new FakeSettingsHost();
            var settings = new Settings(host)
            {
                SortEntries = true,
                FixResxWriter = true,
                RemoveDocumentationComment = false,
                ConfigurationSource = ConfigurationSource.VisualStudio
            };

            Check.That(settings.FixResxWriter).IsTrue();
            Check.That(host.IsReadOnly(nameof(settings.SortEntries))).IsFalse();
            Check.That(host.IsReadOnly(nameof(settings.FixResxWriter))).IsFalse();
            Check.That(host.IsReadOnly(nameof(settings.RemoveDocumentationComment))).IsFalse();

            // Act
            settings.ConfigurationSource = ConfigurationSource.EditorConfig;

            // Assert
            Check.That(settings.FixResxWriter).IsFalse();
            Check.That(host.IsReadOnly(nameof(settings.SortEntries))).IsTrue();
            Check.That(host.IsReadOnly(nameof(settings.FixResxWriter))).IsFalse();
            Check.That(host.IsReadOnly(nameof(settings.RemoveDocumentationComment))).IsTrue();
        }

        [Fact]
        public void Fix_ResxWritter_is_set_to_false_if_remove_comment_is_set_to_false_by_editor_config()
        {
            // Arrange
            var host = new FakeSettingsHost();
            var settings = new Settings(host)
            {
                SortEntries = true,
                FixResxWriter = true,
                RemoveDocumentationComment = true,
                ConfigurationSource = ConfigurationSource.EditorConfig
            };

            Check.That(settings.FixResxWriter).IsTrue();
            Check.That(host.IsReadOnly(nameof(settings.SortEntries))).IsTrue();
            Check.That(host.IsReadOnly(nameof(settings.FixResxWriter))).IsFalse();
            Check.That(host.IsReadOnly(nameof(settings.RemoveDocumentationComment))).IsTrue();

            // Act
            settings.RemoveDocumentationComment = false;

            // Assert
            Check.That(settings.FixResxWriter).IsFalse();
            Check.That(host.IsReadOnly(nameof(settings.SortEntries))).IsTrue();
            Check.That(host.IsReadOnly(nameof(settings.FixResxWriter))).IsTrue();
            Check.That(host.IsReadOnly(nameof(settings.RemoveDocumentationComment))).IsTrue();
        }

        internal class FakeSettingsHost : Dictionary<string, bool>, ISettingsHost
        {
            public bool IsReadOnly(string setting)
            {
                if (this.TryGetValue(setting, out bool value))
                {
                    return value;
                }

                return false;
            }

            public void SetReadOnly(string setting, bool value)
            {
                this[setting] = value;
            }
        }
    }
}