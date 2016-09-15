using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Tags.Models
{
    public class ChartView
    {
        public string List { get; set; }
        public string Chart { get; set; }
        public HtmlString Title { get { return new HtmlString(List.Replace(",", "<br />")); } }
    }

    public class Series2
    {
        public string Name { get; set; }
        public Tag Tag { get; set; }
        public List<List<Val>> Specs { get; set; }
        public List<Val> Data { get; set; }

        public Series2(string nm, Tag t)
        {
            Name = nm;
            Tag = t;
        }
    }

    public class Chart2
    {
        public string Name { get; set; }
        public List<Series2> Series { get; set; }
        public ILookup<int, Run> Runs { get; set; }
        public string exportName;

        public static string _index = @"
            select t.TagId, t.Name, left(reverse(t.Name),2) as [SetPoint], c.Name as Channel, d.Name as Device, t.DataType 
                from [Tag] t 
                join [Device] d on d.DeviceId = t.DeviceId
                join [Channel] c on c.ChannelId = d.ChannelId
                where TagId in ({0})";

        public static string _data = @"
            select TagId, Value, Stamp, convert(float,value,0) as dvalue, dbo.epoch(Stamp) as epoch 
                from [Production]
                where TagId in ({0}) and Stamp >= '{1}' and Stamp <= '{2}'
                order by TagId, Stamp";

        public Chart2(int[] request, DateTime min, DateTime max) :
            this(string.Join(",", request), min, max)
        { }

        /// <summary>
        /// Create chart for tags in cs-list between min and max
        /// Chart
        ///   Name
        ///   exportName
        ///   Runs
        ///   Series
        ///     Name  
        ///     Tag
        ///     Specs
        ///     Data
        /// </summary>
        /// <param name="include"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        public Chart2(string include, DateTime min, DateTime max)
        {
            using (tagDB t = new tagDB())
            {
                var tags = t.Fetch<Tag>(string.Format(_index, include));                   // list of tags being charted
                var chns = tags.Select(v => v.Channel).Distinct().ToArray();               // list of channels 
                exportName = string.Join("_", chns);                                       // set up name for spreadsheet download

                Runs = Run.RunsByLine(chns, min, max);                                     // runs by channel

                var stringTags = tags.Where(v => v.DataType.ToLower() == "string");
                var ninclude = string.Join(",", tags.Except(stringTags).Select(v => v.TagId).ToArray());

                var multichannel = tags.Select(v => v.Channel).Distinct().Count() > 1;     // multiple channel queries?
                var plots = tags.ToDictionary(i => i.TagId, v => new Series2((multichannel ? (v.Channel + ".") : "") + v.Name, v));

                // multidevice needed: layflat tag is used in both wet and dry devices
                // index = index.GroupBy(x => x.Value).Where(x => x.Count() > 1).Select(x => x.Value.Replace(".","."+t.Device+"."));    except where is t?
                
                var samples = t.Fetch<Value>(string.Format(_data, ninclude, min.ToStamp(), max.ToStamp())).ToLookup(n => n.TagId);
                if (!samples.Any())
                {
                    return;
                }

                var limits = Limit.Specs(t, ninclude, min, max).ToLookup(l => l.TagId);        // get limits for those tags that have them attached

                Series = tags.Select(p =>
                {
                    var series = plots[p.TagId];
                    var spectrum = limits[p.TagId];
                    var lolo = spectrum.Select(m => new Val(m.epoch, m.LoLo)).ToList();
                    var lo = spectrum.Select(m => new Val(m.epoch, m.Lo)).ToList();
                    var aim = spectrum.Select(m => new Val(m.epoch, m.Aim)).ToList();
                    var hi = spectrum.Select(m => new Val(m.epoch, m.Hi)).ToList();
                    var hihi = spectrum.Select(m => new Val(m.epoch, m.HiHi)).ToList();

                    series.Specs = new List<List<Val>>() { lolo, lo, aim, hi, hihi };
                    series.Data = samples[p.TagId].Select(s => new Val(s)).ToList();
                    return series;
                }).ToList();
            }
        }
    }
}