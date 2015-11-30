using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BioKey.WebKey;
using Key.Models;

namespace Key.Controllers
{
    public class EnrollController : BaseController
    {
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult GetBio(string id)
        {
            webkey.SubmitURL = Url.Action(
                                    ConfigurationManager.AppSettings["method"], 
                                    ConfigurationManager.AppSettings["controller"], null, Request.Url.Scheme);

            var admin = ConfigurationManager.AppSettings["admin"];

            webkey.PersonID = id;
            try
            {
                webkey.GetPerson(admin);
                if (!webkey.BIOID.Equals(""))
                {
                    error = -1;
                    message = id + " is already enrolled";
                    return RedirectToAction("Index", "Enroll");
                }
            }
            catch (WebKeyException ex)
            {
                error = ex.ErrorCode;
                if (ex.ErrorCode != ServerError.PersonNotFound)
                {
                    message = "While getting " + id + " error occurred because " + ex.Message;
                    return RedirectToAction("Index", "Enroll");
                }
                try
                {
                    webkey.EnrollmentCode = ConfigurationManager.AppSettings["enrollcode"];
                    webkey.AddPerson(admin);
                }
                catch (WebKeyException exi)
                {
                    error = exi.ErrorCode;
                    message = "Failed adding " + id + " because " + exi.Message;
                    return RedirectToAction("Index", "Enroll");
                }

            }
            TempData["Enroll"] = webkey;
            return RedirectToAction("Enroll", "Enroll");
        }

        public ActionResult Enroll()
        {
            if (webkey == null)
                return RedirectToAction("Index", "Home");

            webkey.SubmitURL = Url.Action(
                                    ConfigurationManager.AppSettings["method"], 
                                    ConfigurationManager.AppSettings["controller"], null, Request.Url.Scheme);

            message = "Please swipe";
            try
            {
                webkey.EnrollmentStart(request);
                ViewBag.Swiper = webkey.ObjectEmbedTag;
            }
            catch (WebKeyException ex)
            {
                error = ex.ErrorCode;
                message = ex.Message;
            }
            return View();
        }

        public ActionResult Status()
        {
            Server webkey = new Server(System.Web.HttpContext.Current.Session);
            webkey.SiteID = 1001;

            HMI line = new HMI();
            try
            {
                webkey.EnrollmentSubmit(System.Web.HttpContext.Current.Request);
            }
            catch (WebKeyException ex)
            {
                message = ex.Message;
                error = ex.ErrorCode;
            }
            TempData["HMI"] = line;
            return RedirectToAction("Index", "Home");
        }
    }
}