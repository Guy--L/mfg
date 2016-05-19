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
    public class AdminController : BaseController
    {
        // GET: Admin
        public ActionResult Index()
        {
            throw new Exception("Test Error");
            return View();
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
        public ActionResult Digest(string s, string y)
        {
            if (!Directory.Exists(s))
            {
                Error(s + " does not exist");
                return RedirectToAction("CasingSamples", "Home");
            }
            int year = 2016;
            if (!int.TryParse(y, out year))
            {
                Error(y + " year is wrong");
                return RedirectToAction("CasingSamples", "Home");
            }
            s = CasingSamples.ReadExcels(s, year);
            TempData["stats"] = s;
            Success(s);
            return RedirectToAction("CasingSamples", "Home");
        }

        public ActionResult Completed()
        {
            ViewBag.Message = TempData["stats"];
            return View();
        }

        public ActionResult CompleteAll(int year)
        {
            CasingSampleArchive.complete(year);
            CasingSampleArchive.publish(year);
            return RedirectToAction("CasingSamples", "Home");
        }
    }
}