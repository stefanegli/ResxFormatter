namespace ResxFormatter
{
    using global::ResxFormatter.VisualStudio;

    using Microsoft.VisualStudio;
    using Microsoft.VisualStudio.Shell;
    using Microsoft.VisualStudio.Shell.Interop;
    using Microsoft.VisualStudio.Threading;

    using System;
    using System.Runtime.InteropServices;
    using System.Threading;

    using Task = System.Threading.Tasks.Task;

    [Guid("40d1f52e-e828-4cca-8279-df4ccd348f09")]
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [ProvideAutoLoad(VSConstants.UICONTEXT.NoSolution_string, PackageAutoLoadFlags.BackgroundLoad)]
    [ProvideAutoLoad(VSConstants.UICONTEXT.SolutionExists_string, PackageAutoLoadFlags.BackgroundLoad)]
    [ProvideOptionPage(typeof(OptionPageGrid), Vsix.Name, OptionPageGrid.GeneralCategory, 0, 0, true)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
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
            await this.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
            var dte = await this.GetServiceAsync<SDTE, EnvDTE80.DTE2>(true, cancellationToken);
            if (dte is null)
            {
                throw new InvalidOperationException("Failed to get DTE2 instance.");
            }

            applicationObject = dte;

            documentEvents = new VsDocumentEvents();
            documentEvents.Saved += this.OnDocumentSaved;

            Log.Current.WriteLine(this.Settings.ToString());
            await Commands.FormatAllCommand.InitializeAsync(this);
        }

        private void OnDocumentSaved(object sender, VsDocument document)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            Log.Current.WriteLine("Save event received: " + document.Path);
            var formatter = new ConfigurableResxFormatter(Log.Current);
            formatter.Run(document.Path);
            if ((formatter.IsFileChanged && settings.ReloadFile == ReloadMode.AfterModification)
                || settings.ReloadFile == ReloadMode.Always)

            {
                Log.Current.WriteLine("Reloading file.");
                var documentPath = document.Path;
                document.Close();

                this.JoinableTaskFactory.RunAsync(async () =>
                {
                    await Task.Yield();
                    await this.JoinableTaskFactory.SwitchToMainThreadAsync();
                    applicationObject.ItemOperations.OpenFile(documentPath);
                }).FileAndForget("ResxFormatter/ReloadFile");
            }
        }
    }
}
