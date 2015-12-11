using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Test.Models
{
    public partial class System
    {
        public int SolutionBatchId { get; set; }
        public int SolutionRecipeId { get; set; }
        public string SolutionType { get; set; }

        public static string Current = @"
            select s.systemid
                ,s.status
                ,s.system
                ,b.solutionbatchid
                ,b.solutionrecipeid
                ,r.solutiontype
            from system s
            join solutionbatch b on b.systemid = s.systemid
            join solutionrecipe r on b.solutionrecipeid = r.solutionrecipeid
        ";

        public static string Active = @"
            select distinct s.systemid
                ,s.status
                ,s.system
            from system s
            join linestate l on l.systemid = s.systemid
        ";
    }
}