using System;
using System.Reflection;
using System.Resources;

namespace ResxFormatter
{
    internal class ResxWriterFix
    {
        private bool isActive;

        public bool IsActive
        {
            get => this.isActive;
            set
            {
                if (this.isActive != value)
                {
                    if (value)
                    {
                        Log.Current.WriteLine("Fixing ResXResourceWriter.");
                        FixResxWriter();
                    }
                }

                this.isActive = value;
            }
        }

        private static void FixResxWriter()
        {
            var field = typeof(ResXResourceWriter).GetField("ResourceSchema", BindingFlags.Static | BindingFlags.Public);
            if (field != null)
            {
                // remove the comment from the schema as it only bloats the resource files
                if (field.GetValue(null) is string schema)
                {
                    var endOfComment = schema.IndexOf("-->", StringComparison.Ordinal);
                    if (endOfComment > 0)
                    {
                        schema = schema.Substring(endOfComment + 3);
                        field.SetValue(null, schema);
                    }
                }
            }
        }
    }
}