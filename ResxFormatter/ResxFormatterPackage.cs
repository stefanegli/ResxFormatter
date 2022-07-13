namespace ResxFormatter
{
    using global::ResxFormatter.VisualStudio;

    using Microsoft.VisualStudio;
    using Microsoft.VisualStudio.Shell;
    using Microsoft.VisualStudio.Shell.Interop;

    using System;
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
        private static VsDocumentEvents documentEvents;
        private static OptionPageGrid settings;

        private ISettings Settings
        {
            get
            {
                if (settings == null)
                {
                    settings = (OptionPageGrid)this.GetDialogPage(typeof(OptionPageGrid));
                }

                return settings;
            }
        }

        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            applicationObject = (EnvDTE80.DTE2)await this.GetServiceAsync(typeof(SDTE));
            if (applicationObject is null)
            {
                throw new InvalidOperationException("Failed to get DTE2 instance.");
            }

            documentEvents = new VsDocumentEvents();
            documentEvents.Saved += this.OnDocumentSaved;
        }

        private void OnDocumentSaved(object sender, VsDocument document)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            if (document.IsResx)
            {
                Log.Current.WriteLine("Save event for xml document received: " + document.Path);
                var formatter = new ConfigurableResxFormatter(Log.Current);
                formatter.Run(document.Path);
                if ((formatter.IsFileChanged && settings.ReloadFile == ReloadMode.AfterModification)
                    || settings.ReloadFile == ReloadMode.Always)

                {
                    Log.Current.WriteLine("Reloading file.");
                    document.Close();

                    Task.Run(async () =>
                    {
                        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                        applicationObject.ItemOperations.OpenFile(document.Path);
                    });
                }
            }
        }
    }
}