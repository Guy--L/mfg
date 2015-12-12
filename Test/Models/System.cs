using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using NPoco;

namespace Test.Models
{
    public partial class System
    {
        public static string _active = @"select distinct q.systemid, q.[system], q.solutiontype from (" +
                    SolutionBatch._all + 
                    " where b.completed is null and [system] not like 'gr%') q where q.row = 1 order by q.systemid";

        public int SolutionBatchId { get; set; }
        public int SolutionRecipeId { get; set; }
        [ResultColumn] public string SolutionType { get; set; }

        public string Pretty { get { return _System + " " + SolutionType; } }
    }
}