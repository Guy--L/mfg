using System;
using System.Web.Mvc;
using Test.Models;
using System.Diagnostics;
using System.IO;

namespace Test.Controllers
{
    public class ControlController : BaseController
    {
        // GET: Control
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Lines()
        {
            var n = new Lines();
            return View(n);
        }

        public ActionResult Line(int id)
        {
            var sample = TempData["CasingSample"] as Sample;
            var when = sample == null ? DateTime.Now : sample.Scheduled;
            var ln = new LineView(id, when);
            
            ViewBag.Undo = false;
            ln.action = null;
            return View(ln);
        }

        [HttpPost]
        public ActionResult SaveLine(LineView ln)
        {
            if (!string.IsNullOrWhiteSpace(ln.action))  
                return RedirectToAction(ln.action, new { id = ln.line.ConversionId });
            
            ln.line.Stamp = ln.when;
            ln.line.Save();
            Success("Saved line " + ln.line.Name);
            return RedirectToAction("Lines");
        }

        public ActionResult Conversions()
        {
            ViewBag.Undo = false;
            var c = new Conversions();
            return View(c);
        }

        public ActionResult Conversion(int id)
        {
            var c = new ConversionView(id);
            return View(c);
        }

        public ActionResult DeleteConversion(int id)
        {
            var c = new ConversionView(id);
            c.c.Delete();
            Success("Deleted conversion");
            return RedirectToAction("Conversions");
        }

        [HttpPost]
        public ActionResult SaveConversion(Conversion c)
        {
            if (c.Comment != null && c.Comment.Length > 200) c.Comment = c.Comment.Substring(0, 200);
            c.Save();
            Success("Saved conversion");
            return RedirectToAction("Conversions");
        }

        public ActionResult UndoConversions()
        {
            ViewBag.Undo = true;
            var c = new Conversions(true);
            return View(c);
        }

        public ActionResult ConfirmUndoConversion(int id)
        {
            var u = new UndoConversionView(id);
            if (u.prior == null)
            {
                Error("No conversion available to rollback to");
                return RedirectToAction("UndoConversions");
            }
            return View(u);
        }

        [HttpPost]
        public ActionResult UndoConversion(UndoConversionView u)
        {
            var msg = u.Commit(0);
            if (msg.Contains("Error"))
            {
                Error(msg);
                return RedirectToAction("UndoConversions");
            }
            Success(msg);
            return RedirectToAction("Conversions");
        }

        public ActionResult CompleteConversion(int id)
        {
            var c = new Conversion(id);
            var msg = c.Complete(0);                                // pass personid
            if (msg.Contains("Error"))
                Error(msg);
            else
                Success(msg);
            return RedirectToAction("Conversions");
        }

        public ActionResult IgnoreConversion(int id)
        {
            var c = new Conversion(id);
            var msg = c.Ignore(0);                                // pass personid
            if (msg.Contains("Error"))
                Error(msg);
            else
                Success(msg);
            return RedirectToAction("Conversions");
        }

        public ActionResult StartConversion(int id)
        {
            var c = new Conversion(id);
            var msg = c.Start(0);                                // pass personid
            if (msg.Contains("Error"))
                Error(msg);
            else
                Success(msg);
            return RedirectToAction("Conversions");
        }

        public ActionResult ExportConversions()
        {
            return View();
        }

        [DeleteFile]
        public ActionResult GetWeeklyProduction(DateTime monday)
        {
            //var chart = TempData["chart"] as Chart;
            //if (chart == null)
            //    return new EmptyResult();
            //Stream stream = new MemoryStream();
            //var count = chart.Export(stream);
            //stream.Position = 0;
            //Debug.WriteLine(count);
            //TempData["chart"] = chart;
            //return new FileStreamResult(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
            //{
            //    FileDownloadName = chart.exportName + "_" + DateTime.Now.ToString("yyyyMMddhhmmss") + ".xlsx"
            //};
            Information("Excel export is not implemented yet (" + monday + " requested)");
            return RedirectToAction("Conversions");
        }

        public ActionResult Products()
        {
            var p = new Products();
            return View(p);
        }

        public ActionResult Product(int id)
        {
            var p = new ProductView(id);
            ViewBag.Cloned = false;
            return View(p);
        }

        public ActionResult CloneProduct(int id)
        {
            var p = new ProductView(id);
            var q = new ProductView(p.p);
            Success(p.p._ProductCode + " " + p.p.ProductSpec + " cloned here ready to add");
            ViewBag.Cloned = true;
            return View("Product", q);
        }

        [HttpPost]
        public ActionResult SaveProduct(ProductCode p)
        {
            // check for existing product
            if (p.Exists())
            {
                Error(p._ProductCode + " " + p.ProductSpec + " already exists");
                var q = new ProductView(p);
                ViewBag.Cloned = true;
                return View("Product", q);
            }
            p.Save();
            Success("Saved product");
            return RedirectToAction("Products", "Control");
        }
    }
}