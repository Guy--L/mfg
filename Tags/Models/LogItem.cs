using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Xml.Linq;

namespace Tags.Models
{
	public class LogItem
	{
        private XNamespace ns;

        public string Name
        {
            get { return node.Element(ns+"Name").Value; }
            set { node.SetElementValue(ns+"Name", value); }
        }

        public string NumericID
        {
            get { return node.Element(ns+"NumericID").Value; }
            set { node.SetElementValue(ns+"NumericID", value); }
        }

        private XFactory xml;
        public XElement node { get { return xml.node; } }

        public LogItem(XElement logGroup, string path, int numericid)
        {
            xml = new XFactory(logGroup, "LogItem", path);
            ns = xml.node.Name.Namespace;
            NumericID = numericid.ToString();
            //Debug.WriteLine("LogItem " + Name + " ID: " + NumericID);
        }
	}
}