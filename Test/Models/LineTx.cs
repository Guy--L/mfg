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
                c.[LineTxId]
                ,c.[LineId]
                ,c.[PersonId]
                ,c.[Stamp]
                ,c.[Comment]
                ,c.[LineTankId]
                ,c.[UnitId]
                ,c.[LineNumber]
                ,c.[SystemId]
                ,c.[StatusId]
                ,c.[ProductCodeId]
                ,s.[SystemId]
                ,s.[System]
                ,p.[ProductCodeId]
                ,p.[ProductCode]
                ,p.[ProductSpec]
                ,p.[PlastSpec]
                ,v.[ConversionId]
                ,v.[SolutionRecipeId]
                ,v.[ExtruderId]
                ,v.[FinishFootage]
                ,v.[EndStateId]
                ,v.[ConversionState]
            from linetx c
            join productcode p on p.productcodeid = c.productcodeid
            join system s on s.systemid = c.systemid
            left join conversion v on v.linetxid = c.linetxid
            where c.lineid = @0 {1} order by c.stamp desc 
        ";

        public Status status { get; set; }
        public System system { get; set; }
        public ProductCode product { get; set; }
        public Conversion conversion { get; set; }

        public static LineTx Map(LineTx l, System y, ProductCode p, Conversion c)
        {
            l.status = Status.state[l.StatusId];
            l.system = y;
            l.product = p ?? new ProductCode() { _ProductCode = "00?00", ProductCodeId = 0 };
            l.ProductCodeId = l.product.ProductCodeId;
            l.conversion = c;
            return l;
        }

        public string Action { get; set; }

        public string Name
        {
            get { return letters[UnitId] + LineNumber.ToString(); }
        }

        public static string contextByLine(string when)
        {
            var clause = " and c.stamp <= '" + when + "' ";
            return string.Format(_byLine, " top 1 ", clause);
        }

        private static string priorByLine(int horizon)
        {
            var top = horizon > 0 ? ("top " + horizon) : "";
            return string.Format(_byLine, top, "");
        }

        public static int LatestTx(int lineid)
        {
            LineTx ltx = null;
            using (labDB db = new labDB())
            {
                ltx = db.Fetch<LineTx>(priorByLine(1), lineid).SingleOrDefault();
            }
            if (ltx == null) return 0;
            return ltx.LineTxId;
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
            List<LineTx> tx = null;

            using (labDB db = new labDB())
            {
                tx = db.Fetch<LineTx, System, ProductCode, Conversion, LineTx>(Map, priorByLine(10), id);
            }
            //if (pending.Any())
            //    pending.Last().Action = (new Conversion(pending.Last())).Action;
            return tx.ToList();
        }
    }
}