using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using TwitterBootstrapMVC;
using Test.Models;
using Test.Controllers;
using Test.Properties;
using System.Threading;
using System.Configuration;
using Test.Hubs;
using Microsoft.AspNet.SignalR;

namespace Test
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
            SolutionChart.path = Server.MapPath("~/Content/Slides/");

            CasingSample.mapReflection();

            Unit.ips = ConfigurationManager.AppSettings["unitstations"].Split(',');
            Unit.setLineIds();

            int[] systms = new int[] { 1, 2, 4, 5, 6, 7, 8, 9, 10, 11, 12 };
            foreach (var i in systms)
                SolutionChart.UpdateDeck(i);

            //LineStatus.Initialize();
            //    LineTestView.Lines = db.Fetch<Line>();
        }

        protected void Application_Exit()
        {
            var hub = GlobalHost.ConnectionManager.GetHubContext<RefreshHub>();
            hub.Clients.Group("Deck").upVersion();
        }

        protected void ErrorMail_Mailing(object sender, Elmah.ErrorMailEventArgs e)
        {
            e.Mail.Body = "Project: unit-readings\n" + e.Mail.Body;
        } 

        protected void Session_Start(object sender, EventArgs e)
        {
#if DEBUG
            var user = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
            if (user == "GUYLISTER3546\\guy")
                user = "lister.g.1";
#else
            var user = Thread.CurrentPrincipal.Identity.Name;
            if (user == null)
                user = "Anonymous";
#endif
            //scheduleDB _db = new scheduleDB();
            HttpContext.Current.Session["user"] = user;
            //HttpContext.Current.Session["authority"] = _db.Fetch<User>(string.Format(Models.User.get_role, user)).FirstOrDefault();
        }

    }
}
