using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Quartz;
using Tags.Models;
using System.Web.Configuration;
using System.Windows.Forms.DataVisualization.Charting;
using cx = System.Windows.Forms.DataVisualization.Charting;
using System.Drawing;

namespace Tags.Jobs
{
    public class ChartJob : JobBase
    {
        private static string _alldata = @";
            with end1 as (
                select TagId, '{2}' Stamp, Value from (
                 select n.tagid, n.stamp, n.value, 
                  ROW_NUMBER() OVER (PARTITION BY tagid ORDER BY stamp asc) rn
                 from [All] n
                 join dbo.SplitInts('{0}',',') s on s.Item = n.TagId
                 where stamp > '{2}'
                ) a where rn = 1
            ),
            end2 as (
                select c.TagId, '{2}' Stamp, c.Value
                from [Current] c
                join dbo.SplitInts('{0}',',') s on s.Item = c.TagId
            )
            select * from (
                select TagId, '{1}' Stamp, Value from (
                 select n.tagid, n.stamp, n.value, 
                  ROW_NUMBER() OVER (PARTITION BY tagid ORDER BY stamp DESC) rn
                 from [All] n
                 join dbo.SplitInts('{0}',',') s on s.Item = n.TagId
                 where stamp < '{1}'
                ) a where rn = 1
            union all
                select a.TagId, a.Stamp, a.Value
                from [All] a
                join dbo.SplitInts('{0}',',') s on s.Item = a.TagId
                where a.Stamp >= '{1}' and a.Stamp <= '{2}' 
            union all
                select * from end1
            union all
                select * from end2 where not exists (
                    select null from end1
                )
            ) q
            order by q.TagId, q.Stamp           
        ";

        private List<Graph> deck;
        private Dictionary<int, List<DateTime>> time;
        private Dictionary<int, List<object>> value;
        private ILookup<int, Limit> limits;
        private DateTime start;
        private DateTime end;

        public ChartJob()
        { }

        public ChartJob(int reviewId)
        {
            jobid = reviewId;
            review = Review.SingleOrDefault(jobid);
        }

        public override void ExecuteJob(IJobExecutionContext context)
        {
            TimeSpan interval;
            var trigger = context.Trigger as ICronTrigger;
            var next = trigger.GetNextFireTimeUtc();
            var ago = DateTime.Now;
            interval = next.Value - ago;
            Render(jobid, ago - interval, ago);
        }

        /// <summary>
        /// Render graphs listed under the review given for the time interval given.
        /// </summary>
        /// <param name="reviewId">Review containing graphs for rendering</param>
        /// <param name="a">Start time</param>
        /// <param name="b">End time</param>
        public void Render(int reviewId, DateTime a, DateTime b)
        {
            var path = WebConfigurationManager.AppSettings["graphjoboutdir"];
            var template = WebConfigurationManager.AppSettings["graphjobtemplate"];
            var root = Path.Combine(review.Path, b.ToString(review.Template));

            time = new Dictionary<int, List<DateTime>>();   // cache data between graphs
            value = new Dictionary<int, List<object>>();

            deck = Graph.plotsByReview(reviewId);           // get all graphs in this review

            deck.Select(g =>
            {
                end = b;
                start = a;

                var channels = g.Canon();           // get all tags for all lines
                var tags = Read(g);                 // get data for period and list of tags

                channels.Select(line =>
                {
                    var linedir = Path.Combine(root, line);
                    var slice = tags.Where(t => t.Channel == line);

                    Render(g, linedir, slice);      // create chart for this line and  
                    return 1;
                }).ToList();
                return 1;
            }).ToList();
        }

