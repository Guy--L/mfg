using System;
using System.Collections.Generic;
using NPoco;
using System.Linq;

namespace Tags.Models
{
    public class Run
    {
        private static string _byLines = @";
            with 
			lines as (
				select lineid from unit u
					join line l on l.unitid = u.unitid
					where u.unit+convert(char,l.linenumber,0) in (@0)
					),
			 cut as (
	            select linetxid, lineid, stamp, productcodeid, statusid, endstamp from (
		            select distinct linetxid, lineid, stamp, productcodeid, statusid,
			            lead(stamp, 1, getdate()) over (partition by lineid order by stamp) as endstamp
		            from linetx
	            ) a
            )
            select distinct
                  n.linetxid
	            , n.lineid
	            , n.stamp
	            , n.endstamp
                , n.productcodeid
            from cut n
			join lines a on a.lineid = n.lineid
            join line l on l.lineid = n.lineid
            join unit u on u.unitid = l.unitid
            join productcode p on p.ProductCodeId = n.ProductCodeId
            join [status] s on s.StatusId = n.StatusId
            where n.stamp >= @1 and n.stamp <= @2
			or n.endstamp >= @1 and n.endstamp <= @2
			or n.stamp <= @1 and @1 <= n.endstamp
            and s.Code = 'RP'
            order by n.lineid, n.stamp
        ";

        public int LineId { get; set; }
        public int LineTxId { get; set; }
        public DateTime Stamp { get; set; }
        public DateTime EndStamp { get; set; }
        public string ProductCode { get; set; }
        public string ProductSpec { get; set; }
        public int ProductCodeId { get; set; }
        public int ProductWidth { get; set; }
        public string GelType { get; set; }
        public string Comment { get; set; }

        public long start { get { return Stamp.ToJSMSecs(); } }
        public long stop { get { return EndStamp.ToJSMSecs(); } }


        public Run() { }

        public static ILookup<int, Run> RunsByLine(string[] channels, DateTime start, DateTime end)
        {
            var include = "'" + string.Join("','", channels) + "'";
            ILookup<int, Run> runs = null;
            using (labDB d = new labDB())
            {
                try
                {
                    runs = d.Fetch<Run>(_byLines, include, start, end).ToLookup(k => k.LineId);
                }
                catch (Exception e)
                {
                    Elmah.ErrorSignal.FromCurrentContext().Raise(new Exception("Error finding runs by "+include+" ["+start.ToShortDateString()+"-"+end.ToShortDateString()+"]", e));
                }
            }
            return runs;
        }
    }
}