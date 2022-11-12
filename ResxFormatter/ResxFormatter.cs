namespace ResxFormatter
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Xml;
    using System.Xml.Linq;

    public class ResxFormatter
    {
        public ResxFormatter(IFormatSettings settings, ILog log)
        {
            this.Log = log;
            this.Settings = settings;
        }

        public bool IsFileChanged { get; private set; }

        private ILog Log { get; }
        private IFormatSettings Settings { get; }

        public static bool HasDocumentationComment(XDocument document)
        {
            var firstComment = document.Root.Nodes().FirstOrDefault(n => n.NodeType == XmlNodeType.Comment) as XComment;
            if (firstComment == null)
            {
                return false;
            }

            var value = RemoveWhiteSpace(firstComment.ToString());
            var schema = RemoveWhiteSpace(ResxWriterFix.OriginalComment);
            return value == schema;

            string RemoveWhiteSpace(string text) => string.Join("", text.Split(default(string[]), StringSplitOptions.RemoveEmptyEntries));
        }

        /// <summary>
        /// Returns true if the given file was modified.
        /// </summary>
        public void Run(string resxPath)
        {
            this.IsFileChanged = this.FormatResx(resxPath);
        }

        private bool FormatResx(string resxPath)
        {
            var isResx = false;
            var hasSchemaRemoved = false;
            var hasCommentRemoved = false;
            var toSave = new List<XNode>();
            var toSort = new List<XElement>();
            var document = XDocument.Load(resxPath);

            foreach (var node in document.Root.Nodes())
            {
                if (this.Settings.RemoveXsdSchema)
                {
                    if (!hasSchemaRemoved && node.NodeType == XmlNodeType.Element)
                    {
                        hasSchemaRemoved = true;
                        continue;
                    }
                }

                if (this.Settings.RemoveDocumentationComment)
                {
                    if (!hasCommentRemoved && node.NodeType == XmlNodeType.Comment)
                    {
                        hasCommentRemoved = true;
                        continue;
                    }
                }

                if (node is XElement element && (element.Name.LocalName == "data" || element.Name.LocalName == "metadata"))
                {
                    toSort.Add(element);
                }
                else
                {
                    toSave.Add(node);

                    if (node is XElement e
                        && e.Name.LocalName == "resheader"
                        && e.Attribute("name").Value == "resmimetype"
                        && e.FirstNode.ToString() == "<value>text/microsoft-resx</value>")
                    {
                        isResx = true;
                    }
                }
            }

            var sorted = this.Settings.SortEntries
                ? toSort.OrderBy(e => e.Attribute("name").Value, this.Settings.Comparer)
                    .OrderBy(e => e.Name.ToString(), this.Settings.Comparer)
                    .ToList()
                : toSort;

            var hasCommentAdded = false;
            if (!this.Settings.RemoveDocumentationComment && !HasDocumentationComment(document))
            {
                toSave.Insert(0, new XComment(ResxWriterFix.OriginalCommentContent));
                hasCommentAdded = true;
            }

            var requiresSorting = this.Settings.SortEntries && !toSort.SequenceEqual(sorted);
            if (isResx && (hasSchemaRemoved || hasCommentRemoved || hasCommentAdded || requiresSorting))
            {
                toSave.AddRange(sorted);
                document.Root.ReplaceNodes(toSave);
                this.Log.WriteLine($"Updating {resxPath}");
                document.Save(resxPath);
                return true;
            }
            else
            {
                var reason = isResx ? "No modifications" : "Not a .resx file";
                this.Log.WriteLine($"Update was not required: {reason}.");
                return false;
            }
        }
    }
}