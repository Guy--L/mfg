using System;
using System.Linq;
using System.Collections.Generic;
using NPoco;
using System.IO;
using ClosedXML.Excel;

namespace Test.Models
{
    public class Value : All
    {
        [ResultColumn] public long prdid { get; set; }
        [ResultColumn] public double? dvalue { get; set; }
        [ResultColumn] public long epoch { get; set; }
        [ResultColumn] public int ctrl { get; set; }

        public string print()
        {
            return TagId + " " + Stamp.ToStamp() + ", " + epoch + " " + dvalue;
        }
    }

    public class Trace
    {
        [ResultColumn] public int tagid { get; set; }
        [ResultColumn] public long start { get; set; }
        [ResultColumn] public long stop { get; set; }
        [ResultColumn] public int ctl { get; set; }
    }

    public class TagSample
    {
        private static string _stats = @"
            select 
	              AVG(convert(float,p.value,0)) as Average
	            , stdevp(convert(float,p.value,0)) as Stdp
	            , min(convert(float,p.value,0)) as Minimum
	            , max(convert(float,p.value,0)) as Maximum
	            , count(p.value) as N
            from production p
            join tag t on p.tagid = t.tagid
            join device d on t.DeviceId = d.DeviceId
            join channel c on d.ChannelId = c.ChannelId
            where c.name = @0 and t.name = @1
            and p.stamp >= @2 and p.stamp <= @3
        ";

        private static string _sample = @";
            select convert(float,value,0) as dvalue
                ,dbo.epoch(stamp) as epoch
                ,stamp
            from production p
            join tag t on p.tagid = t.tagid
            join device d on t.DeviceId = d.DeviceId
            join channel c on d.ChannelId = c.ChannelId
            where c.name = @0 and t.name = @1
            and p.stamp >= @2 and p.stamp <= @3

            select top 1 limitid, tagid, stamp, lolo, lo, aim, hi, hihi
            from limitid
            where stamp <= @2
            order by stamp desc
        ";

        private static string _linesample = @";
            declare @@tags table (tagid int, devname varchar(64), tagname varchar(64), limitid int, stamp datetime, lolo float, lo float, aim float, hi float, hihi float)    
            declare @@vals table (tagid int, dvalue float, epoch bigint, stamp datetime, ctrl int)
            
            ;with asof as (
                select limitid, tagid, stamp, round(lolo, 1) as lolo, round(lo, 1) as lo, round(aim, 1) as aim, round(hi, 1) as hi, round(hihi, 1) as hihi
                from limit
                where stamp <= @1
            )
            insert into @@tags 
            select t.tagid, d.name as devname, t.name as tagname, m.limitid, m.stamp, m.lolo, m.lo, m.aim, m.hi, m.hihi
            from tag t
            left join asof m on m.tagid = t.tagid
            left join asof n on n.tagid = m.tagid and n.stamp > m.stamp
            join device d on t.deviceid = d.deviceid
            join channel c on d.channelid = c.channelid
            where c.name = @0
            and t.name in ('layflat_mm_pv', 'csg_moist_pct', 'csg_glyc_pct')       
            and n.limitid is null

            select tagid, devname, tagname, limitid, stamp, lolo, lo, aim, hi, hihi from @@tags

            select p.tagid 
                ,convert(float,p.value,0) as dvalue
                ,round(convert(float,p.value,0),1) as rvalue
                ,dbo.epoch(p.stamp) as epoch
                ,p.stamp as stamp
                ,case when round(convert(float,p.value,0),1) < lolo then -2
                     when round(convert(float,p.value,0),1) > hihi then 2
                     when round(convert(float,p.value,0),1) < lo then -1
                     when round(convert(float,p.value,0),1) > hi then 1
                     else 0
                 end as ctrl
                ,t.lolo, t.lo, t.aim, t.hi, t.hihi
            from production p
            join @@tags t on p.tagid = t.tagid
            and p.stamp >= @1 and p.stamp <= @2
            order by p.tagid, p.stamp
        ";

        private static string _trace = @"
            ;with a as (
                select tagid
                    , abs(ctrl) as ctl
                    , epoch
					, prvepoch = lag(epoch, 1, epoch) over (order by epoch)
                from @@vals
             )
             select tagid
                , ctl
                , epoch as stop
                , case when prvepoch > epoch then epoch else prvepoch end as start 
             from a
             where ctl > 0
             order by tagid, start
        ";

