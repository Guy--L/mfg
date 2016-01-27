using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Test.Models
{
    public partial class LineTx
    {
        private static string _priorbyline = @"
            select top 2 
                [LineTxId],
                [LineId],
                [PersonId],
                [Stamp],
                [Comment],
                [LineTankId],
                [UnitId],
                [LineNumber],
                [SystemId],
                [StatusId],
                [ProductCodeId]
            from linetx where lineid = @0 order by stamp desc
        ";

        public static Line Prior(int id)
        {
            List<Line> pair = null;
            using (labDB db = new labDB())
            {
                pair = db.Fetch<Line>(_priorbyline, id);
            }
            return pair.Any()? pair.Last(): null; 
        }
    }
}