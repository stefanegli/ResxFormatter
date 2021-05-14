namespace ResxFormatter
{
    using EnvDTE;

    using Microsoft.VisualStudio;
    using Microsoft.VisualStudio.Shell;
    using Microsoft.VisualStudio.Shell.Interop;

    using System;
    using System.IO;
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

        private ISettings Settings
        {
            get
            {
                if (settings == null)
                {
                    settings = (OptionPageGrid)GetDialogPage(typeof(OptionPageGrid));
                }

                ApplyEditorConfigSettings(settings);
                return settings;
            }
        }

        internal static void ApplyEditorConfigSettings(OptionPageGrid currentSettings)
        {
            var editorConfig = new ResxEditorConfigSettings(createResxFilePathForSettings());
            if (editorConfig.IsActive)
            {
                currentSettings.ConfigurationSource = ConfigurationSource.EditorConfig;
                currentSettings.SortEntries = editorConfig.SortEntries;
                currentSettings.RemoveDocumentationComment = editorConfig.RemoveDocumentationComment;
            }
            else
            {
                currentSettings.ConfigurationSource = ConfigurationSource.VisualStudio;
            }

            string createResxFilePathForSettings()
            {
                var fileName = "dummy.resx";
                var solutionPath = applicationObject?.Solution?.FullName;
                var solutionDir = string.IsNullOrWhiteSpace(solutionPath) ? null : Path.GetDirectoryName(solutionPath);
                return string.IsNullOrWhiteSpace(solutionDir) ? fileName : Path.Combine(solutionDir, fileName);
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

                Log.Current.WriteLine(this.Settings.ToString());
            }
        }

        private void OnDocumentSaved(Document document)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            var settings = this.Settings;
            if (document.Kind.ToUpperInvariant() == "{8E7B96A8-E33D-11D0-A6D5-00C04FB67F6A}"
                && document.FullName.ToUpperInvariant().EndsWith(".RESX"))
            {
                Log.Current.WriteLine("Save event for xml document received.");
                var formatter = new ResxFormatter(settings, Log.Current);
                if ((formatter.Run(document.FullName) && settings.ReloadFile == ReloadMode.Off)
                    || settings.ReloadFile == ReloadMode.Always)

                {
                    Log.Current.WriteLine("Reloading file.");
                    document.Close(vsSaveChanges.vsSaveChangesNo);
                    applicationObject.ItemOperations.OpenFile(document.FullName);
                }
            }
        }
    }
}