using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using Test.Models;

namespace Test.Controllers
{
    public class AdminController : Controller
    {
        // GET: Admin
        public ActionResult Index()
        {
            var types = AppDomain.CurrentDomain.GetAssemblies()
                       .SelectMany(t => t.GetTypes())
                       .Where(t => t.IsClass && t.BaseType != null && t.BaseType.Name.StartsWith("Record") && t.Namespace == "Test.Models").ToList();
            TempData["Types"] = types;
            return View(types);
        }

        [HttpPost]
        public ActionResult Fields(int picked)
        {
            List<Type> t = TempData["Types"] as List<Type>;

            var fields = t[picked].GetProperties().ToList();
            var picko = new PickFieldView(fields);
            return View(picko);
        }

        public ActionResult BatchPath()
        {
            return View();
        }


        [HttpPost]
        public ActionResult Ingest(string s)
        {
            if (!Directory.Exists(s))
            {
                s += " does not exist";
                return RedirectToAction("Completed", new { id = s });
            }
            s = CasingSamples.ReadExcels(s, 2015);

            return RedirectToAction("Completed", new { id = s });
        }

        [HttpPost]
        public ActionResult Digest(string s)
        {
            if (!Directory.Exists(s))
            {
                s += " does not exist";
                return RedirectToAction("Completed", new { id = s });
            }
            s = CasingSamples.ReadExcels(s, 2015);
            TempData["stats"] = s;
            //return RedirectToAction("Completed", new { id = s });
            return RedirectToAction("Completed");
        }

        public ActionResult Completed()
        {
            ViewBag.Message = TempData["stats"];
            return View();
        }
    }
}