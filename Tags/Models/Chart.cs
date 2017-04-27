﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Web;
using ClosedXML.Excel;

namespace Tags.Models
{
    public static class MyExtensions {
        private static string _somepoints = "[new Date({0}),{1}],{2},[new Date({3}),{4}]";
        private static string _mid = "[new Date({0}),{1}]";
        private static string[] _end = new string[] { _mid + ",[null,null]", "[null,null]," + _mid };

        public static string LineSeries<T>(this IGrouping<int, T> data, Func<T, DateTime> moment, Func<T, string> member)
        {
            return string.Join(",", data.Select(q => "[new Date(" + moment(q).ToJSMSecs() + ")," + member(q) + "]").ToArray());
        }

        public static string TimeLine<T>(this List<T> data, Func<T, DateTime> moment, Func<T, int> member, int index)
        {
            // intervals are indicated using a -1 (end of value interval) or -2 (beginning of value interval) in the quality member
            return string.Join(",", data.Select(q => string.Format(member(q) < 0 ? _end[-member(q)-1] : _mid, moment(q).ToJSMSecs(), index)));
        }

        public static string Bounded<T>(this IGrouping<int, T> data, Func<T, DateTime> moment, Func<T, string> member, DateTime a, DateTime b)
        {
            return string.Format(_somepoints, a.ToJSMSecs(), member(data.First()), data.LineSeries(moment, member), b.ToJSMSecs(), member(data.Last()));
        }

        public static string ToStamp(this DateTime stamp)
        {
            return stamp.ToString("yyyy-MM-dd HH:mm:ss");
        }
    }

    public partial class Chart
    {
        private static string _delview = @"
            
        ";

        private static string _point = ",[new Date({0}),{1}]";
        private static string _nopoints = "[new Date({0}),{2}],[new Date({1}),{2}]";
        private static string _somepoints = "[new Date({0}),{1}],{2},[new Date({3}),{4}]";
        private static string _specs = "var hihi{0}=[{1}],hi{0}=[{2}],aim{0}=[{3}],lo{0}=[{4}],lolo{0}=[{5}];";

        private static string _fills = @"
               {{ id: 'lhihi{0}', data: hihi{0},                        lines: {{ show: true, steps: true }}, color: 'rgb(255,0,0)' }},
               {{ id: 'lhi{0}', data: hi{0},                            lines: {{ show: true, steps: true }}, color: 'rgb(0,255,0)' }},
               {{ id: 'laim{0}', data: aim{0}, points:{{show: true}},   lines: {{ show: true, steps: true }}, color: 'rgb(0,0,255)', markers: {{ show: true }} }},
               {{ id: 'llo{0}', data: lo{0},                            lines: {{ show: true, steps: true }}, color: 'rgb(0,255,0)' }},
               {{ id: 'llolo{0}', data: lolo{0},                        lines: {{ show: true, steps: true }}, color: 'rgb(255,0,0)' }}
        ";

        public static string _data = @"
            select TagId, Value, Stamp 
            from [Production]
            where TagId in ({0}) and Stamp >= '{1}' and Stamp <= '{2}'
                union all
            select TagId, Value, getdate() Stamp  
            from [Current]
            where TagId in ({0}) and rtrim(Value) != '' 
            order by TagId, Stamp";

        public static string _dataSince = @"
            select TagId, Value, Stamp
            from [Past]
            where TagId in ({0}) and Stamp >= '{1}'
                union all
            select TagId, Value, Stamp 
            from [All]
            where TagId in ({0}) and Stamp >= '{2}' 
                union all
            select TagId, Value, Stamp  
            from [Current]
            where TagId in ({0}) and rtrim(Value) != '' 
            order by TagId, Stamp";

        public static string _index = @"
            select t.TagId, t.Name, left(reverse(t.Name),2) as [SetPoint], c.Name as Channel, t.DataType 
                from [Tag] t 
                join [Device] d on d.DeviceId = t.DeviceId
                join [Channel] c on c.ChannelId = d.ChannelId
                where TagId in ({0})";
        public static string _current = @"select TagId, Value from [Current] where TagId in ({0})";

