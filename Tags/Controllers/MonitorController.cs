using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Tags.Models;

namespace Tags.Controllers
{
    public class MonitorController : Controller
    {
        private static string _name2tag = @"
              where t.name = @0
        ";

        [Route("{channel:regex(^[A-D][1-8]$)}/{device:regex(Dry|Wet)}/{tagname:regex(^[A-Za-z0-9_]+$)}")]
        public ActionResult Index(string channel, string device, string tagname)
        {
            var name = channel + "." + device + "." + tagname;           
            var current = Current.SingleOrDefault(" where name=@0", name);
            if (current == null)
                return View("NoSuchTag", name);

            var limit = Limit.SingleOrDefault(" where tagid=@0", current.TagId);
            if (limit == null)
                return View("Monitor", current);

            limit.TagName = name;
            return View("Limit", limit);
        }
    }
}