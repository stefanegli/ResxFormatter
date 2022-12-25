namespace ResxFormatter.Commands
{
    using EnvDTE;

    using EnvDTE80;

    using Microsoft.VisualStudio.Shell;

    using System;
    using System.ComponentModel.Design;
    using System.IO;

    using Task = System.Threading.Tasks.Task;

    internal sealed class FormatAllCommand
    {
        public const int CommandId = 0x0100;

        public static readonly Guid CommandSet = new Guid("c7e059c9-69b5-443d-88f4-930c6a3975ca");

        private readonly ResxFormatterPackage package;

        private FormatAllCommand(ResxFormatterPackage package, OleMenuCommandService commandService)
        {
            this.package = package ?? throw new ArgumentNullException(nameof(package));
            commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

            var menuCommandID = new CommandID(CommandSet, CommandId);
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

        /// <summary>
        /// Gets the service provider from the owner package.
        /// </summary>
        private Microsoft.VisualStudio.Shell.IAsyncServiceProvider ServiceProvider
        {
            get
            {
                return this.package;
            }
        }

        public static async Task InitializeAsync(ResxFormatterPackage package)
        {
            // Switch to the main thread - the call to AddCommand in FormatAllCommand's constructor requires
            // the UI thread.
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

            OleMenuCommandService commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
            Instance = new FormatAllCommand(package, commandService);

            Environment = await package.GetServiceAsync(typeof(DTE)) as DTE2;
        }

        private void Execute(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var solution = Environment?.Solution;
            if (solution is object)
            {
                var solutionPath = Path.GetDirectoryName(solution.FullName);
                foreach (var file in Directory.EnumerateFiles(solutionPath, "*.resx", SearchOption.AllDirectories))
                {
                    var formatter = new ConfigurableResxFormatter(Log.Current);
                    formatter.Run(file);
                }
            }
        }
    }
}