        private static string _xspec = @"
            ;with a as (
                select tagid
                    , abs(ctrl) as ctl
                    , epoch
					, prvepoch = lag(epoch, 1, epoch) over (order by epoch)
                    , prvctl = lag(abs(ctrl), 1, abs(ctrl)) 
                        over (order by epoch) 
                from @@vals
            ), b as (
                select tagid
                    , ctl
                    , epoch
					, prvepoch
                    , ranker = sum(case when ctl = prvctl then 0 else 1 end) 
                        over (order by epoch, ctl)
                from a
            )
            select tagid
                ,ctl
                ,min(prvepoch) start
                ,max(epoch) stop
            from b
            where ctl > 0
            group by tagid, ctl, ranker
            order by tagid, start
        ";

        public DateTime Start { get; set; }
        public DateTime End { get; set; }

        public double? Average { get; set; }
        public int N { get; set; }
        public double? Stdp { get; set; }
        public double? Minimum { get; set; }
        public double? Maximum { get; set; }

        public int id { get; set; }
        public string Label { get; set; }
        public Limit limit { get; set; }
        public List<Value> series { get; set; }
        public List<Trace> xcontrol { get; set; }
        public List<Trace> xspec { get; set; }

        public TagSample(int lineid, string tagname, DateTime start, DateTime end, bool detail)
        {
            var channel = Line.names[lineid].Replace("-", "");
            using (tagDB t = new tagDB())
            {
                t.SingleInto(this, _stats, channel, tagname, start.ToStamp(), end.ToStamp());
                if (detail)
                {
                    var results = t.FetchMultiple<Value, Limit>(_sample, channel, tagname, start.ToStamp(), end.ToStamp());
                    series = results.Item1;
                    limit = results.Item2.SingleOrDefault();
                }
            }
        }

        public TagSample(Limit _limit, List<Value> values, DateTime _start, DateTime _end)
        {
            Label = _limit.label;
            limit = _limit;
            series = values;
            Start = _start;
            End = _end;
        }

        public int Export(XLWorkbook wb, DateTime start, DateTime end)
        {
            int tot = 0;
            string path;

            if (limit == null)
                path = "tag." + (series.Any() ? series.First().TagId.ToString() : "name");
            else
                path = limit.devname + '.' + limit.tagname;

            var ws = wb.Worksheets.Add(path);
            IEnumerable<Value> all = series;

            if (start != null)
                all = all.Where(d => d.Stamp >= start);

            if (end != null)
                all = all.Where(d => d.Stamp <= end);

            all.Select((d, i) =>
            {
                ws.Row(i + 1).Cell(1).SetValue<DateTime>(d.Stamp);
                ws.Row(i + 1).Cell(2).SetValue<double>(d.dvalue ?? 0.0);
                return i;
            }).ToList();

            if (limit != null)
            {
                var wl = wb.Worksheets.Add(path + "_limits");
                wl.Row(1).Cell(1).SetValue<DateTime>(limit.Stamp);
                wl.Row(1).Cell(2).SetValue<double>(limit.LoLo);
                wl.Row(1).Cell(3).SetValue<double>(limit.Lo);
                wl.Row(1).Cell(4).SetValue<double>(limit.Aim);
                wl.Row(1).Cell(5).SetValue<double>(limit.Hi);
                wl.Row(1).Cell(6).SetValue<double>(limit.HiHi);
            }
            return tot;
        }

        public static List<TagSample> Span(string channel, DateTime start, DateTime end)
        {
            List<TagSample> tagsamples = null;

            using (tagDB t = new tagDB())
            {
                try
                {
                    t.CommandTimeout = 60000;
                    var results = t.FetchMultiple<Limit, Value>(_linesample, channel, start.ToStamp(), end.ToStamp());
                    var series = results.Item2.ToLookup(k => k.TagId, v => v);
                    tagsamples = results.Item1.Select(
                        n => new TagSample(n,
                                        series[n.TagId].ToList(),
                                        start,
                                        end)).ToList();

                }
                catch (Exception e)
                {
                    Elmah.ErrorSignal.FromCurrentContext().Raise(new Exception("Error getting detail for tags ", e));
                }
            }
            return tagsamples;
        }
    }
}