using System;
using System.Collections.Generic;
using NPoco;
using System.Linq;

namespace Test.Models
{
    public class Run
    {
        private static string _latest = @" left join cut m on n.lineid = m.lineid and m.stamp > n.stamp ";
        private static string _where = @" m.stamp is null and ";
        private static string _current = @" where a.stamp < @1 ";
        private static string _byLineTx = @" where a.linetxid = @0 ";
        private static string _product = @" n.productcodeid = @0 and s.Code = 'RP' and ";
        private static string _byProduct = @";
            with cut as (
	            select linetxid, lineid, stamp, productcodeid, statusid, endstamp from (
		            select distinct linetxid, lineid, stamp, productcodeid, statusid, 
			            lead(stamp, 1, getdate()) over (partition by lineid order by stamp) as endstamp
		            from linetx
	            ) a
	            {0}
            )
            select 
                  n.linetxid
	            , n.lineid
	            , n.stamp
	            , n.endstamp
                , p.productcodeid
                , p.productcode
                , p.productspec
	            , count(x.sampleid) as [samples]
            from cut n
            {1}
            join line l on l.lineid = n.lineid
            join unit u on u.unitid = l.unitid
            join productcode p on p.productcodeid = n.productcodeid
            join [status] s on s.StatusId = n.StatusId
            left join [sample] x on x.lineid = n.lineid
            where {2}
                x.stamp >= n.stamp 
                and x.stamp <= n.endstamp
            group by                   
                  n.linetxid
	            , n.lineid
	            , n.stamp
	            , n.endstamp
                , p.productcodeid
                , p.productcode
                , p.productspec
            order by {3} n.stamp
        ";

        private static string _byCode = @";
            with cut as (
	            select linetxid, lineid, stamp, productcodeid, statusid, endstamp from (
		            select distinct linetxid, lineid, stamp, productcodeid, statusid, 
			            lead(stamp, 1, getdate()) over (partition by lineid order by stamp) as endstamp
		            from linetx
	            ) a
            )
            select 
                  n.linetxid
	            , n.lineid
	            , n.stamp
	            , n.endstamp
                , n.productcodeid
	            , count(x.sampleid) as [samples]
            from cut n
            join line l on l.lineid = n.lineid
            join unit u on u.unitid = l.unitid
            join productcode p on p.ProductCodeId = n.ProductCodeId
            join [status] s on s.StatusId = n.StatusId
            left join [sample] x on x.lineid = n.lineid
            where productcode like '@0%'
            x.stamp >= n.stamp 
            and x.stamp <= n.endstamp
            and s.Code = 'RP'
            group by n.linetxid, n.lineid, n.stamp, n.endstamp
            order by n.lineid, n.stamp
        ";

        private static string[] detail = new string[]
        {
            "csg_glyc_pct", 
            "csg_moist_pct",
            "layflat_mm_pv"
        };

        public string Name { get { return Line.names[LineId].Replace("-", ""); } }
        public int LineId { get; set; }
        public int LineTxId { get; set; }
        public DateTime Stamp { get; set; }
        public DateTime EndStamp { get; set; }
        public int Samples { get; set; }
        public string ProductCode { get; set; }
        public string ProductSpec { get; set; }
        public int ProductCodeId { get; set; }

        public string begin { get { return Stamp.ToShort(); } }
        public string end { get { return EndStamp.ToShort(); } }

        [ResultColumn] public List<Sample> samples { get; set; }

        public List<TagSample> series {get;set;}


        public Run() { }

        public Run(Context c)
        {
            LineTxId = 0;
            Stamp = c.Start;
            EndStamp = c.End;
            LineId = c.LaneId;
        }

        public Run(int id)
        {
            Run run = null;
            using (labDB d = new labDB())
            {
                try
                {
                    run = d.Fetch<Run>(string.Format(_byProduct, _byLineTx, "", "", ""), id).SingleOrDefault();
                }
                catch (Exception e)
                {
                    Elmah.ErrorSignal.FromCurrentContext().Raise(new Exception("Error finding run by line tx ", e));
                }
            }
            series = detail.Select(t => new TagSample(run.LineId, t, run.Stamp, run.EndStamp, true)).ToList();
        }

        public static List<Run> RunsNow(DateTime asof, int productcodeid)
        {
            List<Run> runs = null;
            using (labDB d = new labDB())
            {
                try
                {
                    runs = d.Fetch<Run>(string.Format(_byProduct, _current, _latest, _where, ""), productcodeid, asof);
                }
                catch (Exception e)
                {
                    Elmah.ErrorSignal.FromCurrentContext().Raise(new Exception("Error finding current runs ", e));
                }
            }
            return runs;
        }

        public static List<Run> RunsEver(int productcodeid)
        {
            List<Run> runs = null;
            using (labDB d = new labDB())
            {
                try
                {
                    runs = d.Fetch<Run>(string.Format(_byProduct, "", "", _product, "n.lineid,"), productcodeid);
                }
                catch (Exception e)
                {
                    Elmah.ErrorSignal.FromCurrentContext().Raise(new Exception("Error finding all runs by productcodeid ", e));
                }
            }
            return runs;
        }

        public static List<Run> RunsEver(string productcode)
        {
            List<Run> runs = null;
            using (labDB d = new labDB())
            {
                try
                {
                    runs = d.Fetch<Run>(_byCode, productcode);
                }
                catch (Exception e)
                {
                    Elmah.ErrorSignal.FromCurrentContext().Raise(new Exception("Error finding all runs by productcode ", e));
                }
            }
            return runs;
        }
    }
}