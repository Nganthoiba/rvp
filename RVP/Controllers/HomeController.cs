using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.IO;
using RVP.Models;
using Microsoft.AspNet.Identity;

namespace RVP.Controllers
{
    public class HomeController : Controller
    {
        private BOSEMEntities db = new BOSEMEntities();

        public string profileImg() {
            if (!Request.IsAuthenticated)
            {
                return "/images/user.png";
            }
            string user_id = User.Identity.GetUserId() == null ? "" : User.Identity.GetUserId();
            AspNetUsers user = db.AspNetUsers.Find(user_id);
            string path = user.image == null ? "/images/user.png" : user.image;
            return path;
        }
        
        public ActionResult Index()
        {
            if (!Request.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }
            
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Contact and address";
            return View();
        }
    }
}