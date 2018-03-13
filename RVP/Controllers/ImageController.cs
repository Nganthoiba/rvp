using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;
using RVP.Models;
using Microsoft.AspNet.Identity;

namespace RVP.Controllers
{
    public class ImageController : Controller
    {
        // GET: Image
        public ActionResult Index()
        {
            return View();
        }
        private BOSEMEntities db = new BOSEMEntities();

        [HttpPost]
        public ActionResult cropped_img(ProfileImage profileImage)
        {
            string user_id = profileImage.user_id;
            string base64 = profileImage.img_data;
            byte[] bytes = Convert.FromBase64String(base64.Split(',')[1]);

            string file_name = "profile.png";
            string savedurl = "/images/uploads/" + user_id;

            bool exists = Directory.Exists(Server.MapPath(savedurl));
            if (!exists)
            {
                Directory.CreateDirectory(Server.MapPath(savedurl));
                Directory.GetAccessControl(Server.MapPath(savedurl));
            }
            try
            {
                using (FileStream stream = new FileStream(Server.MapPath(savedurl + "/" + file_name), FileMode.Create))
                {
                    stream.Write(bytes, 0, bytes.Length);
                    stream.Flush();
                    db.Database.ExecuteSqlCommand("update AspNetUsers set image='" + savedurl + "/" + file_name + "' where Id='" + user_id + "'");
                }
                return Json(new { message = "Successfully uploaded.", SavedUrl = savedurl + "/" + file_name }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception exc)
            {
                return Json(new { message = exc.GetBaseException().ToString() }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}