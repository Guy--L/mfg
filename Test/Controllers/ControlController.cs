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

        public ActionResult SaveLine(Line ln)
        {
            ln.Save();
            return RedirectToAction("Lines");
        }

        public ActionResult Conversions()
        {
            var c = new Conversions();
            return View(c);
        }

        public ActionResult Conversion(int id)
        {
            var c = new ConversionView(id);
            return View(c);
        }

        public ActionResult SaveConversion(Conversion c)
        {
            c.Save();
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
            return View(p);
        }

    }
}