using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CronExpressionDescriptor;
using Microsoft.AspNet.SignalR;
using Quartz;
using Tags.Hubs;
using Tags.Models;

namespace Tags.Controllers
{
    public class AdminController : BaseController
    {
        // GET: Admin
        public ActionResult Index()
        {
            throw new Exception("This is test Exception");
            return View();
        }

        public ActionResult Upload()
        {
            return View();
        }

        public ActionResult Reload()
        {
            return View();
        }

        public ActionResult Restart()
        {
            var r = new Restarter();
            return RedirectToAction("Index", "Home");
        }

        public ActionResult PickReview()
        {
            var p = new PickReview(0);
            var r = TempData["rejectedReview"] as PickReview;
            if (r != null)
            {
                p.NewSchedule = r.NewSchedule;
                p.NewReview = r.NewReview;
                p.picked = r.picked;
            }                
            return View(p);
        }

        [HttpPost]
        public ActionResult PickedReview(PickReview p)
        {
            if (p.NewReview == null || string.IsNullOrWhiteSpace(p.NewReview))
                Error("No review name provided");
            else if (p.picked == null || p.picked.Length == 0)
                Error("No charts/jobs were selected");
            else if (!CronExpression.IsValidExpression(p.NewSchedule))
                Error(p.NewSchedule + " is not a valid Cron Expression");
            else {
                TagHub.Update(p.ReviewId, p.NewReview, p.NewSchedule);
                Review.Save(p.NewReview, p.NewSchedule, p.picked);
                Success(p.NewReview + " has been updated to run " + ExpressionDescriptor.GetDescription(p.NewSchedule));
                p = null;
            }
            TempData["rejectedReview"] = p;
            return RedirectToAction("PickReview", "Admin");
        }

        [HttpPost]
        public ActionResult Upload(HttpPostedFileBase file)
        {
            var path = Path.Combine(Server.MapPath("~/App_Data/Files/"), Path.GetFileName(file.FileName));

            var data = new byte[file.ContentLength];
            file.InputStream.Read(data, 0, file.ContentLength);

            using (var sw = new FileStream(path, FileMode.Create))
            {
                sw.Write(data, 0, data.Length);
            }

            var tm = new TagMap();
            tm.Import(path);

            return RedirectToAction("Reload");
        }

        public ActionResult Users()
        {
            var users = Tags.Models.User.Fetch("");
            return View(users);
        }

        public ActionResult UserEdit(int id)
        {
            var user = new User(id);
            return View(user);
        }

        [HttpPost]
        public ActionResult UserUpdate(User u)
        {
            u.Save();
            return RedirectToAction("Users");
        }

        public ActionResult UserDelete(int id)
        {
            var ret = Tags.Models.User.Delete(id);
            Success("Deleted User with return code of " + ret);
            return RedirectToAction("Users");
        }

        public ActionResult Role(int id)
        {
            var role = new Role(id);
            return View(role);
        }

        public ActionResult Roles()
        {
            var roles = Tags.Models.Role.Roles();
            return View(roles);
        }

        [HttpPost]
        public ActionResult RoleUpdate(Role r)
        {
            if (r.ADGroup == null) r.ADGroup = "";
            r.Save();
            return RedirectToAction("Roles");
        }

        public ActionResult UserRoles(int id)
        {
            var rv = new RoleView(id);
            return View(rv);
        }

        public ActionResult UserRoleUpdate(RoleView rv)
        {
            rv.Save();
            return RedirectToAction("Users", "Admin");
        }

        public ActionResult Groups()
        {
            List<Group> groups = null;
            using (tagDB t = new tagDB())
            {
                groups = t.Fetch<Group>(" where UserId = 0");
            }
            return View(groups);
        }

        public ActionResult Group(int? id)
        {
            if (!id.HasValue) id = 0;
            var gv = new GroupView(id.Value);
            return View(gv);
        }

        [HttpPost]
        public ActionResult Regroup(GroupView gv) 
        {
            gv.Save();
            return RedirectToAction("Groups");
        }
    }
}