        public static string _correlate = @"
            select d.Tagid from [Tag] d 
                join [Tag] s on s.Name = d.Name
                join [Device] v on v.DeviceId = d.DeviceId
                join [Channel] c on c.ChannelId = v.ChannelId
                where c.ChannelId = {0} and s.Tag in ({1})";

        public Dictionary<int, string> index { get; set; }
        public Dictionary<int, bool> setp { get; set; }
        public ILookup<int, All> scalar { get; set;}
        public Dictionary<int, Dictionary<string, List<All>>> timelined { get; set; }
        public ILookup<int, Limit> limits { get; set; }
        public Dictionary<int, string> current { get; set; }

        public double zoomA { get; set; }
        public double zoomB { get; set; }
        public string axes { get; set; }
        public string charts { get; set; }
        public string series { get; set; }
        public bool isEmpty { get; set; }

        public string exportName;

        private string makeSeries(IGrouping<int, All> s, DateTime a, DateTime b)
        {
            if (!s.Any())
                return string.Format(_nopoints, a.ToJSMSecs(), b.ToJSMSecs(), current[s.Key]);
            
            return string.Format(_somepoints, a.ToJSMSecs(), s.First().Value,
                s.LineSeries(t => t.Stamp, q => q.Value),
                b.ToJSMSecs(), current[s.Key]);
        }

        private string makeTimeLines(List<All> s, DateTime a, DateTime b, int index, bool current)
        {
            if (!s.Any())
                return string.Format(_nopoints, a.ToJSMSecs(), b.ToJSMSecs(), index);

            return s.TimeLine(t => t.Stamp, q => q.Quality, index) + (current ? string.Format(_point, DateTime.Now.ToJSMSecs(), index):"");
        }

        private string makeBounds(IGrouping<int, Limit> g, int index, DateTime a, DateTime b)
        {
            if (!g.Any())
                return "";

            var hihi = g.Bounded(t => t.Stamp, v => v.HiHi.ToString(), a, b);
            var hi = g.Bounded(t => t.Stamp, v => v.Hi.ToString(), a, b);
            var aim = g.Bounded(t => t.Stamp, v => v.Aim.ToString(), a, b);
            var lo = g.Bounded(t => t.Stamp, v => v.Lo.ToString(), a, b);
            var lolo = g.Bounded(t => t.Stamp, v => v.LoLo.ToString(), a, b);

            return string.Format(_specs, index, hihi, hi, aim, lo, lolo);
        }

        public Chart()
        { }

        //public Chart(object tags, object points)
        //{
        //    index = tags as Dictionary<int, string>;
        //    scalar = points as ILookup<int, All>;

        //    if (index == null || scalar == null)
        //        return;
        //}

        public Chart(int channel, int[] request) : 
            this(string.Format(_correlate, channel, string.Join(",", request)))
        { }

        public Chart(int[] request, DateTime min) :
            this(string.Join(",", request), min, DateTime.Now)
        { }

        public Chart(int[] request, DateTime min, DateTime max) :
            this(string.Join(",", request), min, max)
        { }

        public Chart(int[] request) :
            this(string.Join(",", request), DateTime.MinValue, DateTime.Now)
        { }

        public Chart(string include) : this(include, DateTime.MinValue, DateTime.Now)
        { }

