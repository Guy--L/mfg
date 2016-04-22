using System;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Test.Models;
using Test.Properties;

namespace Test.Controllers
{
    public class HomeController : BaseController
    {
        /*
        public ActionResult TensileSamples()
        {
            TensileSamples t = new TensileSamples();
            using (labDB db = new labDB())
            {
                t.list = db.Fetch<TensileSample>(Resources.TensileSamples);
            }
            return View(t);
        }

        public ActionResult TSample(int id)
        {
            TensileSampleView s = new TensileSampleView(id);
            return View(s);
        }

        public ActionResult StopTensile(int id)
        {
            var b = TensileSample.Single(id);
            b.Completed = DateTime.Now;
            b.Update();

            return RedirectToAction("TensileSamples");
        }

        [HttpPost]
        public ActionResult SaveTSample(TensileSampleView sbv)
        {
            sbv.t.LineId = sbv.LineId.Value;
            if (sbv.t.Note == null) sbv.t.Note = "";
            if (string.IsNullOrWhiteSpace(sbv.t.Tech)) sbv.t.Tech = "";
            sbv.t.Save();
            return RedirectToAction("TensileSamples");
        }

        public ActionResult TensileTests(int id)
        {
            var t = new TensileTests(id);
            TempData["TensileSample"] = t.sample;
            return View(t);
        }

        public ActionResult TTest(int id)
        {
            TensileSample s = TempData["TensileSample"] as TensileSample;
            if (s == null)
            {
                using (labDB db = new labDB()) {
                    s = db.SingleOrDefault<TensileSample>(string.Format(Resources.TensileSampleById, (id < 0) ? -id : id));
                }
            }
            TensileTestView v = new TensileTestView(id);
            v.sample = s;
            v.t.TensileSampleId = s.TensileSampleId;
            return View(v);
        }

        [HttpPost]
        public ActionResult SaveTTest(TensileTestView ttv)
        {
            ttv.t.Save();
            return RedirectToAction("TensileTests", new { id = ttv.sample.TensileSampleId });
        }
        */

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult SolutionBatches()
        {
            SolutionBatches s = new SolutionBatches();
            return View(s);
        }

        public ActionResult SBatch(int id)
        {
            SolutionBatchView s = new SolutionBatchView(id);
            return View(s);
        }

        public ActionResult StopBatch(int id)
        {
            var b = SolutionBatch.Single(id);
            b.Completed = DateTime.Now;
            b.Update();

            return RedirectToAction("SolutionBatches");
        }

        [HttpPost]
        public ActionResult SaveSBatch(SolutionBatchView sbv)
        {
            sbv.b.SystemId = sbv.SysId.Value;
            sbv.b.Save();
            return RedirectToAction("SolutionBatches");
        }

        public ActionResult SolutionTests(int id)
        {
            SolutionTests t = new SolutionTests();
            using (labDB db = new labDB())
            {
                t.list = db.Fetch<SolutionTest>(Resources.SolutionTestByBatch, id);
                var top = t.list.FirstOrDefault();
                SolutionTestView.lastshift = top == null ? 0 : top.ReadingNumber;
                if (top == null)
                {
                    var batch = db.Single<SolutionBatch>(Resources.SolutionBatchById, id);
                    t.Recipe = batch.SolutionType;
                    t.System = batch.System;
                }
                else
                {
                    t.Recipe = top.SolutionType;
                    t.System = top.System;
                }
                t.BatchId = id;
            }
            return View(t);
        }

        public ActionResult SolutionChart(int id, int group)
        {
            SolutionTests t = new SolutionTests();

            using (labDB db = new labDB())
                t.list = db.Fetch<SolutionTest>(Resources.SolutionTestByBatch, id);

            t.Recipe = t.list.First().SolutionType;
            t.System = t.list.First().System;

            return View(new SolutionChart(t, group));
        }

        public ActionResult PNGCharts()
        {
            return View();
        }

        public ActionResult STestNew(int bid)
        {
            SolutionTestView t = new SolutionTestView(bid);
            return View(t);
        }

        public ActionResult STest(int id)
        {
            SolutionTestView t = new SolutionTestView(id);
            return View(t);
        }

        [HttpPost]
        public ActionResult SaveSTest(SolutionTestView tv)
        {
            tv.Save();
            return RedirectToAction("SolutionTests", new { id = tv.t.SolutionBatchId });
        }

        public ActionResult CasingTests(int id)
        {
            CasingTests c = new CasingTests(id);
            return View(c);
        }

        public ActionResult CTestNew()
        {
            CasingTestView c = new CasingTestView();
            return View(c);
        }

        public ActionResult CTest(int id)
        {
            CasingTestView c = new CasingTestView(id);
            return View(c);
        }

        [HttpPost]
        public ActionResult SaveCTest(CasingTestView cv)
        {
            cv.Save();
            return RedirectToAction("CasingTests");
        }

        public ActionResult Units()
        {
            using (labDB db = new labDB())
            {
                var units = db.Fetch<Unit>();
                return View(units.ToList());
            }
        }

        public ActionResult Snapshot()
        {
            int[] systms = new int[] { 1, 2, 4, 5, 6, 7, 8, 9, 10, 11, 12 };
            foreach (var i in systms)
                Models.SolutionChart.UpdateDeck(i);
            return RedirectToAction("Units");
        }

        public ActionResult Show()
        {
            return View();
        }

        public ActionResult CasingSamples()
        {
            CasingSamples s = new CasingSamples();

            return View(s);
        }

        public ActionResult CasingSample(int id)
        {
            CasingSamplesView s = new CasingSamplesView(id);
            TempData["CasingSampleView"] = s;
            return View(s);
        }

        public ActionResult LabCasingResults(int id)
        {
            CasingSamplesView s = new CasingSamplesView(id);
            s.Seal();
            return RedirectToAction("CasingSample", new { id = id });
        }

        public ActionResult ExportCasingSample(int? id)
        {
            CasingSamplesView csv = id.HasValue ? new CasingSamplesView(id.Value) : (TempData["CasingSampleView"] as CasingSamplesView);
            string file = csv.Export(Server.MapPath(@"~/Content/Casing.xls"));
            return File(file, "application/vnd.ms-excel", Path.GetFileName(file));
        }

        public ActionResult CasingLoaded()
        {
            ViewBag.SampleCount = TempData["count"] as int?;
            return View();
        }

        [HttpPost]
        public ActionResult UploadCasing(HttpPostedFileBase file)
        {
            var load = Models.CasingSamples.ReadExcel(file.InputStream, false);

            Success(load.Item1 + " casing sample records loaded for " + load.Item2);
            return RedirectToAction("CasingSamples");
        }

        [HttpPost]
        public ActionResult UploadCasings(HttpPostedFileBase file)
        {
            var load = Models.CasingSamples.ReadExcel(file.InputStream, true);

            return Content(load.Item2.Value.ToString("MM/dd HH:mm") + "\t" + load.Item1);
        }
    }
}