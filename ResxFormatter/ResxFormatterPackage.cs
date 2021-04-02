namespace ResxFormatter
{
    using EnvDTE;

    using Microsoft.VisualStudio;
    using Microsoft.VisualStudio.Shell;
    using Microsoft.VisualStudio.Shell.Interop;

    using System;
    using System.Reflection;
    using System.Resources;
    using System.Runtime.InteropServices;
    using System.Threading;

    using Task = System.Threading.Tasks.Task;

    [Guid("40d1f52e-e828-4cca-8279-df4ccd348f09")]
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [ProvideAutoLoad(VSConstants.UICONTEXT.NoSolution_string, PackageAutoLoadFlags.BackgroundLoad)]
    [ProvideAutoLoad(VSConstants.UICONTEXT.SolutionExists_string, PackageAutoLoadFlags.BackgroundLoad)]
    [ProvideOptionPage(typeof(OptionPageGrid), Vsix.Name, OptionPageGrid.GeneralCategory, 0, 0, true)]
    public sealed class ResxFormatterPackage : AsyncPackage
    {
        private static EnvDTE80.DTE2 applicationObject;
        private static DocumentEvents documentEvents;
        private static Events events;
        private static OptionPageGrid settings;

        private static ILog Log { get; } = new Log();

        private ISettings Settings
        {
            get
            {
                if (settings == null)
                {
                    settings = (OptionPageGrid)GetDialogPage(typeof(OptionPageGrid));
                }

                var editorConfig = new ResxEditorConfigSettings();
                if (editorConfig.IsActive)
                {
                    settings.ConfigurationSource = ConfigurationSource.EditorConfig;
                    settings.SortEntries = editorConfig.SortEntries;
                    settings.RemoveDocumentationComment = editorConfig.RemoveDocumentationComment;
                }
                else
                {
                    settings.ConfigurationSource = ConfigurationSource.VisualStudio;
                }

                return settings;
            }
        }

        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            // When initialized asynchronously, the current thread may be a background thread at this point.
            // Do any initialization that requires the UI thread after switching to the UI thread.
            await this.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);

            // avoid garbage collection
            applicationObject = await this.GetServiceAsync(typeof(SDTE)) as EnvDTE80.DTE2;
            if (applicationObject is object)
            {
                events = applicationObject.Events;
                documentEvents = events.DocumentEvents;
                documentEvents.DocumentSaved += this.OnDocumentSaved;

                if (this.Settings.FixResxWriter)
                {
                    Log.WriteLine("Fixing ResXResourceWriter.");
                    FixResxWriter();
                }
            }
        }

        private static void FixResxWriter()
        {
            var field = typeof(ResXResourceWriter).GetField("ResourceSchema", BindingFlags.Static | BindingFlags.Public);
            if (field != null)
            {
                // remove the comment from the schema as it only bloats the resource files
                if (field.GetValue(null) is string schema)
                {
                    var endOfComment = schema.IndexOf("-->", StringComparison.Ordinal);
                    if (endOfComment > 0)
                    {
                        schema = schema.Substring(endOfComment + 3);
                        field.SetValue(null, schema);
                    }
                }
            }
        }

        private void OnDocumentSaved(Document document)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            var settings = this.Settings;
            if (document.Kind.ToUpperInvariant() == "{8E7B96A8-E33D-11D0-A6D5-00C04FB67F6A}"
                && document.FullName.ToUpperInvariant().EndsWith(".RESX"))
            {
                Log.WriteLine("Save event for xml document received.");
                var formatter = new ResxFormatter(settings, Log);
                if ((formatter.Run(document.FullName) && settings.ReloadFile == ReloadMode.Off)
                    || settings.ReloadFile == ReloadMode.Always)

                {
                    Log.WriteLine("Reloading file.");
                    document.Close(vsSaveChanges.vsSaveChangesNo);
                    applicationObject.ItemOperations.OpenFile(document.FullName);
                }
            }
        }
    }
}