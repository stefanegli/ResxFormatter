namespace ResxFormatter.Commands
{
    using EnvDTE;

    using EnvDTE80;

    using Microsoft.VisualStudio.Shell;
    using Microsoft.VisualStudio.Threading;

    using System;
    using System.ComponentModel.Design;
    using System.IO;

    using Task = System.Threading.Tasks.Task;

    internal sealed class FormatAllCommand
    {
        private readonly ResxFormatterPackage package;

        private FormatAllCommand(ResxFormatterPackage package, OleMenuCommandService commandService)
        {
            this.package = package ?? throw new ArgumentNullException(nameof(package));
            commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

            var id = new CommandID(PackageGuids.guidResxFormatterPackageCmdSet, PackageIds.FormatAllCommandId);
            var command = new OleMenuCommand(this.Execute, id);
            command.BeforeQueryStatus += this.OnBeforeQueryStatus;

            commandService.AddCommand(command);
        }

        public static DTE2 Environment { get; private set; }

        /// <summary>
        /// Gets the instance of the command.
        /// </summary>
        public static FormatAllCommand Instance
        {
            get;
            private set;
        }

        public static async Task InitializeAsync(ResxFormatterPackage package)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

            OleMenuCommandService commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
            Instance = new FormatAllCommand(package, commandService);
            Environment = await package.GetServiceAsync(typeof(DTE)) as DTE2;
        }

        public static bool SkipFile(string filePath) => filePath is null || filePath.Contains(@"\bin\") || filePath.Contains(@"\obj\");

        private static void FormatAllFiles(string solutionPath)
        {
            foreach (var file in Directory.EnumerateFiles(solutionPath, "*.resx", SearchOption.AllDirectories))
            {
                if (SkipFile(file))
                {
                    continue;
                }

                //                for (int i = 0; i < 20; i++)
                //                {
                //                    System.Threading.Thread.Sleep(200);
                var formatter = new ConfigurableResxFormatter(Log.Current);
                formatter.Run(file);
            }
            //            }
        }

        private bool CanExecute()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var solution = Environment?.Solution;
            if (solution is null)
            {
                return false;
            }

            var fullName = solution.FullName;
            if (fullName is null || !File.Exists(fullName))
            {
                return false;
            }

            return true;
        }

        private void Execute(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            if (!this.CanExecute())
            {
                Log.Current.WriteLine("No solution available.");
                return;
            }

            var solutionPath = Path.GetDirectoryName(Environment?.Solution?.FullName);
            this.package.JoinableTaskFactory.RunAsync(async () =>
            {
                await Task.Run(() => { FormatAllFiles(solutionPath); });
                Log.Current.WriteLine("Success: All files processed.");
            }, JoinableTaskCreationOptions.LongRunning);
        }

        private void OnBeforeQueryStatus(object sender, EventArgs e)
        {
            var command = sender as OleMenuCommand;
            if (command is null)
            {
                return;
            }

            command.Enabled = this.CanExecute();
        }
    }
}