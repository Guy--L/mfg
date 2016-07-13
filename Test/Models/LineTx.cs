using System;
using System.Collections.Generic;
using System.Linq;
using NPoco;

namespace Test.Models
{
    public partial class LineTx
    {
        private static string letters = " ABCDEFGHIJ";  // units start at 1

        private static string _byProductTime = @"
            ;with cut as (
	            select lineid, stamp, productcodeid, statusid, endstamp from (
		            select distinct lineid, stamp, productcodeid, statusid, 
			            lead(stamp, 1, getdate()) over (partition by lineid order by stamp) as endstamp
		            from linetx
	            ) a
	            where a.stamp < @0
            )
            select
                 n.[LineTxId]
                ,n.[LineId]
                ,n.[PersonId]
                ,n.[Stamp]
                ,n.[EndStamp]
                ,n.[Comment]
                ,n.[LineTankId]
                ,n.[UnitId]
                ,n.[LineNumber]
                ,n.[SystemId]
                ,n.[StatusId]
                ,n.[ProductCodeId]
                ,x.[SampleId]       samples__SampleId
            from cut n
            left join cut m on n.lineid = m.lineid and m.stamp > n.stamp
            join line l on l.lineid = n.lineid
            join unit u on u.unitid = l.unitid
            join productcode p on p.ProductCodeId = n.ProductCodeId
            join [status] s on s.StatusId = n.StatusId
            left join [Sample] x on x.lineid = n.lineid and x.stamp >= n.stamp and x.stamp <= n.endstamp
            where m.stamp is null and s.Code = 'RP' and n.productcodeid = @1
        ";

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
                ,x.[StatusId]           status__StatusId 
                ,x.[Code]               status__Code  
                ,x.[Description]        status__Description
                ,x.[Icon]               status__Icon      
                ,x.[Color]              status__Color           
                ,s.[SystemId]           system__SystemId
                ,s.[System]             system__System
                ,p.[ProductCodeId]      product__ProductCodeId   
                ,p.[ProductCode]        product__ProductCode   
                ,p.[ProductSpec]        product__ProductSpec
                ,p.[PlastSpec]          product__PlastSpec   
                ,v.[ConversionId]       conversion__ConversionId  
                ,v.[SolutionRecipeId]   conversion__SolutionRecipeId
                ,v.[ExtruderId]         conversion__ExtruderId  
                ,v.[FinishFootage]      conversion__FinishFootage 
                ,v.[EndStateId]         conversion__EndStateId   
                ,v.[ConversionState]    conversion__ConversionState
            from linetx c
            join status x on x.statusid = c.statusid
            join productcode p on p.productcodeid = c.productcodeid
            join system s on s.systemid = c.systemid
            left join conversion v on v.linetxid = c.linetxid
            where c.lineid = @0 {1} order by c.stamp desc 
        ";

        [ResultColumn, ComplexMapping] public Status status { get; set; }
        [ResultColumn, ComplexMapping] public System system { get; set; }
        [ResultColumn, ComplexMapping] public ProductCode product { get; set; }
        [ResultColumn, ComplexMapping] public Conversion conversion { get; set; }

        [ResultColumn] public DateTime EndStamp { get; set; }

        [ResultColumn] public CasingSample samples { get; set; }

        //public static LineTx Map(LineTx l, System y, ProductCode p, Conversion c)
        //{
        //    l.status = Status.state[l.StatusId];
        //    l.system = y;
        //    l.product = p ?? new ProductCode() { _ProductCode = "00?00", ProductCodeId = 0 };
        //    l.ProductCodeId = l.product.ProductCodeId;
        //    l.conversion = c;
        //    return l;
        //}

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
                tx = db.Fetch<LineTx>(priorByLine(10), id);
            }
            //if (pending.Any())
            //    pending.Last().Action = (new Conversion(pending.Last())).Action;
            return tx.ToList();
        }
    }
}