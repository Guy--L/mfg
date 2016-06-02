using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;
using System.Xml.Linq;
using ClosedXML.Excel;

namespace Tags.Models
{
    public class LinkChannel
    {
        public string channel { get; set; }
        public int id { get; set; }
    }

    public class TagMap
    {
        static readonly Dictionary<string, string> _map = new Dictionary<string, string>() {
            {"bool", "Boolean"},
            {"Discrete", "Boolean"},
            {"bcd", "BCD"},
            {"BCD_int_16", "BCD"},
            {"BCD_int_32", "BCD"},
            {"Floating_PT_32", "Float"},
            {"unsigned int_16", "Word"},
            {"real", "Float"},
            {"default", "Word"},
        };
        static TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;

        static public string projectPath;
        
        string _inputfile;

        static object typenorm(object typein)
        {
            string typeout;
            return (object)(_map.TryGetValue(typein.ToString(), out typeout) ? typeout : "");
        }

        XDocument _newProject;
        
        public void Import(string inputFile)
        {
            _inputfile = inputFile;
            _newProject = XFactory.projectXML;

            Tag.StartVersion();

            var wb = new XLWorkbook(_inputfile);
            var ws = wb.Worksheet(1);
            DoXML(ws, 4);

            //var ns = XFactory.nmspaces[typeof(LogGroup)];

            ws = wb.Worksheet(2);
            DoXML(ws, 5);

            ws = wb.Worksheet(3);
            DoXML(ws, 4);

            ws = wb.Worksheet("12Layflat");
            DoXML(ws, 3);

            ws = wb.Worksheet("links");
            ControlTags(ws);

            Tag.EndVersion();

            _newProject.Save(projectPath);
        }

        private bool replaceColumn = false;
        private string TagName(IXLRangeRow r)
        {
            if (!replaceColumn)
                return r.Cell(1).Value.ToString();

            var newName = r.Cell(2).Value.ToString();
            if (string.IsNullOrWhiteSpace(newName))
                return r.Cell(1).Value.ToString();

            return newName;
        }

        void DoXML(IXLWorksheet ws, int column)
        {
            var deviceModel = ws.Row(1).Cell(column-1).GetValue<string>();
            var first = ws.Row(3).Cell(1).Address;
            var last = ws.LastCellUsed().Address;

            var types = ws.Range(first, last).Rows().ToDictionary(r => r.RowNumber(), r => typenorm(r.Cell(column-1).Value));
            replaceColumn = ws.Row(2).Cell(2).Value.ToString().ToLower().Contains("name");

            var skip = 0;
            while (column <= last.ColumnNumber)
            {
                string[] plc = ws.Row(1).Cell(column).Value.ToString().Split(',');
                string line = plc[0].Trim().Substring(0, 2).ToUpper();
                string end = textInfo.ToTitleCase(plc[0].Trim().Substring(2));

                // Write Device Node adding channel (C3) and device (Dry) as necessary 

                var channel = new Channel(_newProject.Root, line);
                var device = new Device(channel, end, plc[1].Trim(), deviceModel);

                IEnumerable<IXLRangeRow> rows;
                List<Tag> tags;

                if (column == 5 && ws.Row(2).Cell(7).Value.ToString().ToLower().Contains("updated"))
                {
                    rows = ws.Range(first, last).Rows().Where(s => 
                        !string.IsNullOrWhiteSpace(s.Cell(column).Value.ToString() + s.Cell(column + 2).Value.ToString()) &&
                        !string.IsNullOrWhiteSpace(TagName(s))
                    );

                    // Write Tags

                    tags = rows.Select(t => new Tag(device, TagName(t),
                        (string.IsNullOrWhiteSpace(t.Cell(column + 2).Value.ToString()) ? t.Cell(column).Value.ToString() : t.Cell(column + 2).Value.ToString()),
                        types[t.RowNumber()].ToString())).ToList();
                    skip = 1;
                }
                else
                {
                    rows = ws.Range(first, last).Rows().Where(s => 
                        !string.IsNullOrWhiteSpace(s.Cell(column).Value.ToString()) &&
                        !string.IsNullOrWhiteSpace(TagName(s))
                    );
                    tags = rows.Select(t => new Tag(device, TagName(t), t.Cell(column).Value.ToString(), types[t.RowNumber()].ToString())).ToList();
                    skip = 0;
                }

                // Set up seven day log
                
                var all = new LogGroup(_newProject.Root, channel.Name, "All");
                tags.Select(r => new LogItem(all.node, r.Path, r.TagId)).ToList();
                var ns = all.node.Name.Namespace;

                _newProject.Descendants(ns + "NumericID")
                .Where(w => Convert.ToInt32(w.Value) == 0)
                .FirstOrDefault(
                    q=>{q.Value = Tag.map[q.Parent.Element(ns+"Name").Value].ToString(); return false;}
                );

                var alltrigger = new Trigger(all.node, 1, false, true);

                // Use remark field first character to set up log groups

                //var synch = rows.Where(u => u.Cell(column + 1).ToString() == "1").Select(v => TagName(v));
                //if (synch.Any())
                //{
                //    var min1 = new LogGroup(_newProject.Root, "PerMinute", "Recent");
                //    synch.Select(w => new LogItem(min1.node, w, Tag.map[w])).ToList();
                //    var syntrigger = new Trigger(min1.node, 1, true, false);
                //}

                //var delta = rows.Where(x => x.Cell(column + 1).ToString() == "0").Select(y => TagName(y));
                //if (delta.Any())
                //{
                //    var del = new LogGroup(_newProject.Root, "PerChange", "Recent");
                //    delta.Select(d => new LogItem(del.node, d, Tag.map[d]));
                //    var deltrigger = new Trigger(del.node, 0, false, true);
                //}
                column += skip;
                column += 2;
            }
        }

