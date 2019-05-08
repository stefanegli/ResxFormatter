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
            var sorted = new List<XElement>();
            var toSort = new List<XElement>();
            var document = XDocument.Load(resxPath);
            foreach (var element in document.Root.Elements())
            {
                if (element.NodeType == System.Xml.XmlNodeType.Comment)
                {
                    break;
                }

                if (element.Name.LocalName == "data")
                {
                    toSort.Add(element);
                }
                else
                {
                    sorted.Add(element);
                }
            }

            sorted.AddRange(toSort.OrderBy(e => e.Attribute("name").Value));
            document.Root.ReplaceNodes(sorted);
            document.Save(resxPath);
        }
    }
}
