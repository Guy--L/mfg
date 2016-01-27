using System;
using System.Web.Mvc;
using Test.Models;

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
            var ln = new LineView(id);
            return View(ln);
        }

        [HttpPost]
        public ActionResult SaveLine(LineView ln)
        {
            ln.line.Stamp = DateTime.Now;
            ln.line.Save();
            Success("Saved line");
            return RedirectToAction("Lines");
        }

        public ActionResult Conversions()
        {
            ViewBag.Undo = false;
            var c = new Conversions(Models.Conversions._pending);
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
            if (c.Note != null && c.Note.Length > 50) c.Note = c.Note.Substring(0, 50);
            c.Save();
            Success("Saved conversion");
            return RedirectToAction("Conversions");
        }

        public ActionResult UndoConversions()
        {
            ViewBag.Undo = true;
            var c = new Conversions(Models.Conversions._recent);
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
            var msg = u.current.Undo(0);
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
            return View(c);
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