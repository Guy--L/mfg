using System;
using System.Collections.Generic;
using System.Linq;
using SysWeb = System.Web;
using NPoco;
using Test.Properties;
using System.Threading.Tasks;

namespace Test.Models
{
    public class BatchSpan
    {
        private static string _spans = @";with t as (
            select x.solutionbatchid, x.[datetime]
				, x.Glycerin 
				, x.CMC
				, x.Conductivity
				, x.CasingGlycerin
			from solutiontest x where x.[datetime] >= dateadd(day, -@0, getdate())
            )
            select t.solutionbatchid batchid, r.SolutionType recipe, min(t.[datetime]) [begin], max(t.[datetime]) [end]
				, avg(t.Glycerin)		Glycerin
				, avg(t.CMC)			CMC
				, avg(t.Conductivity)	Conductivity
				, avg(t.CasingGlycerin)	CasingGlycerin
            from t
            left join solutionbatch b on b.solutionbatchid = t.solutionbatchid
            left join solutionrecipe r on r.SolutionRecipeId = b.SolutionRecipeId
            group by t.solutionbatchid, b.systemid, r.SolutionType
            having b.systemid = @1
        ";

        public int BatchId { get; set; }
        public DateTime Begin { get; set; }
        public DateTime End { get; set; }
        public Decimal CMC { get; set; }
        public Decimal Glycerin { get; set; }
        public Decimal CasingGlycerin { get; set; }
        public Decimal Conductivity { get; set; }
        public string Recipe { get; set; }

        public static List<BatchSpan> Since(int system, int days)
        {
            List<BatchSpan> stripes = null;
            using (labDB db = new labDB())
            {
                stripes = db.Query<BatchSpan>(_spans, days, system).ToList();
            }
            return stripes;
        }
    }

    public class SolutionTests 
    {
        public int BatchId { get; set; }
        public string System { get; set; }
        public string Recipe { get; set; }
        public bool isGr { get { return System.StartsWith("gr"); } }
        public List<SolutionTest> list { get; set; }
    }

    public class SolutionTestView 
    {
        public static int lastshift;
        public SolutionTest t { get; set; }
        public string Recipe { get; set; }
        public string System { get; set; }
        public bool isGr { get { return System.StartsWith("gr"); } }

        public SolutionTestView() { }

        public SolutionTestView(int tid)
        {
            using (labDB db = new labDB())
            {
                SolutionBatch batch;

                if (tid < 0)
                {
                    batch = db.Single<SolutionBatch>(Resources.SolutionBatchById, -tid);
                    t = new SolutionTest()
                    {
                        SolutionTestId = 0,
                        SolutionBatchId = -tid,
                        DateTime = DateTime.Now,
                        ReadingNumber = lastshift + 1
                    };
                }
                else
                {
                    t = db.SingleOrDefault<SolutionTest>(" where SolutionTestId = @0", tid);
                    batch = db.Single<SolutionBatch>(Resources.SolutionBatchById, t.SolutionBatchId);
                }
                Recipe = batch.SolutionType;
                System = batch.System;
                t.System = System;
                t.SystemId = batch.SystemId;
            }
        }

        public void Save()
        {
            var IsNew = t.SolutionTestId == 0;
            t.Save();
            if (IsNew)
            {
                SysWeb.Hosting.HostingEnvironment.QueueBackgroundWorkItem(async (_) =>
                {
                    await Task.Run(() => { SolutionChart.UpdateDeck(t.SystemId); });
                });
            }
        }
    }

    public partial class SolutionTest
    {
        private static string _bydate = @"
            SELECT  t.[SolutionTestId]
                  , t.[SolutionBatchId]
                  , t.[DateTime]
                  , t.[SolutionRecipeId]
                  , t.[CMC]
                  , t.[DensitySetPoint]
                  , t.[ConsoleDensity]
                  , t.[pHSetPoint]
                  , t.[Viscoscity]
                  , t.[Temperature]
                  , t.[TitrationMLs]
                  , t.[NaOCl Pump Set]
                  , t.[NaOCl Flow]
                  , row_number() over (partition by cast(t.[DateTime] as date) order by t.[DateTime]) ReadingNumber
                  , t.[MeasuredDensity]
                  , t.[ConsolepH]
                  , t.[MeasuredpH]
	              , t.[Feed]
	              , t.[Steam]
                  , t.[Conductivity]
                  , t.[Acid Pump Output]
                  , t.[Booster Pump Output]
                  , t.[Glycerin]
	              , t.[Hypochlorite]
	              , t.[CasingGlycerin]
	              , b.[datetime] batchstamp
                  , b.systemid
	              , s.[system]
                  , r.solutiontype
                from solutiontest t
                inner join solutionbatch b on b.solutionbatchid = t.solutionbatchid
                inner join solutionrecipe r on b.solutionrecipeid = r.solutionrecipeid
                inner join [system] s on b.systemid = s.systemid
                WHERE b.systemid = @0 and t.[DateTime] >= dateadd(day, -@1, getdate())
        ";

        [ResultColumn] public int ReadingNumber { get; set; }
        [ResultColumn] public string SolutionType { get; set; }
        [ResultColumn] public int SystemId { get; set; }
        [ResultColumn] public string System { get; set; }
        public bool isGr { get { return System.StartsWith("gr"); } }

        public static List<SolutionTest> Recent(int system, int days)
        {
            using (labDB d = new labDB())
            {
                return d.Fetch<SolutionTest>(_bydate, system, days);
            }
        }
    }
}