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

            var menuCommandID = new CommandID(PackageGuids.guidResxFormatterPackageCmdSet, PackageIds.FormatAllCommandId);
            var menuItem = new MenuCommand(this.Execute, menuCommandID);
            commandService.AddCommand(menuItem);
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

        private void Execute(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var solution = Environment?.Solution;
            if (solution is object)
            {
                var solutionPath = Path.GetDirectoryName(solution.FullName);
                this.package.JoinableTaskFactory.RunAsync(async () =>
                {
                    await Task.Run(() => { FormatAllFiles(solutionPath); });
                    Log.Current.WriteLine("Success: All files processed.");
                }, JoinableTaskCreationOptions.LongRunning);
            }
        }
    }
}