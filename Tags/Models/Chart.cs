using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using ClosedXML.Excel;

namespace Tags.Models
{
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

        public static string _data = @"select TagId, Value, Stamp from [All] where TagId in ({0}) 
                                                    union all
                                                    select TagId, Value, Stamp from [Current]
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
        public ILookup<int, All> data { get; set;}
        public Dictionary<int, string> current { get; set; }

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
                string.Join(",", s.Select(q => "[new Date(" + q.Stamp.ToJSMSecs() + ")," + q.Value + "]").ToArray()),
                b.ToJSMSecs(), current[s.Key]);
        }

        public Chart()
        { }

        public Chart(object tags, object points)
        {
            index = tags as Dictionary<int, string>;
            data = points as ILookup<int, All>;

            if (index == null || data == null)
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

            using (tagDB t = new tagDB()) {
                var tags = t.Fetch<Tag>(string.Format(_index, include));                            // list of tags being charted
                exportName = string.Join("_", tags.Select(v => v.Channel).Distinct().ToArray());    // set up name for spreadsheet download

                var multichannel = tags.Select(v=>v.Channel).Distinct().Count() > 1;                // multiple channel queries?
                // multidevice needed: layflat tag is used in both wet and dry devices

                index = tags.ToDictionary(i => i.TagId, i => (multichannel?(i.Channel + "."):"") + i.Name);

                var samples = t.Fetch<All>(string.Format(_data, include));

                if (!samples.Any())
                {
                    var taglist = string.Join(", ", index.Values.ToArray()); 
                    series = "$('#ChartContainer').html('<h2><br />No data in last 28 days</h2><h4><i>for</i> " + taglist+"</h4>');";                      // explicit notice
                    isEmpty = true;
                    return;
                }
                isEmpty = false;
                min = samples.Min(d => d.Stamp);
                max = samples.Max(d => d.Stamp);

                var limits = Limit.Specs(t, include, min, DateTime.Now);

                data = samples.ToLookup(d => d.TagId);
                current = t.Fetch<Current>(string.Format(_current, include)).ToDictionary(c => c.TagId, c => c.Value);
            }
            var sequence = data.Select((s, i) => "d" + i + "=[" + makeSeries(s, min, max) + "]").ToList();
            series = "var " + string.Join(",\n", sequence.ToArray()) + ";";
            charts = string.Join(",\n", data.Select((r, p) => "{data:d" + p + ",points:{show:false},lines:{show:true},label:'" + index[r.First().TagId] + "'}").ToArray());
            axes = @"yaxis:  { autoscale: true, autoscaleMargin: .1 },";
        }

        public int Export(Stream path)
        {
            int tot = 0;
            var wb = new XLWorkbook();
            foreach(var e in index)
            {
                var ws = wb.Worksheets.Add(e.Value);
                var all = data[e.Key];
                double dbl;
                bool number = double.TryParse(all.First().Value, out dbl);
                var t = all.Select((d, i) => {
                    ws.Row(i + 1).Cell(1).SetValue<DateTime>(d.Stamp);
                    if (number)
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
    }
}