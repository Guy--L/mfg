﻿using System;
using System.Collections.Generic;

namespace Test.Models
{
    public class Run
    {
        private static string _latest = @" left join cut m on n.lineid = m.lineid and m.stamp > n.stamp ";
        private static string _where = @" m.stamp is null and ";
        private static string _current = @"
            where a.stamp < @1
        ";

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
	            , count(x.sampleid) as [samples]
            from cut n
            {1}
            join line l on l.lineid = n.lineid
            join unit u on u.unitid = l.unitid
            join productcode p on p.ProductCodeId = n.ProductCodeId
            join [status] s on s.StatusId = n.StatusId
            left join [sample] x on x.lineid = n.lineid
            where {2}
            x.stamp >= n.stamp 
            and x.stamp <= n.endstamp
            and n.productcodeid = @0
            and s.Code = 'RP'
            group by n.linetxid, n.lineid, n.stamp, n.endstamp
            order by n.stamp
        ";

        public string Name { get { return Line.names[LineId].Replace("-",""); } }
        public int LineId { get; set; }
        public int LineTxId { get; set; }
        public DateTime Stamp { get; set; }
        public DateTime EndStamp { get; set; }
        public int Samples { get; set; }

        public string begin { get { return Stamp.ToShort(); } }
        public string end { get { return EndStamp.ToShort(); } }

        public static List<Run> RunsNow(DateTime asof, int productcodeid)
        {
            List<Run> runs = null;
            using (labDB d = new labDB())
            {
                try
                {
                    runs = d.Fetch<Run>(string.Format(_byProduct, _current, _latest, _where), productcodeid, asof);
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
                    runs = d.Fetch<Run>(string.Format(_byProduct, "", "", ""), productcodeid);
                }
                catch (Exception e)
                {
                    Elmah.ErrorSignal.FromCurrentContext().Raise(new Exception("Error finding all runs ", e));
                }
            }
            return runs;
        }
    }
}