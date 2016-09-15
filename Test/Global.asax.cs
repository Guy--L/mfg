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

            using (labDB db = new labDB())
            {
                Reading.Types = db.Fetch<Parameter>().ToDictionary(k => k.ParameterId, v => v);
                Reading.TypeOf = Reading.Types.ToDictionary(k => k.Value.Name, v => v.Key);
                Reading.Times = Reading.Types.ToDictionary(k => k.Key, k => k.Value.Diary.Split(',').Select(h => int.Parse(h)).ToList());
                Reading.SetLines(db.Fetch<Line>());
                Status.SetIcons(db.Fetch<Status>());
                SolutionRecipe.Solutions = db.Fetch<SolutionRecipe>().ToDictionary(k => k.SolutionRecipeId, v => v.SolutionType);
                Extruder.Colors = db.Fetch<Extruder>().ToDictionary(k => k.ExtruderId, v => v.Color);
                Models.System.Systems = db.Fetch<Models.System>().ToDictionary(k => k.SystemId, v => v._System.Replace("unassigned","0"));
            }

            CasingSample.mapReflection();

            Unit.ips = ConfigurationManager.AppSettings["unitstations"].Split(',');
            Unit.setLineIds();

            //LineStatus.Initialize();
            //    LineTestView.Lines = db.Fetch<Line>();
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
