using System;
using System.Collections.Generic;
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

        public static string Series<T>(this IGrouping<int, T> data, Func<T, DateTime> moment, Func<T, string> member)
        {
            return string.Join(",", data.Select(q => "[new Date(" + moment(q).ToJSMSecs() + ")," + member(q) + "]").ToArray());
        }

        public static string TimeLine<T>(this List<T> data, Func<T, DateTime> moment, Func<T, int> member, int index)
        {
            return string.Join(",", data.Select(q => string.Format(member(q) < 0 ? _end[-member(q)-1] : _mid, moment(q).ToJSMSecs(), index)));
        }

        public static string Bounded<T>(this IGrouping<int, T> data, Func<T, DateTime> moment, Func<T, string> member, DateTime a, DateTime b)
        {
            return string.Format(_somepoints, a.ToJSMSecs(), member(data.First()), data.Series(moment, member), b.ToJSMSecs(), member(data.Last()));
        }
    }

    public partial class Chart
    {
        private static string _delview = @"
            
        ";
        private static string _updview = @"
            Merge [user] u
            using (select [Identity] = '{0}') s 
            ON s.[Identity] = u.[Identity]
            WHEN NOT matched THEN 
            INSERT ([Identity]) VALUES (s.[Identity]);

            merge 
        ";

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
            from [All]
            where TagId in ({0}) 
                union all
            select TagId, Value, Stamp  
            from [Current]
            where TagId in ({0}) and rtrim(Value) != '' order by TagId, Stamp";

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
                s.Series(t => t.Stamp, q => q.Value),
                b.ToJSMSecs(), current[s.Key]);
        }

        private string makeTimeLines(List<All> s, DateTime a, DateTime b, int index)
        {
            if (!s.Any())
                return string.Format(_nopoints, a.ToJSMSecs(), b.ToJSMSecs(), index);

            return string.Format(_somepoints, a.ToJSMSecs(), index,
                s.TimeLine(t => t.Stamp, q => q.Quality, index),
                b.ToJSMSecs(), index);
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

        public Chart(object tags, object points)
        {
            index = tags as Dictionary<int, string>;
            scalar = points as ILookup<int, All>;

            if (index == null || scalar == null)
                return;
        }

        public Chart(int channel, int[] request) : 
            this(string.Format(_correlate, channel, string.Join(",", request)))
        { }

        public Chart(int[] request) :
            this(string.Join(",", request))
        { }

        public Chart(string include)
        {
            DateTime max = DateTime.Now;
            DateTime min = max.AddDays(-28);

            series = "";

            using (tagDB t = new tagDB()) {
                var tags = t.Fetch<Tag>(string.Format(_index, include));                            // list of tags being charted
                exportName = string.Join("_", tags.Select(v => v.Channel).Distinct().ToArray());    // set up name for spreadsheet download

                var multichannel = tags.Select(v => v.Channel).Distinct().Count() > 1;                // multiple channel queries?
                // multidevice needed: layflat tag is used in both wet and dry devices

                var stringValued = tags.Where(v => v.DataType.ToLower() == "string").Select(d => d.TagId);
                index = tags.ToDictionary(i => i.TagId, i => (multichannel ? (i.Channel + ".") : "") + i.Name);

                var samples = t.Fetch<All>(string.Format(_data, include));
                if (!samples.Any())
                {
                    var taglist = string.Join(", ", index.Values.ToArray());
                    series = "$('#ChartContainer').html('<h2><br />No data in last 28 days</h2><h4><i>for</i> " + taglist + "</h4>');";                      // explicit notice
                    isEmpty = true;
                    return;
                }
                isEmpty = false;
                min = samples.Min(d => d.Stamp);
                max = samples.Max(d => d.Stamp);

                limits = Limit.Specs(t, include, min, DateTime.Now).ToLookup(l => l.TagId);             // get limits for those tags that have them attached
                var splitByType = samples.Where(d => stringValued.Contains(d.TagId));                   // some values are labels but most are float data points

                timelined = splitByType.ToLookup(d => d.TagId).ToDictionary(d => d.Key, d => ThreadsByLabel(d));        // labels on a timeline to be y2axis
                scalar = samples.Except(splitByType).ToLookup(d => d.TagId);                                            // tags with only numeric values

                current = t.Fetch<Current>(string.Format(_current, include)).ToDictionary(c => c.TagId, c => c.Value);  // get values from right now
            }

            // create client side data payload:  numeric timeline, specification lines, occupation lines
            var numeric = scalar.Select((s, i) => "d" + i + "=[" + makeSeries(s, min, max) + "]").ToList();             // create javascript for numeric data
            var specs = limits.Select((m, j) => makeBounds(m, j, min, max)).ToList();                                   // same for the limit lines

            // declare and set up javascript variables with data arrays
            series = (numeric.Any() ? "var " + string.Join(",\n", numeric.ToArray()) + ";\n" : "") + (specs.Any() ? (string.Join("\n", specs.ToArray()) + ";\n") : "") + (timelined.Any() ? "var " : "");
            charts = "";
            axes = @"yaxis: { autoscale: true, autoscaleMargin: .1 },";
            axes += timelined.Any() ? "y2axis: { autoscale: true, autoscaleMargin: .05, ticks: [" : "";

            var q = 1;
            foreach (var tagtlines in timelined)
            {
                var line = index[tagtlines.Key].Split('.');
                var prefix = "";
                if (line.Length > 1)
                    prefix = line[0];
                var tlines = tagtlines.Value;
                series += string.Join(",\n", tlines.Keys.Select((label, k) => "v" + (k + q) + "=[" + makeTimeLines(tlines[label], min, max, (k + q)) + "]").ToArray()) + ",\n";
                charts += string.Join(",\n", tlines.Keys.Select((label, r) => "{yaxis:2, data:v" + (r + q) + ",points:{show:true},lines:{show:true, lineWidth:5},label:'" + prefix + "." + label + "'}")) + ",";
                axes += string.Join(",", tlines.Keys.Select((label, r) => "[" + (r + q) + ",'" + label + "']").ToArray()) + ",";

                q += tlines.Count();          // assign each tag's value a timeline of it's own
                q++;                                // put a spacer line between different tags 
            }

            if (timelined.Any())
            {
                series += " dummy = 0;\n";
                charts = charts.Substring(0, charts.Length - 1) + "\n";
                axes = axes.Substring(0, axes.Length - 1) + "] },";
            }

            charts += (specs.Any() ? "," : "") + string.Join(",\n", specs.Select((x, u) => string.Format(_fills, u)).ToArray());
            charts += (scalar.Any() ? "," : "") + string.Join(",\n", scalar.Select((r, p) => "{data:d" + p + ",points:{show:false},lines:{show:true},label:'" + index[r.First().TagId] + "'}").ToArray());
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
            }
            wb.SaveAs(path);
            return tot;
        }

        public static void SaveView(string user, string name, int[] tagids)
        {
            if (string.IsNullOrWhiteSpace(name))
                return;

            using (tagDB t = new tagDB())
            {
               
            }
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