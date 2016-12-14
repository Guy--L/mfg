using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Tags.Controllers;
using Tags.Models;
using Tags.Properties;
using TwitterBootstrapMVC;
using Quartz;
using Quartz.Impl;
using Common.Logging.Configuration;
using spc = System.Collections.Specialized;
using System.Configuration;
using System.Threading;
using System.Diagnostics;

namespace Tags
{
    public class MvcApplication : System.Web.HttpApplication
    {
        public static IScheduler sched;

        protected void Application_Start()
        {
            Debug.WriteLine("Application Startup");
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            Bootstrap.Configure();
            BaseController.built = Resources.BuildDate;

            XFactory.Initialize(Server.MapPath("~/App_Data/XML/"));
            Tag.All();
            TagMap.projectPath = Server.MapPath("~/App_Data/XML/TestOutput.xml");

            All.GetFirstStamp();
            Past.GetFirstStamp();

            var config = (spc.NameValueCollection) ConfigurationManager.GetSection("quartz");
            ISchedulerFactory schedFact = new StdSchedulerFactory(config);
            // get a scheduler
            NameValueCollection properties = new NameValueCollection();
            Common.Logging.LogManager.Adapter = new Common.Logging.EventSource.EventSourceLoggerFactoryAdapter(properties);
            sched = schedFact.GetScheduler();
            //sched.Start();
        }

        protected void Application_Stop()
        {
            //sched.Shutdown();
        }

        protected void Session_Start(object sender, EventArgs e)
        {
#if DEBUG
            var user = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
#elif DEMO
            var user = "gseibel";
#else
            var user = Thread.CurrentPrincipal.Identity.Name;
#endif
            //scheduleDB _db = new scheduleDB();
            HttpContext.Current.Session["user"] = user;
            if (user != null)
            {
                var usr = user.Split('\\')[1];
                var u = new User(usr);
                if (u == null || u.UserId == 0)
                    return;
                Session["login"] = u;
                var roles = u.Roles();
                Session["roles"] = roles.Select(r => r._Role).ToList();
            }
            All.GetFirstStamp();

            //HttpContext.Current.Session["authority"] = _db.Fetch<User>(string.Format(Models.User.get_role, user)).FirstOrDefault();
        }

    }
}
