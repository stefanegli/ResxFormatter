﻿namespace ResxFormatter
{
    using Microsoft.VisualStudio.Shell;
    using Microsoft.VisualStudio.Shell.Interop;

    using System;

    public class Log : ILog
    {
        private static IVsOutputWindowPane outputPane;

        private Log()
        {
        }

        public static ILog Current { get; } = new Log();

        public bool IsActive { get; set; } = true;

        private static IVsOutputWindowPane OutputPane
        {
            get
            {
                ThreadHelper.ThrowIfNotOnUIThread();
                if (outputPane == null)
                {
                    outputPane = CreateOutputPane();
                }

                return outputPane;
            }
        }

        public void Write(Exception ex)
        {
            this.WriteLine(ex.ToString());
        }

        public void WriteLine(string message)
        {
            if (!this.IsActive)
            {
                return;
            }

            if (ThreadHelper.CheckAccess())
            {
                WriteLineInternal(message);
            }
            else
            {
                ThreadHelper.JoinableTaskFactory.StartOnIdle(() =>
                {
                    WriteLineInternal(message);
                });
            }
        }

        private static IVsOutputWindowPane CreateOutputPane()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var outWindow = Package.GetGlobalService(typeof(SVsOutputWindow)) as IVsOutputWindow;

            var guid = Guid.Parse("{4DDD4974-C22A-4D9A-B148-3594680AAC76}");
            outWindow.CreatePane(ref guid, Vsix.Name, 1, 1);
            outWindow.GetPane(ref guid, out var generalPane);
            return generalPane;
        }

        private static void WriteLineInternal(string message)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var line = $"[{DateTime.Now.ToLongTimeString()}] {message}{Environment.NewLine}";
            OutputPane?.OutputString(line);
        }
    }
}