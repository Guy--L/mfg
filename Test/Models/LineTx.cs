using System;
using System.Collections.Generic;
using System.Linq;
using NPoco;

namespace Test.Models
{
    public partial class LineTx
    {
        private static string letters = " ABCDEFGHIJ";  // units start at 1

        private static string _byLine = @"
            select {0}
                c.[LineTxId],
                c.[LineId],
                c.[PersonId],
                c.[Stamp],
                c.[Comment],
                c.[LineTankId],
                c.[UnitId],
                c.[LineNumber],
                c.[SystemId],
                c.[StatusId],
                c.[ProductCodeId],
                c.[ConversionId]
                  ,y.[StatusId]
                  ,y.[Code]
                  ,y.[Icon]
                  ,y.[Color]
                  ,s.[SystemId]
                  ,s.[System]
                  ,p.[ProductCodeId]
                  ,p.[ProductCode]
                  ,p.[ProductSpec]
                  ,p.[PlastSpec]
            from linetx c
            join productcode p on p.productcodeid = c.productcodeid
            join system s on s.systemid = c.systemid
            join status y on y.statusid = c.statusid
            where c.lineid = @0 {1} order by c.stamp desc
        ";

        private static string _pendingByLine = @"
            select
                c.[LineId],
                c.[Scheduled],
                c.[Completed],
                c.[Note],
                c.[SystemId],
                c.[StatusId],
                c.[ProductCodeId],
                c.[ConversionId],
                c.[SolutionRecipeId]
                  ,y.[StatusId]
                  ,y.[Code]
                  ,y.[Icon]
                  ,y.[Color]
                  ,s.[SystemId]
                  ,s.[System]
                  ,p.[ProductCodeId]
                  ,p.[ProductCode]
                  ,p.[ProductSpec]
                  ,p.[PlastSpec]
            from conversion c
            join productcode p on p.productcodeid = c.productcodeid
            join system s on s.systemid = c.systemid
            join status y on y.statusid = c.statusid
            where c.lineid = @0 order by dbo.SinceNow(c.[Started], c.[Completed])
        ";

        public Status status { get; set; }
        public System system { get; set; }
        public ProductCode product { get; set; }

        public static LineTx Map(LineTx l, Status s, System y, ProductCode p)
        {
            l.status = s;
            l.system = y;
            l.product = p ?? new ProductCode() { _ProductCode = "00?00", ProductCodeId = 0 };
            l.ProductCodeId = l.product.ProductCodeId;
            l.IsConversion = l.SolutionRecipeId!=null;
            if (l.IsConversion)
                l.Stamp = l.Completed < DateTime.Now ? l.Completed : l.Scheduled;
            return l;
        }

        // Columns in Conversion table not in Line(Tx) table
        [ResultColumn] public DateTime Scheduled { get; set; }
        [ResultColumn] public DateTime Completed { get; set; }
        [ResultColumn] public string Note { get; set; }
        [ResultColumn] public int? SolutionRecipeId { get; set; }

        public bool IsConversion { get; set; }
        public string Action { get; set; }

        public string Name
        {
            get { return letters[UnitId] + LineNumber.ToString(); }
        }

        public static string contextByLine(string when)
        {
            var clause = " and stamp <= '" + when + "' ";
            return string.Format(_byLine, " top 1 ", clause);
        }

        private static string priorByLine(int horizon)
        {
            var top = horizon > 0 ? ("top " + horizon) : "";
            return string.Format(_byLine, top, "");
        }

        public static Line Prior(int id)
        {
            List<Line> pair = null;
            using (labDB db = new labDB())
            {
                pair = db.Fetch<Line>(priorByLine(2), id);
            }
            return pair.Any()? pair.Last(): null; 
        }
        
        /// <summary>
        /// Provide a combined timeline of conversions pending and line transactions
        /// </summary>
        /// <param name="id">Line Id</param>
        /// <returns>List of line transactions since conversions look like transactions</returns>
        public static List<LineTx> TimeLine(int id)
        {
            List<LineTx> past = null;
            List<LineTx> future = null;

            using (labDB db = new labDB())
            {
                past = db.Fetch<LineTx, Status, System, ProductCode, LineTx>(Map, priorByLine(10), id);
                future = db.Fetch<LineTx, Status, System, ProductCode, LineTx>(Map, _pendingByLine, id);
            }
            var latest = past.Any()?past.Max(p => p.Stamp):(DateTime.Now.AddYears(-200));
            var pending = future.Where(f => f.Completed > latest);
            if (pending.Any())
                pending.Last().Action = (new Conversion(pending.Last())).Action;
            var timeline = pending.Concat(past);
            return timeline.ToList();
        }
    }
}