using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Xml.Linq;
using NPoco;

namespace Tags.Models
{
	public partial class Tag
	{
        private static string _syncCurrent = @"
            insert into [Current] (TagId, Name, Value, Stamp, SubMinute)
            select b.TagId, c.[Name]+'.'+d.[Name]+'.'+b.[Name] as [Name], '', getdate(), 0
            from [Current] a
            right join Tag b on a.TagId = b.TagId
            join Device d on b.DeviceId = d.DeviceId
            join Channel c on d.ChannelId = c.ChannelId
            where a.TagId is null
        ";

        private static string _unlog = @"
            UPDATE [dbo].[Tag]
               SET [IsLogged] = 0
             WHERE TagId in ({0})
        ";

        private static string _tagsByLines = @"
            select
            c.Name+'.'+d.Name+'.'+t.Name as [Path],
            t.TagId,
            t.DeviceId,
            t.Name,
            t.Address,
            c.Name as [Channel],
            d.Name as [Device]
            from Tag t
            left join Device d on t.DeviceId = d.DeviceId
            left join Channel c on d.ChannelId = c.ChannelId
            where c.ChannelId in ({0})            
        ";
        public static string _all = @"select 
			c.Name+'.'+d.Name+'.'+t.Name as [Path], 
			c.Name+'.'+d.Name+'.'+t.Address as [Unique],
			t.TagId,
			t.DeviceId,
			t.Name,
			t.Address,
			t.DataType,
			t.IsLogged,
			t.IsArchived
			from Tag t
			left join Device d on t.DeviceId = d.DeviceId
			left join Channel c on d.ChannelId = c.ChannelId";

        private static string _insert = @"
			INSERT INTO [dbo].[Tag]
					   ([DeviceId]
					   ,[Name]
					   ,[Address]
					   ,[DataType]
					   ,[IsLogged]
					   ,[IsArchived])
				 VALUES
					   ({0}, '{1}', '{2}', '{3}', {4}, {5})

			select scope_identity() 
			";

        private static string _update = @"
            UPDATE [dbo].[Tag]
               SET [Name] = '{1}'
                  ,[Address] = '{2}'
                  ,[DataType] = '{3}'
                  ,[IsLogged] = {4}
                  ,[IsArchived] = {5}
                  ,[RelatedTagId] = {6}
             WHERE TagId = {0}
            ";

        [ResultColumn] public string Path { get; set; }
        [ResultColumn] public string Unique { get; set; }
        [ResultColumn] public string Device { get; set; }
        [ResultColumn] public string Channel { get; set; }
        [ResultColumn] public string SetPoint { get; set; }

        public bool IsSetPoint { get { return SetPoint == "ps" || SetPoint == "ti"; } }

        public static Dictionary<string, int> map;
        public static List<int> logged;
        public static List<int> version = null;

		public static List<Tag> All()
        {
            List<Tag> results = null;
            using (tagDB tdb = new tagDB())
            {
                results = tdb.Fetch<Tag>(_all);
                map = results.ToDictionary(r => r.Path, r => r.TagId);
                logged = results.Where(r => r.IsLogged).Select(r => r.TagId).ToList();
            }

            return results.ToList();
        }

        public static List<Tag> tagsByLines(string lines)
        {
            List<Tag> results = new List<Tag>();
            using (tagDB tdb = new tagDB())
            {
                var tags = tdb.Fetch<Tag>(string.Format(_tagsByLines, lines));
                results = tags.ToList();
            }

            return results;
        }

        public static void StartVersion()
        {
            version = new List<int>(logged);
        }

        public static void EndVersion()
        {
            if (version == null)
                return;

            using (tagDB t = new tagDB())
            {
                var removed = logged.Except(version);
                if (removed.Any())
                {
                    t.Execute(string.Format(_unlog, string.Join(",", removed)));
                    Debug.WriteLine("removed " + removed.Count() + " tags from log");
                }
                else
                    Debug.WriteLine("no tags removed from log");

                t.Execute(_syncCurrent);
            }
        }

