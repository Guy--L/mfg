using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Tags.Controllers;
using Tags.Models;
using Tags.Properties;
using TwitterBootstrapMVC;

namespace Tags
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            Bootstrap.Configure();
            BaseController.built = Resources.BuildDate;

            XFactory.Initialize(Server.MapPath("~/App_Data/XML/"));
            Tag.All();
            TagMap.projectPath = Server.MapPath("~/App_Data/XML/TestOutput.xml");
        }

        protected void Session_Start(object sender, EventArgs e)
        {
#if DEBUG
            var user = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
            if (user == "GUYLISTER3546\\guy" || user == "NETFDOMAIN\\lister")
                user = "lister";
#else
            var user = Thread.CurrentPrincipal.Identity.Name;
#endif
            //scheduleDB _db = new scheduleDB();
            HttpContext.Current.Session["user"] = user;
            if (user != null)
            {
                var u = new User(user);
                if (u == null || u.UserId == 0)
                    return;
                Session["login"] = u;
                var roles = u.Roles();
                Session["roles"] = roles.Select(r => r._Role).ToList();
            }

            //HttpContext.Current.Session["authority"] = _db.Fetch<User>(string.Format(Models.User.get_role, user)).FirstOrDefault();
        }

    }
}
