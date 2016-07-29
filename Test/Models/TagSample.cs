using System;
using System.Linq;
using System.Collections.Generic;
using NPoco;

namespace Test.Models
{
    public class Value : All
    {
        public double? dvalue { get; set; }
        public long epoch { get; set; }
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

        private static string _linesample = @"
            declare @tags table (tagid int)    
            
            select tagid into @tags from tag t
            join device d on t.deviceid = d.deviceid
            join channel c on d.channelid = c.channelid
            where c.name = @0 and t.name in ('layflat_mm_pv', 'csg_moist_pct', 'csg_glyc_pct')       

            select
                 tagid 
                ,convert(float,value,0) as dvalue
                ,cast(DATEDIFF(s, '1970-01-01 00:00:00', stamp) as bigint)*1000 + cast(DATEPART(ms, stamp) as bigint) as epoch
                ,stamp
            from production p
            join @tag t on p.tagid = t.tagid
            and p.stamp >= @1 and p.stamp <= @2

            ;with asof as (
                select limitid, tagid, stamp, lolo, lo, aim, hi, hihi
                from limit
                where stamp <= @1
            )
            select m.limitid, m.tagid, m.stamp, m.lolo, m.lo, m.aim, m.hi, m.hihi 
            from asof m
            join @tags t on t.tagid = m.tagid
            left join asof n on n.tagid = m.tagid and n.stamp > m.stamp 
            where n.limitid is null
            order by stamp desc
        ";

        public DateTime Start { get; set; }
        public DateTime End { get; set; }

        public double? Average { get; set; }
        public int N { get; set; }
        public double? Stdp { get; set; }
        public double? Minimum { get; set; }
        public double? Maximum { get; set; }

        public Limit limit { get; set; }
        public List<Value> series { get; set; }

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
            limit = _limit;
            series = values;
            Start = _start;
            End = _end;
        }

        public static List<TagSample> Span(string channel, DateTime start, DateTime end)
        {
            List<TagSample> tagsamples = null;

            using (tagDB t = new tagDB())
            {
                var results = t.FetchMultiple<Value, Limit>(_linesample, channel, start.ToStamp(), end.ToStamp());
                var series = results.Item1.ToLookup(k => k.TagId, v => v);
                tagsamples = results.Item2.Select(n => new TagSample(n, series[n.TagId].ToList(), start, end)).ToList();
            }
            return tagsamples;
        }
    }
}