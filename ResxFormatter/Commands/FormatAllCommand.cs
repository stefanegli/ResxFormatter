namespace ResxFormatter.Commands
{
    using EnvDTE;

    using EnvDTE80;

    using Microsoft.VisualStudio.Shell;
    using Microsoft.VisualStudio.TaskStatusCenter;

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

            var commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
            Instance = new FormatAllCommand(package, commandService);
            Environment = await package.GetServiceAsync(typeof(DTE)) as DTE2;
        }

        public static bool SkipFile(string filePath) => filePath is null || filePath.Contains(@"\bin\") || filePath.Contains(@"\obj\");

        private static async Task FormatAllFiles(string solutionPath, TaskProgressData data, ITaskHandler handler)
        {
            await Task.Run(() =>
            {
                foreach (var file in Directory.EnumerateFiles(solutionPath, "*.resx", SearchOption.AllDirectories))
                {
                    if (SkipFile(file))
                    {
                        continue;
                    }

                    var formatter = new ConfigurableResxFormatter(Log.Current);
                    formatter.Run(file);

                    data.ProgressText = $"{Path.GetFileName(file)}";
                    handler.Progress.Report(data);
                }

                data.ProgressText = "Success: All files processed.";
                data.PercentComplete = 100;
                Log.Current.WriteLine(data.ProgressText);
                handler.Progress.Report(data);
            });
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
                var status = await this.package.GetServiceAsync(typeof(SVsTaskStatusCenterService)) as IVsTaskStatusCenterService;
                var options = new TaskHandlerOptions()
                {
                    Title = "Formatting all resx files",
                    ActionsAfterCompletion = CompletionActions.None
                };

                var data = new TaskProgressData()
                {
                    CanBeCanceled = true
                };

                var handler = status.PreRegister(options, data);
                handler.RegisterTask(FormatAllFiles(solutionPath, data, handler));
            });
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