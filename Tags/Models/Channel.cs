using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Xml.Linq;

namespace Tags.Models
{
    public partial class Channel
    {
        private XFactory xml;
        public XElement node;
        public string Path { get; set; }

        private static string activeLinesQ = @"
            select distinct c.*
            from Tag t
            join Device d on d.DeviceId = t.DeviceId
            join Channel c on c.ChannelId = d.ChannelId
            where t.IsLogged = 1
            order by c.Name
        ";

        public static List<Channel> activeDevices()
        {
            List<Channel> lines;

            using (tagDB t = new tagDB())
            {
                lines = t.Fetch<Channel>(activeLinesQ);
            }
            return lines;
        }

        public Channel()
        { }

        public Channel(XElement project, string line)
        {
            this.Path = line.Trim();
            Name = line.Trim();

            xml = new XFactory(project, "Channel", Name);
            node = xml.node;

            using (tagDB tdb = new tagDB())
            {
                var chan = tdb.Fetch<Channel>("where Name = @0", Name).SingleOrDefault();
                if (chan == null) tdb.Insert<Channel>(this);
                else ChannelId = chan.ChannelId;
            }
        }
    }
}