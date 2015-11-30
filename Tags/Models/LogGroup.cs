using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Xml.Linq;

namespace Tags.Models
{
    public class LogGroup
    {
        private XNamespace ns;

        public string Name
        {
            get { return node.Element(ns+"Name").Value; }
            set { node.Element(ns+"Name").Value = value; }
        }

        public string TableName
        {
            get { return node.Element(ns+"TableName").Value; }
            set { node.Element(ns+"TableName").Value = value; }
        }

        private XFactory xml;
        public XElement node { get { return xml.node; } }

        public LogGroup(XElement project, string name, string table)
        {
            xml = new XFactory(project, "LogGroup", name);
            ns = xml.node.Name.Namespace;
            TableName = table;
        }
    }
}