        private static string _auto = @"
            select distinct c.Name from tag t
            join device d on d.DeviceId = t.DeviceId
            join channel c on c.ChannelId = d.ChannelId
            where t.Name = 'Line_Auto'
            ";

        private static string _name = @"HMI__{0:D2}__{1}";
        private static string _addr = @"HMI [{0:D2}].{1}";
        private static string[] _states = { "Error", "RequestPending", "RequestComplete" };
        private static string[] _types = { "Boolean", "Long", "Boolean" };

        private static string[] _links = { "Error", "DryHeartBeat", "WetHeartBeat", "Lock", "Unlock" };
        private static string[][] _linkio = {
            new string [] {"{1}.Dry._System._Error", "Locks.HMI.Error.HMI__{0:D2}__Error", "200"},
            new string [] {"{1}.Dry.PLC_timing", "{1}.Dry.MES_timing", "1000"},
            new string [] {"{1}.Wet.PLC_timing", "{1}.Wet.MES_timing", "1000"},
            new string [] {"{1}.Dry.ScreenLatch", "Locks.HMI.RequestComplete.HMI__{0:D2}__RequestComplete", "100"},
            new string [] {"Locks.HMI.RequestPending.HMI__{0:D2}__RequestPending", "{1}.Dry.ScreenChange", "100"}
        };

        private static List<string[]> _dry2wet = new List<string[]> {
            new string[] {"1", "Dry2WetAuto", ".Dry.Line_Auto", ".Wet.Line_Auto"},
            new string[] {"2", "Dry2WetSpeed", ".Dry.Line_fpm_sp", ".Wet.Line_fpm_sp"},
            new string[] {"3", "Dry2WetStopped", ".Dry.Line_stopped", ".Wet.Dryer_stopped"}
        };

        private static string _linkinsert = @"
            insert #link values ({0},'{1}','{2}','{3}','{4}','{5}');
        ";

        private static string _linkvalid = @";
            create table #link (
                id integer,
                name varchar(64),
                srcdev varchar(32),
                srctag varchar(32),
                dstdev varchar(32),
                dsttag varchar(32)
            );

            {0}

            select distinct c.name as channel, l.id from Channel c
            join device d1 on d1.ChannelId = c.ChannelId
            join device d2 on d2.ChannelId = c.ChannelId
            join tag t1 on t1.DeviceId = d1.DeviceId
            join tag t2 on t2.DeviceId = d2.DeviceId
            join #link l on l.srcdev = d1.Name and l.srctag = t1.Name and l.dstdev = d2.Name and l.dsttag = t2.Name;

            drop table #link;
        ";

        /// <summary>
        /// Set up first database tags generated from HMI table.
        /// Then set up link tags between other tags and database tags
        /// Some link tags are between device tags only
        /// </summary>
        void ControlTags(IXLWorksheet ws)
        {
            List<string[]> links = null; 
            List<HMI> controls = null;

            if (ws != null)
            {
                var first = ws.Row(2).Cell(1).Address;
                var last = ws.LastCellUsed().Address;

                var rows = ws.Range(first, last).Rows();
                links = rows.Select((r, i) =>
                    new string[] { (i+1).ToString(), r.Cell(1).GetString(), r.Cell(2).GetString(), r.Cell(3).GetString() }
                ).ToList();
            }
            else
            {
                links = _dry2wet;
            }

            var check = string.Format(_linkvalid, string.Join("", 
                links.Select(k => string.Format(_linkinsert, k[0], k[1], k[2].Split('.')[1], k[2].Split('.')[2], k[3].Split('.')[1], k[3].Split('.')[2]))));

            List<LinkChannel> linklines;
            using (tagDB t = new tagDB())
            {
                t.Execute(HMI._sync);                       // make new HMI records for any new channels
                controls = t.Fetch<HMI>(HMI._controls);
                linklines = t.Fetch<LinkChannel>(check);
            }
            var channel = new DataChannel(_newProject.Root, "Locks");
            var groups = _states.Select(g => new TagList(channel.node, g));

            for (int i = 0; i < controls.Count(); i++)
            {
                var cnm = controls[i].ChannelName;

                var q = groups.Select((d, j) =>
                    new Tag(d,
                        string.Format(_name, i + 1, _states[j]),
                        string.Format(_addr, i + 1, _states[j]),
                        _types[j])).ToList();


                var p = _links.Select((e, k) =>
                    new LinkTag(_newProject.Root, cnm + e,
                        string.Format(_linkio[k][0], i + 1, cnm),
                        string.Format(_linkio[k][1], i + 1, cnm),
                        _linkio[k][2])).ToList();
            }

            var linktags = links.ToDictionary(k => int.Parse(k[0]), v => new { name = v[1], src = v[2], dst = v[3] });
            var s = linklines.Select(n => 
            {
                var set = linktags[n.id];
                new LinkTag(_newProject.Root, n.channel + set.name, n.channel + set.src, n.channel + set.dst, "1000");
                return 1;
            }).ToList();
        }
    }
}
