using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms.DataVisualization.Charting;
using Microsoft.AspNet.SignalR;
using Test.Hubs;
using cx = System.Windows.Forms.DataVisualization.Charting;

namespace Test.Models
{
    public class SolutionChart
    {
        public static string path { get; set; }
        public string axes { get; set; }
        public string charts { get; set; }
        public string series { get; set; }

        public SolutionChart(SolutionTests t, int chart)
        {
            switch (chart)
            {
                case 0: Chart0(t); break;
                case 1: Chart1(t); break;
                case 2: Chart2(t); break;
                case 3: Chart3(t); break;
                case 4: Chart4(t); break;
                default: break;
            }
        }

        public static void UpdateDeck(int id)
        {
            var tests = SolutionTest.Recent(id, 30);
            if (!(tests?.Count() > 0))
                return; 

            var batches = BatchSpan.Since(id, 30);

            Deck d = new Deck("Solutions");

            var time = tests.Select(t => t.DateTime).ToList();
            var series = d.Slides.SelectMany(s => s.content).GroupBy(x => x.SeriesId).Select(y => y.First()).ToDictionary(k => k.SeriesId);
            foreach(var line in series.Values)
            {
                line.setvalues(tests);
                line.core = new cx.Series(line.Title)
                {
                    ChartType = SeriesChartType.FastLine,
                    XValueType = ChartValueType.DateTime,
                    AxisLabel = line.YLabel,
                    BorderWidth = 2,
                    Color = Color.FromName(line.ForeGround)
                };
                line.core.Points.DataBindXY(time, line.values);
            }
            foreach (var s in d.Slides) { 
                var c = new Chart() { Size = new Size(1920, 1080) };

                c.BackImage = Path.Combine(path, tests.First().System + ".png");
                c.Titles.Add("System "+tests.First().System+": "+string.Join(", ", s.content.Select(x=>x.Title).ToArray()));
                c.Titles[0].Font = new Font("Arial", 14, FontStyle.Bold);
                
                if (s.content.Count == 1) c.Titles[0].ForeColor = Color.FromName(s.content[0].ForeGround);

                ChartArea bottom = null;
                foreach(var line in s.content)
                {
                    var x = series[line.SeriesId];
                    var a = new ChartArea(x.Title);
                    a.AxisY.MajorGrid.LineColor = Color.LightGray;
                    a.AxisX.MajorGrid.LineColor = Color.Gray;
                    a.AxisY.LabelStyle.Font = new Font("Arial", 14);
                    a.AxisY.Title = x.YLabel;
                    a.AxisY.TitleFont = new Font("Arial", 14);
                    a.AxisY.IsStartedFromZero = false;
                    a.AxisX.IsMarginVisible = false;
                    a.AxisX.LabelStyle.Enabled = false;
                    a.BackColor = Color.Transparent;
                    c.ChartAreas.Add(a);

                    c.Series.Add(x.core);
                    
                    x.core.ChartArea = x.Title;
                    bottom = a;
                }
                bottom.AxisX.LabelStyle.Format = "dd/MMM\nhh:mm";
                bottom.AxisX.MajorGrid.LineColor = Color.Gray;
                bottom.AxisX.LabelStyle.ForeColor = Color.Black;
                bottom.AxisX.LabelStyle.Font = new Font("Arial", 14);
                bottom.AxisX.IsLabelAutoFit = true;
                bottom.AxisX.LabelStyle.Enabled = true;

                foreach(var batch in batches)
                {
                    var t = new StripLine();
                    t.BorderWidth = 25;
                    t.BorderColor = Color.FromArgb(127, Color.Aquamarine);
                    t.BackColor = Color.Transparent;
                    t.IntervalOffset = batch.Begin.ToOADate();
                    t.IntervalOffsetType = DateTimeIntervalType.Days;
                    t.StripWidth = batch.End.ToOADate() - t.IntervalOffset;
                    t.StripWidthType = DateTimeIntervalType.Days;
                    t.Text = batch.Recipe;
                    t.Font = new Font("Arial", 14, FontStyle.Bold);
                    bottom.AxisX.StripLines.Add(t);
                }

                c.SaveImage(Path.Combine(path, id + "_" + s.FileNameSuffix + ".png"), ChartImageFormat.Png);
            };

            var hub = GlobalHost.ConnectionManager.GetHubContext<RefreshHub>();
            hub.Clients.Group("Deck").refresh();
        }

