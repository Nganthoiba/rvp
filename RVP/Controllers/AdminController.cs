using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using RVP.Models;
using Microsoft.AspNet.Identity.EntityFramework;

namespace RVP.Controllers
{
    public class AdminController : Controller
    {
        BOSEMEntities context = new BOSEMEntities();
        // GET: Admin
        [Authorize(Roles = "Admin")]
        public ActionResult Index()
        {
            return View();
        }

        [Authorize(Roles = "Admin")]
        public ActionResult Users() {
            if (User.Identity.IsAuthenticated)
            {
                var current_user = User.Identity;
                string user_id = current_user.GetUserId();
                List<AspNetUsers> aspNetUsers = context.AspNetUsers.Where(m=>m.Id != user_id).ToList();
                List<UsersModel> users = new List<UsersModel>();
                foreach (AspNetUsers row in aspNetUsers)
                {
                    UsersModel user = new UsersModel();
                    user.Id = row.Id;
                    user.image = row.image == null ? "/images/user.png" : row.image;
                    user.Email = row.Email;
                    user.PhoneNumber = row.PhoneNumber;
                    user.UserName = row.UserName;
                    users.Add(user);
                }
                return View(users);
            }
            return RedirectToAction("Login", "Account");
        }
    }
}