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
            var toSave = new List<XElement>();
            var toSort = new List<XElement>();
            var document = XDocument.Load(resxPath);
            foreach (var element in document.Root.Elements())
            {
                if (element.NodeType == System.Xml.XmlNodeType.Comment)
                {
                    hasCommentRemoved = true;
                    break;
                }

                if (element.Name.LocalName == "data")
                {
                    toSort.Add(element);
                }
                else
                {
                    toSave.Add(element);
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
