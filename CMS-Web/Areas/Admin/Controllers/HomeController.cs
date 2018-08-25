using CMS_DTO.CMSCrawler;
using CMS_DTO.CMSHome;
using CMS_Shared;
using CMS_Shared.Keyword;
using CMS_Shared.Utilities;
using CMS_Web.Web.App_Start;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;
using System.Web.Security;

namespace CMS_Web.Areas.Admin.Controllers
{
    [NuAuth]
    public class HomeController : Controller
    {
        private CMSKeywordFactory _factory;
        public HomeController()
        {
            _factory = new CMSKeywordFactory();
        }
        public ActionResult Index()
        {
            //var url = "https://www.facebook.com/Awesome-People-Are-Born-In-September-863456000490709/";
            //CMS_CrawlerModels pins = new CMS_CrawlerModels();
            //var cookie = "fr=0g932KaBNIHkPNSHd.AWXxf95lRMddtf2MRVvraujAkc0.BbMcZu.uD.Fs5.0.0.BbPiEI.AWVEgjKc; sb=NmI3W-ffluEtyFHleEWSjhBl; wd=1366x631; datr=NmI3WwtbosYtTwDtslqJtXZd; c_user=100003727776485; xs=136%3Au6XG_yUasjTeFQ%3A2%3A1530356294%3A6091%3A726; pl=n; spin=r.4072650_b.trunk_t.1530773112_s.1_v.2_; presence=EDvF3EtimeF1530798371EuserFA21B03727776485A2EstateFDutF1530798371621Et3F_5b_5dElm3FA2user_3a150588258893142A2Eutc3F1530798370407CEchFDp_5f1B03727776485F2CC; act=1530798384170%2F5";
            //CrawlerFbHelpers.CrawlerAllFb(url,ref pins, cookie);
            return View();
        }

        [HttpGet]
        public ActionResult Logout()
        {
            try
            {
                if (Session["User"] == null)
                    return RedirectToAction("CMSAccount", new { area = "Admin" });

                FormsAuthentication.SignOut();
                Session.Remove("User");
                HttpCookie currentUserCookie = HttpContext.Request.Cookies["UserCookie"];
                HttpContext.Response.Cookies.Remove("UserCookie");
                currentUserCookie.Expires = DateTime.Now.AddDays(-10);
                currentUserCookie.Value = null;
                HttpContext.Response.SetCookie(currentUserCookie);

                return RedirectToAction("Index", "Home", new { area = "Admin" });
            }
            catch (Exception ex)
            {
                NSLog.Logger.Error("Logout Error: ", ex);
                return new HttpStatusCodeResult(400, ex.Message);
            }
        }
    }
}