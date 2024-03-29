﻿namespace ResxFormatter
{
    using Microsoft.VisualStudio.Shell;

    using System.ComponentModel;
    using System.Windows.Forms;

    internal class OptionPageGrid : DialogPage, ISettings
    {
        internal const string ExperimentalCategory = "Experimental";
        internal const string FormattingCategory = "Formatting";
        internal const string GeneralCategory = "General";

        public OptionPageGrid()
        {
            this.Settings = new Settings();
        }

        [Category(ExperimentalCategory)]
        [DisplayName("Fix Resx Writer")]
        [Description("ATTENTION: Unwanted side effects possible: If enabled the ResXResourceWriter is tricked into not writing the 'documentation' comment and/or schema element. This is achived by modifying a static string field through reflection.")]
        [ReadOnly(false)]
        [DefaultValue(FixMode.Off)]
        public FixMode FixResxWriterMode
        {
            get => this.Settings.FixResxWriterMode;
            set => this.Settings.FixResxWriterMode = value;
        }

        [Category(GeneralCategory)]
        [DisplayName("Reload file after saving")]
        [Description("Determines whether or not file is closed and re-opened if changes were my made by the extension so that they become immediately visible.")]
        [ReadOnly(false)]
        [DefaultValue(ReloadMode.AfterModification)]
        public ReloadMode ReloadFile
        {
            get => this.Settings.ReloadFile;
            set => this.Settings.ReloadFile = value;
        }

        private Settings Settings { get; }

        public override string ToString() => $"{nameof(this.FixResxWriterMode)}={this.FixResxWriterMode}";

        protected override void OnActivate(CancelEventArgs e)
        {
            if (this.Window is PropertyGrid grid)
            {
                grid.Refresh();
            }
        }
    }
}