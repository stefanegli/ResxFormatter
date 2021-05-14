using System;

namespace ResxFormatter
{
    public interface ILog
    {
        bool IsActive { get; set; }

        void Write(Exception ex);

        void WriteLine(string message);
    }
}