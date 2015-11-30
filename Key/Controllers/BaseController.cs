using System;
using System.Collections.Generic;
using System.Reflection;
using System.Security.Principal;
using System.Linq;
using System.Web;
using c = System.Web.HttpContext;
using System.Web.Mvc;
using System.Web.Security;
using BioKey.WebKey;
using System.Configuration;

namespace Key.Controllers
{
    public abstract class BaseController : Controller
    {
        protected int error;
        protected string message;
        protected int userid;
        protected DateTime started;
        protected Server webkey;
        protected HttpRequest request;

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            Server cache = TempData["Enroll"] as Server;
            webkey = cache==null ? new Server(c.Current.Session): cache;
            webkey.SiteID = int.Parse(ConfigurationManager.AppSettings["siteid"]);
            request = c.Current.Request;
            message = (TempData["Message"] as string)??"";
            var uid = TempData["UserId"] as int?;
            userid = uid.HasValue?uid.Value:0;

            if (started == null) started = DateTime.Now;

            base.OnActionExecuting(filterContext);
        }

        protected override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            base.OnActionExecuted(filterContext);
            TempData["Message"] = message;
            TempData["UserId"] = userid;
            ViewBag.message = message;
            ViewBag.elapsed = DateTime.Now.Subtract(started).TotalSeconds;
        }
    }
}