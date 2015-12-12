using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using NPoco;

namespace Test.Models
{
    public class SolutionBatches
    {
        public List<SolutionBatch> list { get; set; }

        public SolutionBatches()
        {
            using (labDB db = new labDB())
            {
                list = db.Fetch<SolutionBatch>(SolutionBatch._all + " order by s.[System] asc");
            }
        }
    }
    
    public partial class SolutionBatch
    {
        public static string _all = @"
            SELECT b.[SolutionBatchId]
                    ,b.[DateTime]
                    ,b.[SolutionRecipeId]
                    ,b.[OperatorId]
                    ,b.[CoA]
                    ,s.[SystemId]
                    ,s.[System]
		            ,b.[Completed]
                    ,r.[SolutionType]
                    ,ROW_NUMBER() OVER(PARTITION BY b.[SystemId] ORDER BY b.[DateTime] DESC) AS Row
		            ,(select count(t.solutiontestid) from [dbo].[SolutionTest] t where t.SolutionBatchId = b.SolutionBatchId) as TestCount
                FROM [dbo].[SolutionBatch] b
                join [dbo].[SolutionRecipe] r on b.[SolutionRecipeId] = r.[SolutionRecipeId]
	            join [dbo].[System] s on b.[SystemId] = s.[Systemid]
        ";

        [ResultColumn] public string SolutionType { get; set; }
        [ResultColumn] public int Row { get; set; }
        [ResultColumn] public int TestCount { get; set; }
        [ResultColumn] public string System { get; set; }

        public bool Active { get { return Row == 1 && !Completed.HasValue; } }
    }

    public class SolutionBatchView
    {
        public SolutionBatch b { get; set; }
        public SelectList recipes;
        public int? SysId { get; set; }
        public List<System> systems;

        public SolutionBatchView()
        { }

        public SolutionBatchView(int id)
        {
            using (labDB db = new labDB()){
                systems = db.Fetch<System>(" where rtrim(status) = 'Good'");
                if (id > 0)
                {
                    b = db.SingleOrDefaultById<SolutionBatch>(id);
                    SysId = b.SystemId;
                }
                else
                {
                    b = new SolutionBatch()
                    {
                        CoA = "CoA Stub",
                        DateTime = DateTime.Now,
                        SolutionRecipeId = 0
                    };
                    SysId = 0;
                }
                recipes = AddNone(new SelectList(db.Fetch<SolutionRecipe>(""), "SolutionRecipeId", "SolutionType", b.SolutionRecipeId));
            }
        }

        private SelectList AddNone(SelectList list)
        {
            List<SelectListItem> _list = list.ToList();
            _list.Insert(0, new SelectListItem() { Value = "0", Text = "" });
            return new SelectList((IEnumerable<SelectListItem>)_list, "Value", "Text");
        }
    }
}