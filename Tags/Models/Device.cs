using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Linq;

namespace Tags.Models
{
	public partial class Device
	{
        public string Path { get; set; }
        private XFactory xml;
        public XElement node { get { return xml.node; } }

        public Device()
        { }

        public Device(Channel channel, string deviceName, string ipAddress, string deviceModel)
        {
            Name = deviceName.Trim();
            Path = channel.Path + "." + Name;
            IPAddress = ipAddress.Trim();
            ChannelId = channel.ChannelId;
            deviceModel = string.IsNullOrWhiteSpace(deviceModel) ? null : deviceModel;

            xml = new XFactory(channel.node, "Device", Name);
            var ns = xml.node.Name.Namespace;
            node.Element(ns+"Name").Value = deviceName;
            node.Element(ns+"ID").Value = IPAddress;

            using (tagDB tdb = new tagDB())
            {
                var dev = tdb.Fetch<Device>("where Name = @0 and ChannelId = @1", Name, ChannelId).SingleOrDefault();
                if (dev == null) tdb.Insert<Device>(this);
                else
                {
                    DeviceId = dev.DeviceId;
                    Model = deviceModel ?? dev.Model;
                    this.Update();
                }
            }
            node.Element(ns + "ModelInfo").Descendants().First().Value = Model;
        }
    }
}