        private XFactory xml;
        public XElement node { get { return xml.node; } }

        public Tag()
        { }

        /// <summary>
        /// Create tag xml and reconcile database to match
        /// </summary>
        /// <param name="dev">Device containing tag</param>
        /// <param name="name">Name of tag without Channel.Device prefix</param>
        /// <param name="addr">Address string inside Device</param>
        /// <param name="type">Datatype of tag</param>
        public Tag(Device dev, string name, string addr, string type)
        {
            IsLogged = true;
            Name = name;
            Address = addr;
            DataType = type;
            DeviceId = dev.DeviceId;
            Path = dev.Path + "." + Name;

            xml = new XFactory(dev.node, "Tag", name);
            var ns = xml.node.Name.Namespace;
            node.Element(ns + "Name").Value = name;
            node.Element(ns + "Address").Value = addr;
            node.Element(ns + "DataType").Value = type;

            Tag tag;
            using (tagDB tdb = new tagDB())
            {
                tag = tdb.Fetch<Tag>("where Name = @0 and DeviceId = @1", Name, DeviceId).SingleOrDefault();
                TagId = (tag == null) ? 0 : tag.TagId;
            }
            var oldpath = (tag == null) ? "" : tag.Path;
            var id = reconcile(tag);
            if (version != null)
            {
                if (version.Exists(i => i == id))
                    Debug.WriteLine(Path + " with TagId " + TagId + " is being added over " + oldpath);
                else
                    version.Add(id);
            }
        }

        /// <summary>
        /// Add tag into TagGroup for database tag
        /// </summary>
        /// <param name="tg"></param>
        /// <param name="name"></param>
        /// <param name="addr"></param>
        /// <param name="type"></param>
        public Tag(TagList tg, string name, string addr, string type)
        {
            IsLogged = false;
            Name = name;
            Address = addr;
            DataType = type;

            xml = new XFactory(tg.node, "Tag", name);
            var ns = xml.node.Name.Namespace;
            node.Element(ns + "Name").Value = name;
            node.Element(ns + "Address").Value = addr;
            node.Element(ns + "DataType").Value = type;
        }

        /// <summary>
        /// Reconcile the database to match Tag
        /// </summary>
        /// <param name="t">Become this tag</param>
        private int reconcile(Tag t)
        {
            if (TagId == 0)
            {
                return Ins();
            }
            if (t.Address == Address && t.Name == Name && t.DataType == DataType && t.DeviceId == DeviceId && t.IsLogged == IsLogged)
                return TagId;
            return Upd();
        }

		public int Ins()
        {
            int newid = 0;

            if (map.TryGetValue(Path, out newid))       // different address maps to same tag path
                return newid;

            using (tagDB tdb = new tagDB())
            {
                newid = tdb.ExecuteScalar<int>(string.Format(_insert,
					DeviceId,
					Name,
					Address,
					DataType,
					IsLogged?1:0,
					IsArchived?1:0
				));
                TagId = newid;
                map.Add(Path, newid);
                if (IsLogged)
                    logged.Add(newid);
            }
            return newid;
        }

        public int Upd()
        {
            using (tagDB tdb = new tagDB())
            {
                int prior = 0;
                if (map.TryGetValue(Path, out prior) && prior != TagId)
                {
                    map.Remove(Path);
                    map.Add(Path, TagId);
                }

                tdb.Execute(string.Format(_update,
                    TagId,
                    Name,
                    Address,
                    DataType,
                    IsLogged?1:0,
                    IsArchived?1:0,
                    prior
                ));

                if (IsLogged)
                {
                    if (!logged.Contains(TagId)) logged.Add(TagId);
                }
                else
                    if (logged.Contains(TagId)) logged.Remove(TagId);
            }
            return TagId;
        }
	}
}