using System;
using System.Collections.Generic;
using System.Reflection;
using System.Security.Principal;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using Test.Models;
using Microsoft.AspNet.SignalR;
using Test.Hubs;

namespace Test.Controllers
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

    public static class SessionExtensions
    {
        public static T Get<T>(this HttpSessionStateBase session, string key)
        {
            return (T)session[key];
        }

        public static void Set<T>(this HttpSessionStateBase session, string key, object value)
        {
            session[key] = value;
        }
    }

    public abstract class BaseController : Controller
    {
        public static string built;
        protected bool _timeout;
        protected string _user;
        public Context _top;

        //private static User _user;
        //protected bool _IsAdmin;
        //protected User EiUser
        //{                         // name collision with Identity.User?
        //    get { return _user; }
        //    set
        //    {
        //        _timeout = false;
        //        _user = value;
        //        Session.Set<User>("User", value);

        //        if (value == null)
        //        {
        //            FormsAuthentication.SignOut();
        //            Session.Abandon();

        //            // clear authentication cookie
        //            var cookie1 = new HttpCookie(FormsAuthentication.FormsCookieName, "")
        //            {
        //                HttpOnly = true,
        //                Secure = FormsAuthentication.RequireSSL,
        //                Path = FormsAuthentication.FormsCookiePath,
        //                Domain = FormsAuthentication.CookieDomain,
        //                Expires = DateTime.Now.AddYears(-1)
        //            };

        //            Response.Cookies.Add(cookie1);

        //            // clear session cookie (not necessary for your current problem but i would recommend you do it anyway)
        //            var cookie2 = new HttpCookie("ASP.NET_SessionId", "")
        //            {
        //                HttpOnly = true,
        //                Secure = FormsAuthentication.RequireSSL,
        //                Path = FormsAuthentication.FormsCookiePath,
        //                Domain = FormsAuthentication.CookieDomain,
        //                Expires = DateTime.Now.AddYears(-1)
        //            };

        //            Response.Cookies.Add(cookie2);

        //            FormsAuthentication.RedirectToLoginPage();
        //        }
        //        else
        //        {
        //            var fat = new FormsAuthenticationTicket(value.FullName, true, (int)FormsAuthentication.Timeout.TotalMinutes);
        //            var fid = new FormsIdentity(fat);

        //            var roles = new List<string>();
        //            if (value.IsAdmin) roles.Add("Admin");

        //            HttpContext.User = new GenericPrincipal(fid, roles.ToArray());
        //            FormsAuthentication.SetAuthCookie(value.FullName, false);
        //            string encryptedTicket = FormsAuthentication.Encrypt(fat);
        //            var cookie = new HttpCookie(FormsAuthentication.FormsCookieName, encryptedTicket)
        //            {
        //                HttpOnly = true,
        //                Secure = FormsAuthentication.RequireSSL,
        //                Path = FormsAuthentication.FormsCookiePath,
        //                Domain = FormsAuthentication.CookieDomain
        //            };
        //            Response.Cookies.Remove(FormsAuthentication.FormsCookieName);
        //            Response.Cookies.Add(cookie);
        //        }
        //    }
        //}

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

        //    if (context.HttpContext.Session == null || context.HttpContext.Session.IsNewSession)
        //    {
        //        var test = context.Result as ViewResultBase;

        //        if (test == null)
        //        {
        //            //#if DEBUG
        //            //                    Error("Dev: No model has been prepared for view in base controller");
        //            //#endif
        //            return;
        //        }

        //        if (_timeout || test.Model == null)
        //        {
        //            var result = new ViewResult { ViewName = "Login" };
        //            var model = new LoginModel()
        //            {
        //                UserName = "",
        //                Password = "",
        //                ReturnUrl = Request.Url.LocalPath == "~/Account/Login" ? "" :
        //                    HttpUtility.UrlEncode(context.HttpContext.Request.RawUrl)
        //            };
        //            result.ViewData.Model = model;
        //            context.Result = result;
        //        }                //return;
        //        if (_timeout)
        //        {
        //            TempData.Add(Alerts.ERROR, "Your session has timed out after " + Session.Timeout +
        //                                        " minutes of inactivity.");
        //            _timeout = false;
        //        }
        //    }
        //    base.OnActionExecuted(context);
        //}

        private const string aspCookie = "ASP.NET_SessionId";

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            ViewBag.built = built;
            ViewBag.User = _user = Session.Get<string>("user");
            if (ContextHub.contexts.TryGetValue(_user, out _top))
                Session.Set<Context>("Context", _top);
            else
                Session.Remove("Context");
            ViewBag.Context = _top;   

            base.OnActionExecuting(filterContext);
        }
        //    protected override void OnActionExecuting(ActionExecutingContext context)
        //    {
        //        ViewBag.IsAdmin = false;

        //        if ((context.HttpContext.Session != null) && context.HttpContext.Session.IsNewSession)
        //        {
        //            // if this is a new session but we can read a cookie with the name
        //            // ASP.Net_SesssionId then it must be a session that has expired.

        //            string c = context.HttpContext.Request.Headers["Cookie"];
        //            _timeout = ((c != null) && (c.IndexOf(aspCookie, StringComparison.CurrentCultureIgnoreCase) >= 0));
        //        }
        //        else
        //        {
        //            if (_user == null)
        //            {
        //                _user = Session.Get<User>("User");
        //            }
        //            if (_person == null)
        //            {
        //                _person = Session.Get<Person>("Person");
        //            }
        //        }
        //        base.OnActionExecuting(context);
        //        ViewBag.IsAdmin = false;
        //        if (_user != null)
        //        {
        //            ViewBag.IsAdmin = _user.IsAdmin;
        //            _IsAdmin = _user.IsAdmin;
        //        }
        //    }
        //}
    }
}