using System;
using System.Collections.Generic;
using System.Reflection;
using System.Security.Principal;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using Tags.Models;

namespace Tags.Controllers
{
    public static class Alerts
    {
        public const string SUCCESS = "success";
        public const string ATTENTION = "attention";
        public const string ERROR = "danger";
        public const string INFORMATION = "info";

        public static string[] ALL
        {
            get
            {
                return new[]
                           {
                               SUCCESS,
                               ATTENTION,
                               INFORMATION,
                               ERROR
                           };
            }
        }
    }

    public abstract class BaseController : Controller
    {
        public static string built;
        protected bool _timeout;
        protected bool _IsAdmin;
        protected string _user;
        protected List<string> _roles;

        public void Attention(string message)
        {
            TempData.Add(Alerts.ATTENTION, message);
        }

        public void Success(string message)
        {
            TempData.Add(Alerts.SUCCESS, message);
        }

        public void Information(string message)
        {
            TempData.Add(Alerts.INFORMATION, message);
        }

        public void Error(string message)
        {
            TempData.Add(Alerts.ERROR, message);
        }

        protected override void OnActionExecuted(ActionExecutedContext context)
        {
            base.OnActionExecuted(context);
        }

        private const string aspCookie = "ASP.NET_SessionId";

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            ViewBag.built = built;
            ViewBag.User = _user = Session["user"] as string;
            _roles = Session["roles"] as List<string>;
            ViewBag.IsAdmin = _IsAdmin = false;
            if (_roles != null)
                ViewBag.IsAdmin = _IsAdmin = _roles.Contains("Administrator");
            base.OnActionExecuting(filterContext);
        }
    }
}