        public Chart(string include, DateTime min, DateTime max)
        {
            if (min == DateTime.MinValue)
                min = max.AddDays(-28);

            series = "";

            using (tagDB t = new tagDB()) {
                var tags = t.Fetch<Tag>(string.Format(_index, include));                            // list of tags being charted
                exportName = string.Join("_", tags.Select(v => v.Channel).Distinct().ToArray());    // set up name for spreadsheet download

                var multichannel = tags.Select(v => v.Channel).Distinct().Count() > 1;              // multiple channel queries?
                // multidevice needed: layflat tag is used in both wet and dry devices

                var stringValued = tags.Where(v => v.DataType.ToLower() == "string").Select(d => d.TagId);
                index = tags.ToDictionary(i => i.TagId, i => (multichannel ? (i.Channel + ".") : "") + i.Name);
                setp = tags.ToDictionary(i => i.TagId, i => i.IsSetPoint);

                var samples = t.Fetch<All>(string.Format(_data, include, min.ToStamp(), max.ToStamp()));
                if (!samples.Any())
                {
                    var taglist = string.Join(", ", index.Values.ToArray());
                    series = "$('#ChartContainer').html('<h2><br />No data in last 28 days</h2><h4><i>for</i> " + taglist + "</h4>');";                      // explicit notice
                    isEmpty = true;
                    return;
                }
                isEmpty = false;

                limits = Limit.Specs(t, include, min, DateTime.Now).ToLookup(l => l.TagId);             // get limits for those tags that have them attached
                var splitByType = samples.Where(d => stringValued.Contains(d.TagId));                   // some values are labels but most are float data points

                timelined = splitByType.ToLookup(d => d.TagId).ToDictionary(d => d.Key, d => ThreadsByLabel(d));        // labels on a timeline to be y2axis
                scalar = samples.Except(splitByType).ToLookup(n => n.TagId);                                                                   // tags with only numeric values

                current = t.Fetch<Current>(string.Format(_current, include)).ToDictionary(c => c.TagId, c => c.Value);  // get values from right now
            }

            // for those tags with limits, filter out values so that lolo-10 < value < hihi+10
            foreach (var lm in limits)
            {
                var limited = scalar[lm.Key].Select(u =>
                {
                    var prior = lm.LastOrDefault(m => m.Stamp <= u.Stamp);
                    if (prior == null)
                        return u;
                    u.Value = prior.Clip(u.Value);
                    return u;
                }).ToList();
            }

            // create client side data payload:  numeric timeline, specification lines, occupation lines
            var numeric = scalar.Select((s, i) => "d" + i + "=[" + makeSeries(s, min, max) + "]").ToList();             // create javascript for numeric data
            var specs = limits.Select((m, j) => makeBounds(m, j, min, max)).ToList();                                   // same for the limit lines

            // declare and set up javascript variables with data arrays
            series = (numeric.Any() ? "var " + string.Join(",\n", numeric.ToArray()) + ";\n" : "") + (specs.Any() ? (string.Join("\n", specs.ToArray()) + ";\n") : "");
            charts = "";
            axes = @"yaxis: { autoscale: true, autoscaleMargin: .1 },";

            var q = 1;
            foreach (var tagtlines in timelined)
            {
                var line = index[tagtlines.Key].Split('.');
                var prefix = "";
                if (line.Length > 1)
                    prefix = line[0] + ".";
                var tlines = tagtlines.Value;
                if (!tlines.Any())
                    continue;
                if (q == 1)
                {
                    series += "var ";
                    axes += "y2axis: { autoscale: true, autoscaleMargin: .05, ticks: [";                 // todo ** may want to remove autoscale and set the top margin to (last tag vertical)*5
                }
                var curval = current[tagtlines.Key];
                series += string.Join(",\n", tlines.Keys.Select((label, k) => "v" + (k + q) + "=[" + makeTimeLines(tlines[label], min, max, (k + q), curval == label) + "]").ToArray()) + ",\n";
                charts += string.Join(",\n", tlines.Keys.Select((label, r) => "{yaxis:2, data:v" + (r + q) + ",points:{show:true},lines:{show:true, lineWidth:5},label:'" + prefix + label + "'}")) + ",";
                axes += string.Join(",", tlines.Keys.Select((label, r) => "[" + (r + q) + ",'" + label + "']").ToArray()) + ",";

                q += tlines.Count();          // assign each tag's value a timeline of it's own
                q++;                          // put a spacer line between different tags 
            }

            if (timelined.Any() && q > 1)
            {
                series += " dummy = 0;\n";
                charts = charts.Substring(0, charts.Length - 1) + "\n";
                axes = axes.Substring(0, axes.Length - 1) + "] },";
            }

            charts += ((specs.Any() && charts.Length > 0) ? "," : "") + string.Join(",\n", specs.Select((x, u) => string.Format(_fills, u)).ToArray());
            charts += ((scalar.Any() && charts.Length > 0) ? "," : "") + string.Join(",\n", scalar.Select((r, p) => "{data:d" + p + ",points:{show:false},lines:{show:true,steps:" + (setp[r.First().TagId]?"true":"false")+"},label:'" + index[r.First().TagId] + "'}").ToArray());
        }

