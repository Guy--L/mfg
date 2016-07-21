using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Test.Models
{
    public class TagSample
    {
        private static string _sample = @"
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
            left join limit n on t.tagid = n.tagid         
            where c.name = @0 and t.name = @1
            and p.stamp >= @2 and p.stamp <= @3
        ";
        public DateTime Start { get; set; }
        public DateTime End { get; set; }

        public double? Average { get; set; }
        public int N { get; set; }
        public double? Stdp { get; set; }
        public double? Minimum { get; set; }
        public double? Maximum { get; set; }

        public TagSample(int lineid, string tagname, DateTime start, DateTime end)
        {
            using (tagDB t = new tagDB())
            {
                t.SingleInto(this, _sample, Line.names[lineid].Replace("-", ""), tagname, start.ToStamp(), end.ToStamp());
            }
        }
    }
}