        private void Chart(SolutionTests t, List<ReadingField> readings)
        {
            List<string[]> all = new List<string[]>(readings.Count);
            var x = t.list.Select(r => "new Date(" + r.DateTime.ToInt() + "000)");

            readings.ForEach(r => r.reflect(t.list.First()));

            var sequence = readings.Select((p, q) => "d" + q + "=[" +
                string.Join(",", t.list.Select(s => "[new Date(" + s.DateTime.ToInt() + "000)," + p.propInfo.GetValue(s, null).ToString() + "]").ToArray()) + "]").ToList();
            var timelines = readings.Select((r, p) => "d"+p+" = ["+x.Zip(t.list.Select(s => r.propInfo.GetValue(s, null)), (n, m) => "[" + n + "," + m + "]").ToArray()+"]");
            series = "var " + string.Join(",\n", timelines.ToArray()) + ";";

            //charts = string.Join(",\n",readings.Select((r, p) => "{data:d" + p + ",points:{show:true},lines:{show:true},label:'" + r.Title + "',color:'"+r.LineColor+"'}").ToArray());
        }

        private void ChartN(SolutionTests t)
        {
            using (labDB db = new labDB())
            {
                var chart = db.Fetch<ReadingField>(" where readingfieldid in (1,2,3)");
                Chart(t, chart.ToList());
            }
        }

        private void Chart0(SolutionTests t)
        {
            List<string[]> all = new List<string[]>(3);
            var x = t.list.Select(r => "new Date(" + r.DateTime.ToInt() + "000)");
            all.Add(x.Zip(t.list.Select(r => r.Glycerin),       (n, m) => "[" + n + "," + m.ToString() + "]").ToArray());
            all.Add(x.Zip(t.list.Select(r => r.CMC),            (n, m) => "[" + n + "," + m.ToString() + "]").ToArray());
            all.Add(x.Zip(t.list.Select(r => r.CasingGlycerin), (n, m) => "[" + n + "," + m.ToString() + "]").ToArray());

            series = "var " + string.Join(",\n", all.Select((b, c) => "d" + c + " = [" + string.Join(",", b) + "]").ToArray()) + ";";
            charts = @"{ data : d0, points: {show: true}, lines: {show: true}, label : 'Solution Glycerin', color: 'blue' },
                       { data : d1, points: {show: true}, lines: {show: true}, label : 'CMC', color: 'purple' },
                       { data : d2, points: {show: true}, lines: {show: true}, label : 'Casing Glycerin Avg', yaxis: 2, color: 'red' }";
            axes = @"  yaxis:  { autoscale: true, autoscaleMargin: .2, title: '% CMC & Glycerin'  },
                       y2axis: { autoscale: true, autoscaleMargin: .2, title: 'Casing Glycerin Avg %', color: 'red' },
                       title: 'System " + t.System+@"',
                       subtitle: 'Solution Recipe "+t.Recipe+@"',";
        }
        
        // Glycerin

        private void Chart2(SolutionTests t)
        {
            List<string[]> all = new List<string[]>(3);            
            var x = t.list.Select(r => "new Date(" + r.DateTime.ToInt() + "000)");
            all.Add(x.Zip(t.list.Select(r => r.Glycerin),       (n, m) => "[" + n + "," + m.ToString() + "]").ToArray());

            series = "var " + string.Join(",\n", all.Select((b, c) => "d" + c + " = [" + string.Join(",", b) + "]").ToArray()) + ";";
            charts = @"{ data : d0, points: {show: true}, lines: {show: true}, label : 'Solution Glycerin', color: 'blue' }";
            axes = @"  yaxis:  { autoscale: true, autoscaleMargin: .2, title: '% Glycerin' },
                       title: 'System " + t.System + @"',
                       subtitle: 'Solution Recipe " + t.Recipe + @"',";
        }

