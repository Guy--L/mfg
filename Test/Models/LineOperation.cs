using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using NPoco;

namespace Test.Models
{
    public partial class LineOperation
    {
        public static string _all = @"
            SELECT [INDAY]
                  ,[INUNIT]
                  ,[INLINE]
                  ,[INSHFT]
                  ,[STCODE]
                  ,[INTIME]
                  ,[RSCODE]
                  ,[INPRD]
                  ,dateadd(minute, intime%100, dateadd(hour, cast(intime/100 as int),
                    dateadd(day, inday%1000-1, dateadd(year, cast(inday/1000 as int), datetimefromparts(1990,1,1,0,0,0,0)))
                   )) as stamp
              FROM devusa.devusa.ncifiles#.indtp700
        ";

        public static string _status = @"select t.* from LineOperation t
            left outer join LineOperation u on (t.LineId = u.LineId and t.stamp < u.stamp)
            where u.LineId is null
            order by t.LineId";
    }

    public class LineConversion
    {
        private static Dictionary<int, LineConversion> _conversions = null;

        public int lineid { get; set; }
        public int productcodeid { get; set; }
        public DateTime stamp { get; set; }
        public string productcode { get; set; }
        public string productspec { get; set; }
        public string stcode { get; set; }
        public int currentid { get; set; }
        public ProductCode prior { get; set; }

        public string Active { get { return stcode == "U" ? "up" : "down"; } }
        public string Title { get { return productspec + " " + stamp.ToString("MM/dd hh:mm"); } }

        public LineConversion() { }

        public static string _last2 = @";WITH
            LnStatus (lineid, stamp, productcodeid, stcode, Ord) as (
             SELECT lineid, stamp, productcodeid, stcode,
                     ROW_NUMBER() OVER (PARTITION BY lineid ORDER BY stamp DESC)
             FROM LineOperation
            )
            SELECT a.lineid, b.stamp, b.productcodeid, b.stcode, a.productcodeid currentid, q.productcode, q.productspec
                  ,q.[Oil_Aim]
                  ,q.[Oil_Min]
                  ,q.[Oil_Max]
                  ,q.[Gly_Aim]
                  ,q.[Gly_Min]
                  ,q.[Gly_Max]
                  ,q.[ReelMoist_Aim]
                  ,q.[ReelMoist_Min]
                  ,q.[ReelMoist_Max]
            from LnStatus a
            join LnStatus b on a.lineid = b.lineid and a.ord = 1 and b.ord = 2 and a.productcodeid != b.productcodeid
            join ProductCode q on q.productcodeid = b.productcodeid
        ";

        private static string _outside = @"<td class='Sample product line{0}' title='{1}' data-moist='{3}' data-glyc='{4}'>{2}</td>";
        private static string _nobutton = @"<i class='fa fa-arrow-circle-{0}'></i>{1}";
        private static string _button = @"<button id='{4}_{1}' class='btn sampled btn-small line{0}' data-pid='{1}' title='{2} Click for Conversion' {5} data-moist='{6}' data-glyc='{7}'>{3}</button>";

        public static void RefreshConversions()
        {
            using (labDB db = new labDB())
            {
                _conversions = db.Fetch<LineConversion>(_last2).ToDictionary(k => k.lineid, v => v);
            }
        }

        public static LineStatus Format(LineStatus n)
        {
            var text = string.Format(_nobutton, n.Active, n.INPRD);
            if (!_conversions.ContainsKey(n.LineId)) {
                n.markup = new HtmlString(string.Format(_outside, n.Active, n.Title, text, n.current.MoistSpec, n.current.GlySpec));
                return n;
            }
            LineConversion c = _conversions[n.LineId];
            var priortext = string.Format(_nobutton, c.Active, c.productcode);

            var current = string.Format(_button
                , n.Active
                , n.ProductCodeId
                , n.Title
                , text
                , n.LineId
                , ""
                , n.current.MoistSpec
                , n.current.GlySpec);
            var prior = string.Format(_button
                , c.Active
                , c.productcodeid
                , c.Title
                , priortext
                , c.lineid
                , "style='display: none;'"
                , c.prior.MoistSpec
                , c.prior.GlySpec);

            n.markup = new HtmlString(string.Format(_outside, n.Active, n.Title, current+prior, "", ""));
            return n;
        }
    }

    public partial class LineStatus
    {
        public static string _all = @"
            SELECT s.[INUNT]
                  ,s.[INLIN]
                  ,s.[INDAY]
                  ,s.[INPRD]
                  ,s.[CARTN]
                  ,s.[INSID]
                  ,s.[INLSQ]
                  ,s.[INLST]
                  ,s.[INREL]
                  ,s.[INBSP]
                  ,s.[INSAM]
                  ,s.[LineId]
                  ,s.[Status]
                  ,s.[Reason]
                  ,s.[Stamp]
                  ,s.[ProductCodeId]
                  ,p.[Oil_Aim]
                  ,p.[Oil_Min]
                  ,p.[Oil_Max]
                  ,p.[Gly_Aim]
                  ,p.[Gly_Min]
                  ,p.[Gly_Max]
                  ,p.[ReelMoist_Aim]
                  ,p.[ReelMoist_Min]
                  ,p.[ReelMoist_Max]
            FROM [dbo].[LineStatus] s
            join ProductCode p on s.ProductCodeId = p.ProductCodeId
        ";

        public HtmlString markup { get; set; }
        public string Active { get { return Status == "U" ? "up" : "down"; } }
        public string Title { get { return INSID + " " + Stamp.ToString("MM/dd hh:mm"); } }

        public string MoistSpec { get { return current.MoistSpec; } }
        public string GlySpec { get { return current.GlySpec; } }

        public ProductCode current { get; set; }

        public static List<LineStatus> lineStatus;
        private static DateTime latestStatus = DateTime.MinValue;
        public static void Initialize()
        {
            LineConversion.RefreshConversions();
            using (labDB d = new labDB())
            {
                d.OneTimeCommandTimeout = 2000;
                lineStatus = d.Fetch<LineStatus>(_all).Select(s => LineConversion.Format(s)).ToList();
            }
            
            latestStatus = lineStatus.Max(t => t.Stamp);
        }

        public static List<int> Refresh()
        {
            LineConversion.RefreshConversions();
            List<LineStatus> delta = new List<LineStatus>();
            using (labDB d = new labDB())
            {
                delta = d.Fetch<LineStatus>(_all+" where Stamp > @0", latestStatus).Select(s => LineConversion.Format(s)).ToList();
            }
            if (!delta.Any())
                return null;
            List<int> dirty = new List<int>();
            delta.ForEach(d =>
            {
                latestStatus = latestStatus < d.Stamp ? d.Stamp : latestStatus;
                var j = lineStatus.FindIndex(n => n.LineId == d.LineId);
                dirty.Add(j);
                lineStatus[j] = d;
            });
            return dirty;
        }
    }
}