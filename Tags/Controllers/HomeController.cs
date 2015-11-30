using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Tags.Models;

namespace Tags.Controllers
{
    public class HomeController : BaseController
    {
        public ActionResult Index()
        {
            return RedirectToAction("Review", "Home");
        }

        public ActionResult Review()
        {
            var lines = Channel.activeDevices();
            var channels = new ChannelView(lines);
            return View(channels); 
        }

        public ActionResult TagsByLine(string id)
        {
            var nobricks = id.Substring(1, id.Length - 2);
            var pt = new PickTagView(nobricks, _user);
            return View(pt);
        }

        [HttpPost]
        public ActionResult PickedTags(PickTagView p)
        {
            if (!ModelState.IsValid)
                return RedirectToAction("Index", "Home");

            if (p.Cancel)
                return RedirectToAction("Review", "Home");

            ViewData["Channel"] = p.Channel;
            Chart.SaveView(_user, p.NewView, p.picked);
            var chart = new Chart(p.picked);
            TempData["chart"] = chart;
            return View(chart);
        }

        [DeleteFile]
        public ActionResult GetSpreadSheet()
        {
            var chart = TempData["chart"] as Chart;
            if (chart == null)
                return new EmptyResult();
            Stream stream = new MemoryStream();
            var count = chart.Export(stream);
            stream.Position = 0;
            Debug.WriteLine(count);
            TempData["chart"] = chart;
            return new FileStreamResult(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
            {
                FileDownloadName = chart.exportName + "_" + DateTime.Now.ToString("yyyyMMddhhmmss") + ".xlsx"
            };
        }
    }
}
