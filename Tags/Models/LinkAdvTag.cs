using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Xml.Linq;

namespace Tags.Models
{
    public class LinkTag
    {
        private XNamespace ns;

        public string Name
        {
            get { return node.Element(ns + "Name").Value; }
            set { node.Element(ns + "Name").Value = value; }
        }

        public string Input
        {
            get { return node.Element(ns + "Input").Value; }
            set { node.Element(ns + "Input").Value = value; }
        }

        public string Output
        {
            get { return node.Element(ns + "Output").Value; }
            set { node.Element(ns + "Output").Value = value; }
        }

        public string UpdateRateMS
        {
            get { return node.Element(ns + "UpdateRateMS").Value; }
            set { node.Element(ns + "UpdateRateMS").Value = value; }
        }

        private XFactory xml;
        public XElement node { get { return xml.node; } }


        public LinkTag(XElement advanced, string name, string input, string output, string rate)
        {
            xml = new XFactory(advanced, "LinkTag", name, "Name", null, "Tag");
            ns = xml.node.Name.Namespace;
            Input = input;
            Output = output;
            UpdateRateMS = rate;
        }
    }

    public class TagList
    {
        private XNamespace ns;

        public string Name
        {
            get { return node.Element(ns + "Name").Value; }
            set { node.Element(ns + "Name").Value = value; }
        }

        private XFactory xml;
        public XElement node { get { return xml.node; } }

        /// <summary>
        /// Add or update trigger stanza.
        /// </summary>
        /// <param name="dataChannel">Parent dataChannel</param>
        /// <param name="name">Name of TagGroup</param>
        public TagList(XElement dataChannel, string name)
        {
            xml = new XFactory(dataChannel, "TagGroup", name);
            ns = xml.node.Name.Namespace;
        }
    }
}