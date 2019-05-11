namespace ResxFormatter
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Xml.Linq;

    public class ResxFormatter
    {
        public void Run(String resxPath)
        {
            var hasCommentRemoved = false;
            var toSave = new List<XNode>();
            var toSort = new List<XElement>();
            var document = XDocument.Load(resxPath);            

            foreach (var node in document.Root.Nodes())
            {
                if (!hasCommentRemoved && node.NodeType == System.Xml.XmlNodeType.Comment)
                {
                    hasCommentRemoved = true;
                    continue;
                }

                if (node is XElement element && element. Name.LocalName == "data")
                {
                    toSort.Add(element);
                }
                else
                {
                    toSave.Add(node);
                }
            }

            var sorted = toSort.OrderBy(e => e.Attribute("name").Value).ToList();
            var requiresSorting = !toSort.SequenceEqual(sorted);
            if (hasCommentRemoved || requiresSorting)
            {
                toSave.AddRange(sorted);
                document.Root.ReplaceNodes(toSave);
                document.Save(resxPath);
            }
        }
    }
}
