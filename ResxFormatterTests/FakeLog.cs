namespace ResxFormatterTests
{
    using ResxFormatter;

    using System;

    internal class FakeLog : ILog
    {
        public bool IsActive { get; set; }

        public void Write(Exception ex)
        {
        }

        public void WriteLine(string message)
        {
        }
    }
}