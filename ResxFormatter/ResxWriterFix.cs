using System;
using System.Reflection;
using System.Resources;

namespace ResxFormatter
{
    public class ResxWriterFix : IDisposable
    {
        private FixMode mode;

        public static string Original { get; } = ResXResourceWriter.ResourceSchema;
        public static string OriginalComment { get; } = Comment(ResXResourceWriter.ResourceSchema);
        public static string OriginalCommentContent { get; } = CommentContent(ResXResourceWriter.ResourceSchema);
        public static string OriginalSchema { get; } = Schema(ResXResourceWriter.ResourceSchema);

        public FixMode Mode
        {
            get => this.mode;
            set
            {
                if (this.mode != value)
                {
                    Log.Current.WriteLine($"Fixing ResXResourceWriter: {value}");
                    this.FixResxWriter(value);
                }

                this.mode = value;
            }
        }

        public static string Comment(string text)
        {
            var endOfComment = text.IndexOf("-->", StringComparison.Ordinal);
            return endOfComment > 0 ? text.Substring(0, endOfComment + 3) : text;
        }

        public static string CommentContent(string text)
        {
            var startOfComment = text.IndexOf("<!--", StringComparison.Ordinal);
            var endOfComment = text.IndexOf("-->", StringComparison.Ordinal);
            return startOfComment > 0 && endOfComment > 0 ? text.Substring(startOfComment + 4, endOfComment - startOfComment - 4) : text;
        }

        public static string Schema(string text)
        {
            var endOfComment = text.IndexOf("-->", StringComparison.Ordinal);
            return endOfComment > 0 ? text.Substring(endOfComment + 3).Trim() : text;
        }

        public void Dispose() => this.Mode = FixMode.Off;

        private void FixResxWriter(FixMode mode)
        {
            var field = typeof(ResXResourceWriter).GetField("ResourceSchema", BindingFlags.Static | BindingFlags.Public);
            if (field != null)
            {
                if (mode == FixMode.RemoveComment)
                {
                    field.SetValue(null, OriginalSchema);
                }
                else if (mode == FixMode.RemoveCommentAndSchema)
                {
                    field.SetValue(null, "<schema />");
                }
                else
                {
                    field.SetValue(null, Original);
                }
            }
        }
    }
}