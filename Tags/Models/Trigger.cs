using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Linq;

namespace Tags.Models
{
	public class Trigger
	{
        private XNamespace ns;

        public string Name
        {
            get { return node.Element(ns+"Name").Value; }
            set { node.Element(ns+"Name").Value = value; }
        }

        public string minutes
        {
            get { return node.Element(ns+"minutes").Value; }
            set { node.Element(ns+"minutes").Value = value; }
        }

        public string LogOnInterval
        {
            get { return node.Element(ns+"LogOnInterval").Value; }
            set { node.Element(ns+"LogOnInterval").Value = value; }
        }

        public string LogOnEvent
        {
            get { return node.Element(ns+"LogOnEvent").Value; }
            set { node.Element(ns+"LogOnEvent").Value = value; }
        }

        private XFactory xml;
        public XElement node { get { return xml.node; } }

        /// <summary>
        /// Add or update trigger stanza.
        /// </summary>
        /// <param name="logGroup">Parent LogGroup</param>
        /// <param name="interval">Interval in minutes</param>
        /// <param name="onInterval">Flag true to trigger on interval</param>
        /// <param name="onChange">Flag to trigger on value change</param>
        public Trigger(XElement logGroup, int interval, bool onInterval, bool onChange)
        {
            xml = new XFactory(logGroup, "Trigger", "Trigger");
            ns = xml.node.Name.Namespace;
            minutes = interval.ToString();
            LogOnInterval = onInterval.ToString().ToLower();
            LogOnEvent = onChange.ToString().ToLower();
        }
	}
}