        /// <summary>
        /// Read all the data across all devices contained in this graph
        /// Store tag timestamps and values in respective member dictionaries
        /// </summary>
        /// <param name="g">Graph Id</param>
        /// <returns>List of Tags in this Graph</returns>
        public List<Tag> Read(Graph g)
        {
            var tagsInGraph = g.Plots.SelectMany(p => p.tags);
            var tagtype = tagsInGraph.ToDictionary(k => k.TagId, v => Tag.types[v.DataType]);

            var taglist = string.Join(",", tagsInGraph.Select(i => i.TagId).Except(time.Keys).ToArray());

            using (tagDB t = new tagDB())
            {
                var query = string.Format(_alldata, taglist, start.ToStamp(), end.ToStamp());
                var all = t.Fetch<All>(query).ToLookup(k => k.TagId, v => v);
                limits = Limit.Specs(t, taglist, start, end).ToLookup(l => l.TagId);        // get limits for those tags that have them attached

                all.Select(a =>
                {
                    var limited = limits.Contains(a.Key);
                    var envelope = limited ? limits[a.Key]: null;
                    time[a.Key] = a.Select(u => u.Stamp).ToList();
                    value[a.Key] = a.Select(u =>
                    {
                        var p = tagtype[u.TagId];
                        var blank = ((p == typeof(Double) || p == typeof(Int32)) && u.Value == "");
                        var v = blank?0:Convert.ChangeType(u.Value, p);
                        if (!limited) return v;
                        var prior = envelope.LastOrDefault(m => m.Stamp <= u.Stamp);
                        if (prior == null) return v;
                        return prior.Clip(v);
                    }).ToList();
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

        /// <summary>
        /// Render one graph for this device
        /// </summary>
        /// <param name="g">graph id</param>
        /// <param name="path">path under which to store graph</param>
        /// <param name="slice">list of graph relevant tags for this device</param>
        public void Render(Graph g, string path, IEnumerable<Tag> slice)
        {
            // get data for interval and channel
            // make chart
            var c = new cx.Chart() { Size = new Size(2560, 1440) };

            var stringValued = slice.Where(v => v.DataType.ToLower() == "string");
            var line = slice.First().Channel;
            var tids = slice.Select(s => s.TagId);
            var lims = limits.Where(m => tids.Contains(m.Key));

            c.Titles.Add(line + " " + g.GraphName + " (" + start.ToString("MM/dd") + " - " + end.ToString("MM/dd/yy") + ")");
            c.Titles[0].Font = new Font("Arial", 16, FontStyle.Bold);

            var a = new ChartArea("Production");
            a.AxisY.MajorGrid.LineColor = Color.LightGray;
            a.AxisY.IsStartedFromZero = false;
            a.AxisY.LabelStyle.Font = new Font("Arial", 16);

            a.AxisX.IsMarginVisible = false;
            a.AxisX.LabelStyle.Enabled = false;

            a.AxisX.LabelStyle.Format = "dd/MMM\nhh:mm tt";
            a.AxisX.MajorGrid.LineColor = Color.LightGray;
            a.AxisX.LabelStyle.ForeColor = Color.Black;
            a.AxisX.LabelStyle.Font = new Font("Arial", 16);
            a.AxisX.IsLabelAutoFit = true;
            a.AxisX.LabelStyle.Enabled = true;
            c.ChartAreas.Add(a);

            foreach (var lim in lims)
            {
                if (!value.ContainsKey(lim.Key)) continue;

                var lolo = new Series("") { ChartType = SeriesChartType.StepLine, XValueType = ChartValueType.DateTime, Color = Color.Red, IsVisibleInLegend = false };
                var lo = new Series("") { ChartType = SeriesChartType.StepLine, XValueType = ChartValueType.DateTime, Color = Color.Green, IsVisibleInLegend = false };
                var aim = new Series("") { ChartType = SeriesChartType.StepLine, XValueType = ChartValueType.DateTime, Color = Color.Blue, IsVisibleInLegend = false };
                var hi = new Series("") { ChartType = SeriesChartType.StepLine, XValueType = ChartValueType.DateTime, Color = Color.Green, IsVisibleInLegend = false };
                var hihi = new Series("") { ChartType = SeriesChartType.StepLine, XValueType = ChartValueType.DateTime, Color = Color.Red, IsVisibleInLegend = false };

                var time = lim.Select(l => l.Stamp).ToList();
                lolo.Points.DataBindXY(time, lim.Select(l => l.LoLo).ToList());
                lo.Points.DataBindXY(time, lim.Select(l => l.Lo).ToList());
                aim.Points.DataBindXY(time, lim.Select(l => l.Aim).ToList());
                hi.Points.DataBindXY(time, lim.Select(l => l.Hi).ToList());
                hihi.Points.DataBindXY(time, lim.Select(l => l.HiHi).ToList());

                c.Series.Add(lolo); c.Series.Add(lo); c.Series.Add(aim); c.Series.Add(hi); c.Series.Add(hihi);
                lolo.ChartArea = lo.ChartArea = aim.ChartArea = hi.ChartArea = hihi.ChartArea = "Production";
            }

            foreach (var tag in slice.Except(stringValued))
            {
                if (!value.ContainsKey(tag.TagId)) continue;

                var p = g.Plots.Single(t => tag.CanonId == t.TagId);
                var s = new Series(p.Relabel)
                { 
                    ChartType = tag.IsSetPoint?SeriesChartType.StepLine:SeriesChartType.FastLine,
                    XValueType = ChartValueType.DateTime
                };
                s.Points.DataBindXY(time[tag.TagId], value[tag.TagId]);     
                c.Series.Add(s);
                s.ChartArea = "Production";
            }

            var filename = line + " " + g.GraphName + " " + end.ToString("MMddyy") + ".png";
            if (c.Series.Count == 0)
            {
//                Elmah.ErrorSignal.FromCurrentContext().Raise(new Exception("WARNING!  Current database version ("));
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
                    t.BackColor = Color.FromArgb(127, ColorTranslator.FromHtml(backgrounds[(j-1) % backgrounds.Length]));
                    t.IntervalOffset = times[j-1].ToOADate();
                    var rdg = times[j].ToOADate();
                    t.StripWidth = rdg - t.IntervalOffset;
                    t.Text = values[j-1].ToString();
                    t.Font = new Font("Arial", 16, FontStyle.Bold);
                    a.AxisX.StripLines.Add(t);
                }
            }
            c.Legends.Add(new Legend("Legend")
            {
                IsDockedInsideChartArea = false,
                DockedToChartArea = "Production",
                BackColor = Color.FromArgb(192, Color.White)
            });

            Directory.CreateDirectory(path);         // create directory for charts
            var outname = Path.Combine(path, filename);
            Console.WriteLine(outname);
            c.SaveImage(outname, ChartImageFormat.Png);
        }
    }
}