        // CMC

        private void Chart3(SolutionTests t)
        {
            List<string[]> all = new List<string[]>(3);
            var x = t.list.Select(r => "new Date(" + r.DateTime.ToInt() + "000)");
            all.Add(x.Zip(t.list.Select(r => r.CMC), (n, m) => "[" + n + "," + m.ToString() + "]").ToArray());

            series = "var " + string.Join(",\n", all.Select((b, c) => "d" + c + " = [" + string.Join(",", b) + "]").ToArray()) + ";";
            charts = @"{ data : d0, points: {show: true}, lines: {show: true}, label : 'CMC', color: 'purple' }";
            axes = @"  yaxis:  { autoscale: true, autoscaleMargin: .2, title: '% CMC'  },
                       title: 'System " + t.System + @"',
                       subtitle: 'Solution Recipe " + t.Recipe + @"',";
        }

        // Conductivity

        private void Chart4(SolutionTests t)
        {
            List<string[]> all = new List<string[]>(3);
            var x = t.list.Select(r => "new Date(" + r.DateTime.ToInt() + "000)");
            all.Add(x.Zip(t.list.Select(r => r.Conductivity), (n, m) => "[" + n + "," + m.ToString() + "]").ToArray());

            series = "var " + string.Join(",\n", all.Select((b, c) => "d" + c + " = [" + string.Join(",", b) + "]").ToArray()) + ";";
            charts = @"{ data : d0, points: {show: true}, lines: {show: true}, label : 'uS', color: 'green' }";
            axes = @"  yaxis:  { autoscale: true, autoscaleMargin: .2, title: 'Conductivity [uS]' },
                       title: 'System " + t.System + @"',
                       subtitle: 'Solution Recipe " + t.Recipe + @"',";
        }

        private void Chart1(SolutionTests t)
        {
            List<string[]> all = new List<string[]>(3);
            var x = t.list.Select(r => "new Date(" + r.DateTime.ToInt() + "000)");
            all.Add(x.Zip(t.list.Select(r => r.Glycerin), (n, m) => "[" + n + "," + m.ToString() + "]").ToArray());
            all.Add(x.Zip(t.list.Select(r => r.CasingGlycerin), (n, m) => "[" + n + "," + m.ToString() + "]").ToArray());
            all.Add(x.Zip(t.list.Select(r => r.Conductivity), (n, m) => "[" + n + "," + m.ToString() + "]").ToArray());

            series = "var " + string.Join(",\n", all.Select((b, c) => "d" + c + " = [" + string.Join(",", b) + "]").ToArray()) + ";";
            charts = @"{ data : d0, points: {show: true}, lines: {show: true}, label : 'Solution Glycerin', color: 'blue' },
                       { data : d1, points: {show: true}, lines: {show: true}, label : 'Casing Glycerin Avg', color: 'red' },
                       { data : d2, points: {show: true}, lines: {show: true}, label : 'Conductivity', yaxis: 2, color: 'green' }";
            axes = @"  yaxis:  { autoscale: true, autoscaleMargin: .2, title: '% Glycerin, solution and casing avg' },
                       y2axis: { autoscale: true, autoscaleMargin: .2, title: 'Conductivity [uS]', color: 'green' },
                       title: 'System " + t.System + @"',
                       subtitle: 'Solution Recipe " + t.Recipe + @"',";
        }

        private StripLine BatchStrip(Color background, DateTime A, DateTime B, string title)
        {
            StripLine s = new StripLine();
            s.Interval = 0;
            s.IntervalOffset = A.ToOADate();
            s.IntervalOffsetType = DateTimeIntervalType.Days;
            s.StripWidth = B.ToOADate() - s.IntervalOffset;
            s.StripWidthType = DateTimeIntervalType.Days;
            s.BackColor = background;

            return s;
        }
    }
}