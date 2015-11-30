using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Web;
using System.Xml.Linq;

namespace Tags.Models
{
    public class XFactory
    {
        public static Dictionary<string, XElement> template;
        public static Dictionary<Type, XNamespace> nmspaces;
        public static string _projectPath;
        public static XElement current;
        
        /// <summary>
        /// Set up templates for building KEPServerEX Configuration File
        /// </summary>
        /// <param name="projPath">Place to write the configuration file</param>
        public static void Initialize(string projPath)
        {
            _projectPath = projPath;

            projectXML = XDocument.Load(Path.Combine(_projectPath, "KEPTemplate.xml"));
            ns = projectXML.Root.Name.Namespace;

            template = new Dictionary<string, XElement>();
            //nmspaces = new Dictionary<Type, XNamespace>();

            template.Add("Channel", XElement.Load(Path.Combine(_projectPath, "ChannelTemplate.xml")));
            template.Add("LogGroup", XElement.Load(Path.Combine(_projectPath, "LogGroupTemplate.xml")));
            template.Add("ODBC", XElement.Load(Path.Combine(_projectPath, "ODBCTemplate.xml")));
            template.Add("LinkTag", XElement.Load(Path.Combine(_projectPath, "LinkTagTemplate.xml")));

            var ch = template["Channel"].Name.Namespace;
            var lg = template["LogGroup"].Name.Namespace;
            var lt = template["LinkTag"].Name.Namespace;

            //nmspaces.Add(typeof(Channel), ch);
            //nmspaces.Add(typeof(LogGroup), lg);

            template.Add("Trigger", new XElement(template["LogGroup"].Descendants(lg + "Trigger").First()));
            template.Add("Device", new XElement(template["Channel"].Descendants(ch + "Device").First()));
            template.Add("Tag", new XElement(template["Device"].Descendants(ch + "Tag").First()));
            template.Add("TagGroup", new XElement(template["ODBC"].Descendants(ch + "TagGroup").First()));

            template["Channel"].Descendants(ch + "DeviceList").First().RemoveAll();
            template["LogGroup"].Descendants(lg + "TriggerList").First().RemoveAll();
            template["Device"].Descendants(ch + "TagList").First().RemoveAll();
            template["ODBC"].Descendants(ch + "TagGroupList").First().RemoveAll();
            template["TagGroup"].Descendants(ch + "TagList").First().RemoveAll();

            template.Add("LogItem", new XElement(template["LogGroup"].Descendants(lg+"LogItem").First()));
            template["LogGroup"].Descendants(lg+"LogItemList").First().RemoveAll();
        }

        public XElement node;
        static public XDocument projectXML;
        static public XNamespace ns;

        public XFactory(XElement scope, string level, string id, XElement template)
            : this(scope, level, id, "Name", template, level)
        { }

        public XFactory(XElement scope, string level, string id)
            : this(scope, level, id, "Name", XFactory.template[level], level)
        { }

        public XFactory(XElement scope, string level, string id, string elem, XElement template, string listprefix)
        {
            var desc = scope.DescendantsAndSelf().Where(e => e.Name.LocalName == level);                        // find point of insertion
            node = desc.Elements().Where(e => e.Value == id && e.Name.LocalName == elem).SingleOrDefault();     // find existing node to insert

            if (node == null)                                                       // node is not already inserted
            {
                node = new XElement(template??XFactory.template[level]);            // create it using template
                var ns = node.Name.Namespace;
                node.Element(ns+elem).Value = id;                                   // set id for the new node
                scope.Descendants(ns + listprefix + "List").First().Add(node);      // add it to list of nodes
            }
            else
            {
                node = node.Parent;
                //Debug.WriteLine("found " + level + ": " + id);
            }
        }
    }
}