        private Dictionary<string, List<All>> ThreadsByLabel(IGrouping <int, All> d)
        {
            var indelta = false;
            var delta = d.Zip(d.Skip(1), (a, b) => {
                if (a.Value != b.Value)
                {
                    a.Quality = -1;
                    indelta = true;
                } else if (indelta)
                {
                    a.Quality = -2;
                    indelta = false;
                }
                return a;
            });                  // mark end of runs of the same value to prepare for ending a line segment
           
            return delta.Select(e => e.Value).Distinct().ToDictionary(h => h, f => d.Where(g => g.Value == f).ToList());    // split tag into lists by value...  ie., value string is used as index to list of times
        }

        public int Export(Stream path)
        {
            int tot = 0;
            var wb = new XLWorkbook();
            foreach(var e in index)
            {
                var ws = wb.Worksheets.Add(e.Value);
                var all = scalar[e.Key];

                if (zoomA != 0)
                    all = all.Where(d => d.Stamp >= zoomA.FromJSMSecs().ToLocalTime());

                if (zoomB != 0)
                    all = all.Where(d => d.Stamp <= zoomB.FromJSMSecs().ToLocalTime());

                double dbl;
                bool isNumber = double.TryParse(all.First().Value, out dbl);
                var t = all.Select((d, i) => {
                    ws.Row(i + 1).Cell(1).SetValue<DateTime>(d.Stamp);
                    if (isNumber)
                        ws.Row(i + 1).Cell(2).SetValue<double>(double.Parse(d.Value));
                    else
                        ws.Row(i + 1).Cell(2).SetValue<string>(d.Value);
                    return i;
                }).ToList();
                tot += t.Last()+1;
            
                if (limits.Contains(e.Key))
                {
                    var wl = wb.Worksheets.Add(e.Value + "_limits");
                    var lim = limits[e.Key];
                    int start = 0;
                    long end = long.MaxValue;

                    if (zoomA != 0)
                        start = lim.Last(d => d.Stamp <= zoomA.FromJSMSecs().ToLocalTime()).LimitId;

                    if (zoomB != 0)
                        end = lim.Last(d => d.Stamp <= zoomB.FromJSMSecs().ToLocalTime()).LimitId;

                    lim = lim.Where(d => d.LimitId >= start && d.LimitId <= end);

                    var s = lim.Select((d, i) => {
                        wl.Row(i + 1).Cell(1).SetValue<DateTime>(d.Stamp);
                        wl.Row(i + 1).Cell(2).SetValue<double>(d.LoLo);
                        wl.Row(i + 1).Cell(3).SetValue<double>(d.Lo);
                        wl.Row(i + 1).Cell(4).SetValue<double>(d.Aim);
                        wl.Row(i + 1).Cell(5).SetValue<double>(d.Hi);
                        wl.Row(i + 1).Cell(6).SetValue<double>(d.HiHi);
                        return i;
                    }).ToList();
                    tot += s.Last() + 1;
                }
            }
            wb.SaveAs(path);
            return tot;
        }

    }

    public static class DateTimeJavaScript
    {
        private static readonly long DatetimeMinTimeTicks =
           (new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).Ticks;

        public static long ToJSMSecs(this DateTime dt)
        {
            return (long)((dt.ToUniversalTime().Ticks - DatetimeMinTimeTicks) / 10000);
        }

        public static DateTime FromJSMSecs(this double tm)
        {
            return new DateTime(((long) tm*10000) + DatetimeMinTimeTicks).ToLocalTime();
        }
    }
}