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
            this.Settings = settings ?? throw new ArgumentNullException(nameof(settings));
        }

        public bool IsFileChanged { get; private set; }

        private ILog Log { get; }
        private IFormatSettings Settings { get; }

        public static bool HasDocumentationComment(XDocument document)
        {
            if (document is null)
            {
                throw new ArgumentNullException(nameof(document));
            }

            var schema = RemoveWhiteSpace(ResxSchemaDefaults.OriginalComment);
            return document.Root?.Nodes()
                .OfType<XComment>()
                .Any(comment => RemoveWhiteSpace(comment.ToString()) == schema) == true;

            string RemoveWhiteSpace(string text) => string.Join("", text.Split(default(string[]), StringSplitOptions.RemoveEmptyEntries));
        }

        public static bool HasSchemaNode(XDocument document)
        {
            if (document is null)
            {
                throw new ArgumentNullException(nameof(document));
            }

            return document.Root?.Elements().Any(IsXsdSchema) == true;
        }

        /// <summary>
        /// Returns true if the given file was modified.
        /// </summary>
        public void Run(string resxPath)
        {
            this.Run(resxPath, true);
        }

        public void Run(string resxPath, bool writeChanges)
        {
            this.IsFileChanged = false;
            this.IsFileChanged = this.FormatResx(resxPath, writeChanges);
        }

        private bool FormatResx(string resxPath, bool writeChanges)
        {
            var hasSchemaRemoved = false;
            var hasCommentRemoved = false;
            var toSave = new List<XNode>();
            var toSort = new List<XElement>();
            XDocument document;
            var readerSettings = new XmlReaderSettings
            {
                DtdProcessing = DtdProcessing.Prohibit,
                IgnoreWhitespace = true,
                XmlResolver = null
            };

            using (var reader = XmlReader.Create(resxPath, readerSettings))
            {
                document = XDocument.Load(reader);
            }

            var root = document.Root;
            if (!IsResx(root) || HasUnnamedResourceEntry(root))
            {
                this.Log?.WriteLine("Update was not required: Not a valid .resx file.");
                return false;
            }

            foreach (var node in root.Nodes())
            {
                if (this.Settings.RemoveXsdSchema)
                {
                    if (!hasSchemaRemoved && node is XElement e && IsXsdSchema(e))
                    {
                        toSave.Add(XElement.Parse(ResxSchemaDefaults.FakeSchema));
                        hasSchemaRemoved = true;
                        continue;
                    }
                }

                if (this.Settings.RemoveDocumentationComment)
                {
                    if (!hasCommentRemoved && node is XComment comment && IsDocumentationComment(comment))
                    {
                        hasCommentRemoved = true;
                        continue;
                    }
                }

                if (node is XElement element && IsResourceEntry(element))
                {
                    toSort.Add(element);
                }
                else
                {
                    toSave.Add(node);
                }
            }

            var sorted = this.Settings.SortEntries
                ? toSort.OrderBy(e => e.Name.ToString(), this.Settings.Comparer)
                    .ThenBy(e => e.Attribute("name").Value, this.Settings.Comparer)
                    .ToList()
                : toSort;

            var hasCommentAdded = false;
            if (!this.Settings.RemoveDocumentationComment && !HasDocumentationComment(document))
            {
                toSave.Insert(0, new XComment(ResxSchemaDefaults.OriginalCommentContent));
                hasCommentAdded = true;
            }

            var hasSchemaAdded = false;
            if (!this.Settings.RemoveXsdSchema && !HasSchemaNode(document))
            {
                toSave.Insert(1, XElement.Parse(ResxSchemaDefaults.OriginalSchema));
                hasSchemaAdded = true;
            }

            var requiresSorting = this.Settings.SortEntries && !toSort.SequenceEqual(sorted);
            if (hasSchemaRemoved || hasCommentRemoved || hasCommentAdded || hasSchemaAdded || requiresSorting)
            {
                toSave.AddRange(sorted);
                document.Root.ReplaceNodes(toSave);
                var action = writeChanges ? "Updating" : "Would update";
                this.Log?.WriteLine($"{action} {resxPath}");
                if (writeChanges)
                {
                    document.Save(resxPath);
                }

                return true;
            }
            else
            {
                this.Log?.WriteLine($"Skipping {resxPath}");
                return false;
            }
        }

        private static bool HasUnnamedResourceEntry(XElement root)
        {
            return root.Elements().Any(element =>
                IsResourceEntry(element) && element.Attribute("name") is null);
        }

        private static bool IsDocumentationComment(XComment comment)
        {
            return RemoveWhiteSpace(comment.ToString()) == RemoveWhiteSpace(ResxSchemaDefaults.OriginalComment);
        }

        private static bool IsXsdSchema(XElement element)
        {
            return element.Name == XName.Get("schema", "http://www.w3.org/2001/XMLSchema");
        }

        private static bool IsResourceEntry(XElement element)
        {
            return element.Name.Namespace == XNamespace.None
                && (element.Name.LocalName == "data" || element.Name.LocalName == "metadata");
        }

        private static bool IsResx(XElement root)
        {
            if (root?.Name != XName.Get("root"))
            {
                return false;
            }

            return root.Elements("resheader").Any(element =>
                (string)element.Attribute("name") == "resmimetype"
                && (string)element.Element("value") == "text/microsoft-resx");
        }

        private static string RemoveWhiteSpace(string text)
        {
            return string.Join("", text.Split(default(string[]), StringSplitOptions.RemoveEmptyEntries));
        }
    }
}
