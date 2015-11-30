using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.SessionState;
using BioKey.WebKey;
using Key.Models;

namespace Key.Controllers
{
    public class HomeController : BaseController
    {
        /// <summary>
        /// Display swiper control.  Wait for swipe to be read.
        /// </summary>
        /// <returns></returns>
        public ActionResult Index()
        {
            webkey.SubmitURL = Url.Action("Identify", "Home", null, Request.Url.Scheme);
            message = "Please swipe";
            
            try
            {
                webkey.IdentificationStart(request);
                ViewBag.Swiper = webkey.ObjectEmbedTag;
            }
            catch (WebKeyException ex)
            {
                error = ex.ErrorCode;
                message += " "+ex.Message;
            }
            return View();                     
        }
        
        /// <summary>
        /// If identified, get lines and statuses.  Otherwise return to swiper control.
        /// </summary>
        /// <param name="h"></param>
        /// <returns></returns>
        public ActionResult Identify()
        {
            try
            {
                webkey.IdentificationSubmit(request);
                message = webkey.PersonID + " identified";
                userid = (new User(webkey.PersonID)).UserId;
                return RedirectToAction("Lines", "Home");
            }
            catch (WebKeyException ex)
            {
                message = (ex.ErrorCode == BioKey.WebKey.Error.NoMatch) ? "You could not be identified!" : ex.Message;
                ViewBag.Swiper = null;
            }
            return RedirectToAction("Index", "Home");
        }

        public ActionResult Lines()
        {
            HMIView h = new HMIView(true);
            return View(h);
        }

        /// <summary>
        /// After a line has been selected try to unlock it.
        /// </summary>
        /// <param name="id">
        /// Positive normal case: try to unlock this line id and show it alone on Line page.
        /// Negative undo case:  try to lock this line id and return to Lines page.
        /// </param>
        /// <returns></returns>
        public ActionResult Status(int id)
        {
            HMI line = new HMI(id, userid);
            message = line.message;
            return RedirectToAction("Lines", "Home");
        }
    }
}