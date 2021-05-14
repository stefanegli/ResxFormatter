using System;
using System.Reflection;
using System.Resources;

namespace ResxFormatter
{
    public class ResxWriterFix
    {
        private bool isActive;

        public bool IsActive
        {
            get => this.isActive;
            set
            {
                if (this.isActive != value)
                {
                    var verb = value ? "Fixing" : "Restoring";
                    Log.Current.WriteLine($"{verb} ResXResourceWriter.");
                    this.FixResxWriter(value);
                }

                this.isActive = value;
            }
        }

        private string OriginalSchema { get; set; }

        private void FixResxWriter(bool fixIt)
        {
            var field = typeof(ResXResourceWriter).GetField("ResourceSchema", BindingFlags.Static | BindingFlags.Public);
            if (field != null)
            {
                if (fixIt)
                {
                    // remove the comment from the schema as it only bloats the resource files
                    if (field.GetValue(null) is string schema)
                    {
                        if (this.OriginalSchema is null)
                        {
                            this.OriginalSchema = schema;
                        }

                        var endOfComment = schema.IndexOf("-->", StringComparison.Ordinal);
                        if (endOfComment > 0)
                        {
                            schema = schema.Substring(endOfComment + 3);
                            field.SetValue(null, schema);
                        }
                    }
                }
                else
                {
                    field.SetValue(null, this.OriginalSchema);
                }
            }
        }
    }
}