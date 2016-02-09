using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using NPoco;

namespace Test.Models
{
    public partial class System
    {
        [Ignore] public static Dictionary<int, string> Systems;

        public static string _active = @"select distinct q.systemid, q.[system], q.solutiontype from (" +
                    SolutionBatch._all + 
                    " where b.completed is null and [system] not like 'gr%') q where q.row = 1 order by q.systemid";

        public static string _attime = @"select distinct q.systemid, q.[system], q.solutiontype, q.[datetime], q.completed from (" +
                    SolutionBatch._all +
                    " where b.[datetime] <= '{0}' and [system] not like 'gr%') q where q.row = 1 order by q.systemid";

        public int SolutionBatchId { get; set; }
        public int SolutionRecipeId { get; set; }
        [ResultColumn] public string SolutionType { get; set; }
        [ResultColumn] public DateTime DateTime { get; set; }
        [ResultColumn] public DateTime Completed { get; set; }

        public string Pretty { get { return _System + " " + SolutionType; } }
    }
}