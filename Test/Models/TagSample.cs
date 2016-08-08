using System;
using System.Linq;
using System.Collections.Generic;
using NPoco;

namespace Test.Models
{
    public class Value : All
    {
        [ResultColumn] public double? dvalue { get; set; }
        [ResultColumn] public long epoch { get; set; }
        [ResultColumn] public int ctrl { get; set; }
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
                ,cast(DATEDIFF(s, '1970-01-01 00:00:00', stamp) as bigint)*1000 + cast(DATEPART(ms, stamp) as bigint) as epoch
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
            declare @@tags table (tagid int, tagname varchar(64), limitid int, stamp datetime, lolo float, lo float, aim float, hi float, hihi float)    
            declare @@vals table (tagid int, dvalue float, epoch bigint, stamp datetime, ctrl int)
            
            ;with asof as (
                select limitid, tagid, stamp, round(lolo, 1) as lolo, round(lo, 1) as lo, round(aim, 1) as aim, round(hi, 1) as hi, round(hihi, 1) as hihi
                from limit
                where stamp <= @1
            )
            insert into @@tags 
            select t.tagid, t.name as tagname, m.limitid, m.stamp, m.lolo, m.lo, m.aim, m.hi, m.hihi
            from tag t
            left join asof m on m.tagid = t.tagid
            left join asof n on n.tagid = m.tagid and n.stamp > m.stamp
            join device d on t.deviceid = d.deviceid
            join channel c on d.channelid = c.channelid
            where c.name = @0
            and t.name in ('layflat_mm_pv', 'csg_moist_pct', 'csg_glyc_pct')       
            and n.limitid is null

            select tagid, tagname, limitid, stamp, lolo, lo, aim, hi, hihi from @@tags

            insert into @@vals
            select tagid, dvalue, epoch, stamp, 
                case when rvalue < lolo then -2
                     when rvalue > hihi then 2
                     when rvalue < lo then -1
                     when rvalue > hi then 1
                     else 0
                end as ctrl from (
                    select p.tagid 
                        ,convert(float,p.value,0) as dvalue
                        ,round(convert(float,p.value,0),1) as rvalue
                        ,dbo.epoch(p.stamp) as epoch
                        ,p.stamp
                        ,t.lolo, t.lo, t.aim, t.hi, t.hihi
                    from production p
                    join @@tags t on p.tagid = t.tagid
                    and p.stamp >= @1 and p.stamp <= @2
                    where t.tagname != 'layflat_mm_pv'
                        union all
                    select a.tagid
                        ,avg(a.val) as dvalue
                        ,round(avg(a.val),1) as rvalue 
                        ,dbo.epoch(max(a.stamp)) as epoch
                        ,max(a.stamp) as stamp
                        ,a.lolo, a.lo, a.aim, a.hi, a.hihi
                    from (
                        select p.tagid, convert(float,p.value,0) as val
                            ,p.stamp
                            ,((row_number() over (order by p.stamp))-1)/@3 as mesh
							,t.lolo, t.lo, t.aim, t.hi, t.hihi
                        from production p
                        join @@tags t on p.tagid = t.tagid
                        and p.stamp >= @1 and p.stamp <= @2
                        where t.tagname = 'layflat_mm_pv'
                    ) a
                    group by mesh, tagid, lolo, lo, aim, hi, hihi
                ) v
            order by tagid, epoch

            select tagid, dvalue, epoch, stamp, ctrl from @@vals

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

        public TagSample(Limit _limit, List<Value> values, List<Trace> xs, DateTime _start, DateTime _end)
        {
            Label = _limit.label;
            limit = _limit;
            series = values;
            xspec = xs;
            Start = _start;
            End = _end;
        }

        public static List<TagSample> Span(string channel, DateTime start, DateTime end)
        {
            List<TagSample> tagsamples = null;
            List<Trace> trace = null;

            using (tagDB t = new tagDB())
            {
                try
                {
                    var results = t.FetchMultiple<Limit, Value, Trace>(_linesample, channel, start.ToStamp(), end.ToStamp(), 10);
                    var series = results.Item2.ToLookup(k => k.TagId, v => v);
                    var xspec = results.Item3.ToLookup(k => k.tagid, v => v);
                    tagsamples = results.Item1.Select(
                        n => new TagSample(n,
                                        series[n.TagId].ToList(),
                                        xspec[n.TagId].ToList(),
                                        start,
                                        end)).ToList();
                    
                }
                catch (Exception e)
                {
                    var tst = e;
                }
            }
            return tagsamples;
        }
    }
}