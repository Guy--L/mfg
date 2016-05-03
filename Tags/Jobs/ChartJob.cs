using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Web;
using Quartz;
using Tags.Models;
using System.Web.Configuration;
using System.Windows.Forms.DataVisualization.Charting;
using cx = System.Windows.Forms.DataVisualization.Charting;
using System.Drawing;

namespace Tags.Jobs
{
    public class ChartJob : IJob
    {
        private static string _alldata = @"
            select * from (
            select TagId, '{1}' Stamp, Value from (
             select n.tagid, n.stamp, n.value, 
              ROW_NUMBER() OVER (PARTITION BY tagid ORDER BY stamp DESC) rn
             from [All] n
             join dbo.SplitInts('{0}',',') s on s.Item = n.TagId
             where stamp < '{1}'
            ) a where rn = 1
            union all
            select
                a.TagId,
                a.Stamp,
                a.Value
            from [All] a
            join dbo.SplitInts('{0}',',') s on s.Item = a.TagId
            where a.Stamp >= '{1}' and a.Stamp <= '{2}' 
            ) q
            order by q.TagId, q.Stamp           
        ";

        private List<Graph> deck;
        private Dictionary<int, List<DateTime>> time;
        private Dictionary<int, List<object>> value;
        private ILookup<int, Limit> limits;
        private DateTime start;
        private DateTime end;

        public void Execute(IJobExecutionContext context)
        {

            int id = 0;
            if (!int.TryParse(context.JobDetail.Key.Name, out id))
            {
                return;
            }

            var trigger = context.Trigger as ICronTrigger;

            var last = trigger.GetPreviousFireTimeUtc();
            if (last == null)
            {
                var next = trigger.GetNextFireTimeUtc();
                
            }
            var cron = trigger.CronExpressionString;
            CronExpression cr = new CronExpression(cron);
        }

        public void Render()
        {
            var path = WebConfigurationManager.AppSettings["graphjoboutdir"];
            var template = WebConfigurationManager.AppSettings["graphjobtemplate"];
            var root = Path.Combine(path, DateTime.Now.ToString(template));

            time = new Dictionary<int, List<DateTime>>();   // cache data between graphs
            value = new Dictionary<int, List<object>>();

            deck = Graph.plotsByReview(1);          // get all graphs in this review

            deck.Select(g =>
            {
                end = DateTime.Now.AddDays(-21);
                start = end.AddDays(-7);

                var channels = g.Canon();           // get all tags for all lines
                var tags = Read(g);                 // get data for period and list of tags

                channels.Select(line =>
                {
                    var linedir = Path.Combine(root, line);
                    Directory.CreateDirectory(linedir);         // create directory for charts

                    var slice = tags.Where(t => t.Channel == line);

                    Render(g, linedir, slice);                    // create chart for this line and  
                    return 1;
                }).ToList();
                return 1;
            }).ToList();
        }

        public List<Tag> Read(Graph g)
        {
            var taga = g.Plots.SelectMany(p => p.tags);
            var tagtype = taga.ToDictionary(k => k.TagId, v => Tag.types[v.DataType]);
            var tagsInGraph = g.Plots.SelectMany(p => p.tags);

            var taglist = string.Join(",", tagsInGraph.Select(i => i.TagId).Except(time.Keys).ToArray());

            var st = start.ToStamp();
            var et = end.ToStamp();

            using (tagDB t = new tagDB())
            {
                var query = string.Format(_alldata, taglist, start.ToStamp(), end.ToStamp());
                var all = t.Fetch<All>(query).ToLookup(k => k.TagId, v => v);
                limits = Limit.Specs(t, taglist, start, end).ToLookup(l => l.TagId);             // get limits for those tags that have them attached

                all.Select(a =>
                {
                    time[a.Key] = a.Select(u => u.Stamp).ToList();
                    value[a.Key] = a.Select(u => Convert.ChangeType(u.Value, tagtype[u.TagId])).ToList();
                    return 1;
                }).ToList();
            }
            return tagsInGraph.ToList();
        }

        private static string[] backgrounds = new string[]
        {
            "#F2D7D5",
            "#EBDEF0",
            "#D4E6F1",
            "#D1F2EB",
            "#D4EFDF",
            "#FCF3CF",
            "#FAE5D3"
        };

        public void Render(Graph g, string path, IEnumerable<Tag> slice)
        {
            // get data for interval and channel
            // make chart
            var c = new cx.Chart() { Size = new Size(1920, 1080) };

            var stringValued = slice.Where(v => v.DataType.ToLower() == "string");
            var line = slice.First().Channel;
            
            c.Titles.Add(line + " " + g.GraphName + " (" + start.ToString("MM/dd") + " - " + end.ToString("MM/dd/yy") + ")");
            c.Titles[0].Font = new Font("Arial", 14, FontStyle.Bold);

            var a = new ChartArea("Production");
            a.AxisY.MajorGrid.LineColor = Color.LightGray;
            a.AxisY.IsStartedFromZero = false;
            a.AxisX.IsMarginVisible = false;
            a.AxisX.LabelStyle.Enabled = false;

            a.AxisX.LabelStyle.Format = "dd/MMM\nhh:mm tt";
            a.AxisX.MajorGrid.LineColor = Color.LightGray;
            a.AxisX.LabelStyle.ForeColor = Color.Black;
            a.AxisX.LabelStyle.Font = new Font("Arial", 14);
            a.AxisX.IsLabelAutoFit = true;
            a.AxisX.LabelStyle.Enabled = true;
            c.ChartAreas.Add(a);

            foreach (var tag in slice.Except(stringValued))
            {
                if (!value.ContainsKey(tag.TagId)) continue;

                var p = g.Plots.Single(t => tag.CanonId == t.TagId);
                var s = new Series(p.Relabel)
                { 
                    ChartType = SeriesChartType.FastLine,
                    XValueType = ChartValueType.DateTime
                };
                s.Points.DataBindXY(time[tag.TagId], value[tag.TagId]);     
                c.Series.Add(s);
                s.ChartArea = "Production";
            }

            var filename = line + " " + g.GraphName + " " + DateTime.Now.ToString("MMddyy") + ".png";
            if (c.Series.Count == 0)
            {
                Console.WriteLine("no data for " + filename);
                return;
            }

            foreach (var tag in stringValued)
            {
                if (!value.ContainsKey(tag.TagId)) continue;
                var values = value[tag.TagId];
                if (values.Count == 0) continue;
                var times = time[tag.TagId];

                for (int j = 1; j < values.Count(); j++) {
                    var t = new StripLine();
                    t.BackColor = ColorTranslator.FromHtml(backgrounds[(j-1) % backgrounds.Length]);
                    t.IntervalOffset = times[j-1].ToOADate();
                    var rdg = times[j].ToOADate();
                    t.StripWidth = rdg - t.IntervalOffset;
                    t.Text = values[j-1].ToString();
                    t.Font = new Font("Arial", 14, FontStyle.Bold);
                    a.AxisX.StripLines.Add(t);
                }
            }
            c.Legends.Add(new Legend("Legend")
            {
                IsDockedInsideChartArea = true,
                DockedToChartArea = "Production",
                BackColor = Color.Transparent
            });

            var outname = Path.Combine(path, filename);
            Console.WriteLine(outname);
            c.SaveImage(outname, ChartImageFormat.Png);
        }
    }
}