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
            Session["LineId"] = id;
            return View(pt);
        }

        [HttpPost]
        public ActionResult PickedTags(PickTagView p)
        {
            if (!ModelState.IsValid)
                return RedirectToAction("Index", "Home");

            TempData["picked"] = p;

            if (p.NewView != null && !string.IsNullOrWhiteSpace(p.NewView) && p.picked != null)
                Graph.SaveView(_login, p.NewView, p.picked);

            if (p.DeletedViews != null && !string.IsNullOrWhiteSpace(p.DeletedViews))
                Graph.DeleteView(p.DeletedViews.Substring(1));

            if (p.Monitor)
                return RedirectToAction("Picked2Monitor");

            if (p.picked == null)
            {
                Error("Please pick at least one tag before charting");
                var tid = Session["LineId"] as string;
                return RedirectToAction("TagsByLine", "Home", new { id = tid });
            }
            if (p.Cancel)
                return RedirectToAction("Review", "Home");

            ViewData["Channel"] = p.Channel;
            var chart = new Chart(p.picked);
            Session["chart"] = chart;
            return View(chart);
        }

        public ActionResult Picked2Monitor()
        {
            var p = TempData["picked"] as PickTagView;
            if (p.picked == null)
            {
                Error("Please pick at least one tag before charting");
                var tid = Session["LineId"] as string;
                return RedirectToAction("TagsByLine", "Home", new { id = tid });
            }

            ViewData["Channel"] = p.Channel;
            var twoHours = DateTime.Now.AddHours(-2);
            var chart = new Chart(p.picked, twoHours);
            Session["picked"] = p;
            return View(chart);
        }

        public ActionResult RefreshMonitor()
        {
            var p = Session["picked"] as PickTagView;
            if (p == null)
                return new EmptyResult();

            TempData["picked"] = p;
            return RedirectToAction("Picked2Monitor");
        }

        [DeleteFile]
        [HttpPost]
        public ActionResult GetSpreadSheet(Chart c)
        {
            var chart = Session["chart"] as Chart;
            if (chart == null)
                return new EmptyResult();

            chart.zoomA = c.zoomA;
            chart.zoomB = c.zoomB;
            Stream stream = new MemoryStream();
            var count = chart.Export(stream);
            stream.Position = 0;
            Session["chart"] = chart;
            return new FileStreamResult(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
            {
                FileDownloadName = chart.exportName + "_" + DateTime.Now.ToString("yyyyMMddhhmmss") + ".xlsx"
            };
        }
    }
}
