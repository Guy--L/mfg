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

        public ActionResult Products()
        {
            return View();
        }

        public ActionResult Product()
        {
            return View();
        }

    }
}