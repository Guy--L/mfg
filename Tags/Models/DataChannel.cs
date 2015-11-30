using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Xml.Linq;

namespace Tags.Models
{
    public class DataChannel
    {
        private XFactory xml;
        public XElement node;
        public string Name { get; set; }

        public DataChannel(XElement project, string line)
        {
            Name = line;

            xml = new XFactory(project, "Channel", Name, XFactory.template["ODBC"]);
            node = xml.node;
        }
    }
}