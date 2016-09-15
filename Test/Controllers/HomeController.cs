using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ClosedXML.Excel;
using Test.Models;
using Test.Properties;

namespace Test.Controllers
{
    [Authorize]
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
            var top = Session["Context"] as Context;
            if (top == null)
                top = new Context();

            return View(top);
        }

        [AllowAnonymous]
        public ActionResult SolutionBatches()
        {
            SolutionBatches s = new SolutionBatches();
            return View(s);
        }

        [AllowAnonymous]
        public ActionResult SBatch(int id)
        {
            SolutionBatchView s = new SolutionBatchView(id);
            return View(s);
        }

        [AllowAnonymous]
        public ActionResult StopBatch(int id)
        {
            var b = SolutionBatch.Single(id);
            b.Completed = DateTime.Now;
            b.Update();

            return RedirectToAction("SolutionBatches");
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult SaveSBatch(SolutionBatchView sbv)
        {
            sbv.b.SystemId = sbv.SysId.Value;
            sbv.b.Save();
            return RedirectToAction("SolutionBatches");
        }

        [AllowAnonymous]
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

        [AllowAnonymous]
        public ActionResult SolutionChart(int id, int group)
        {
            SolutionTests t = new SolutionTests();

            using (labDB db = new labDB())
                t.list = db.Fetch<SolutionTest>(Resources.SolutionTestByBatch, id);

            t.Recipe = t.list.First().SolutionType;
            t.System = t.list.First().System;

            return View(new SolutionChart(t, group));
        }

        [AllowAnonymous]
        public ActionResult PNGCharts()
        {
            return View();
        }

        [AllowAnonymous]
        public ActionResult ComboCNC()
        {
            return View();
        }

        [AllowAnonymous]
        public ActionResult STestNew(int bid)
        {
            SolutionTestView t = new SolutionTestView(bid);
            return View(t);
        }

        [AllowAnonymous]
        public ActionResult STest(int id)
        {
            SolutionTestView t = new SolutionTestView(id);
            return View(t);
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult SaveSTest(SolutionTestView tv)
        {
            tv.Save();
            return RedirectToAction("SolutionTests", new { id = tv.t.SolutionBatchId });
        }

        [AllowAnonymous]
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
            return RedirectToAction("PNGCharts");
        }

        public ActionResult Show()
        {
            return View();
        }

        public JsonResult RunsEver()
        {
            var runs = Run.RunsEver(_top.ProductCodeId);

            if (_top.SampleId != 0)
                runs.Add(new Run(_top));                        // add lot context if needed

            var data = runs.Select(r => new
                {
                    lane = r.Lane,
                    id = r.LineTxId,
                    begin = r.Stamp.ToJSMSecs(),
                    finish = r.EndStamp.ToJSMSecs(),
                    samples = r.Samples,
                    productid = r.ProductCodeId,
                    productcode = r.ProductCode,
                    productspec = r.ProductSpec
                });

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Casings()
        {
            Casings s = new Casings(_top.ProductCodeId);
            return View(s);
        }

        public ActionResult CasingView(int id)
        {
            CasingView s = TempData["Casing"] as CasingView;
            return View(s??(new CasingView(id, 0)));
        }

        public ActionResult PreviousCasingView(int id)
        {
            CasingView s = new CasingView(id, -1);
            TempData["Casing"] = s;
            return RedirectToAction("CasingView", new { @id = s.SampleId });
        }

        public ActionResult NextCasingView(int id)
        {
            CasingView s = new CasingView(id, 1);
            TempData["Casing"] = s;
            return RedirectToAction("CasingView", new { @id = s.SampleId });
        }

        /// <summary>
        /// Context sensitive action.  Redirects to 
        /// 1) Lab Batches, 
        /// 2) Samples related to product or 
        /// 3) Specific Sample requested 
        /// </summary>
        /// <returns></returns>
        public ActionResult CasingSamples()
        {
            if (_top.SampleId != 0)
                return RedirectToAction("CasingView", new { @id = _top.SampleId });

            if (_top.ProductCodeId != 0)
                return RedirectToAction("Casings", new { @id = _top.ProductCodeId });

            CasingSamples s = new CasingSamples();
            return View(s);
        }

        public ActionResult CasingSampleView(int id)
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
        [AllowAnonymous]
        public ActionResult UploadCasing(HttpPostedFileBase file)
        {
            if (!ModelState.IsValid || file == null)
            {
                Error("No file uploaded");
                return RedirectToAction("CasingSamples");
            }
            var load = Models.CasingSamples.ReadExcel(file.InputStream, false, DateTime.Now.Year);

            if (load == null)
            {
                Error("Invalid file uploaded");
                return RedirectToAction("CasingSamples");
            }

            Success(load.Item1 + " casing sample records loaded for " + load.Item2);
            return RedirectToAction("CasingSamples");
        }

        [HttpPost]
        public ActionResult UploadCasings(HttpPostedFileBase file)
        {
            var load = Models.CasingSamples.ReadExcel(file.InputStream, true, DateTime.Now.Year);

            if (load == null)
                return Content("nothing loaded");

            return Content(load.Item2.Value.ToString("MM/dd HH:mm") + "\t" + load.Item1);
        }

        [DeleteFile]
        [HttpPost]
        public ActionResult GetSpreadSheet(string Product, long Start, long End)
        {
            if (_top == null)
                return Json(new { status = "error", message = "context not found" });

            if (_top.samples == null)
                return Json(new { status = "error", message = "no samples attached to context" });

            var wb = new XLWorkbook();
            var a = Start.FromJSMSecsLocal();
            var b = End.FromJSMSecsLocal();

            _top.samples.Select(w => w.Export(wb, a, b)).ToList();
            Stream stream = new MemoryStream();
            wb.SaveAs(stream);

            stream.Position = 0;
            //Session["chart"] = chart;
            return new FileStreamResult(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
            {
                FileDownloadName = Product.Replace(' ','_') + "_" + a.ToString("yyyy-MM-dd-HH-mm") + "_" + b.ToString("yyyy-MM-dd-HH-mm") + ".xlsx"
            };
        }
    }
}