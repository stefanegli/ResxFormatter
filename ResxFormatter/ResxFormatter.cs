﻿namespace ResxFormatter
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Xml;
    using System.Xml.Linq;

    public class ResxFormatter
    {
        public ResxFormatter(ILog log)
        {
            this.Log = log;
        }

        private ILog Log { get; }

        public void Run(String resxPath)
        {
            var isResx = false;
            var hasCommentRemoved = false;
            var toSave = new List<XNode>();
            var toSort = new List<XElement>();
            var document = XDocument.Load(resxPath);

            foreach (var node in document.Root.Nodes())
            {
                if (!hasCommentRemoved && node.NodeType == XmlNodeType.Comment)
                {
                    hasCommentRemoved = true;
                    continue;
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

            var sorted = toSort.OrderBy(e => e.Attribute("name").Value).OrderBy(e => e.Name.ToString()).ToList();
            var requiresSorting = !toSort.SequenceEqual(sorted);
            if (isResx && (hasCommentRemoved || requiresSorting))
            {
                toSave.AddRange(sorted);
                document.Root.ReplaceNodes(toSave);
                this.Log.WriteLine($"Updating {resxPath}");
                document.Save(resxPath);
            }
            else
            {
                var reason = isResx ? "No modifications" : "Not a .resx file";
                this.Log.WriteLine($"Update was not required: {reason}.");
            }
        }
    }
}
