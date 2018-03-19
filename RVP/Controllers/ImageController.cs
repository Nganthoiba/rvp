using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;
using RVP.Models;
using Microsoft.AspNet.Identity;
using System.Net.Http;
using System.Net;

namespace RVP.Controllers
{
    public class ImageController : Controller
    {
        private BOSEMEntities db = new BOSEMEntities();
        // GET: Image
        public ActionResult Index()
        {
            return View();
        }
        
        [HttpPost]
        public ActionResult cropped_img(ProfileImage profileImage)
        {
            string user_id = profileImage.user_id;
            string base64 = profileImage.img_data;
            byte[] bytes = Convert.FromBase64String(base64.Split(',')[1]);

            string file_name = "profileimg.png";
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

        /**** To remove profile image ****/
        [HttpDelete]
        public ActionResult removeImage(RemoveImage model) {
            if (Request.IsAuthenticated)
            {
                if (model.user_id != null && model.user_id==User.Identity.GetUserId())
                {
                    db.Database.ExecuteSqlCommand("update AspNetUsers set image=NULL where Id='" + model.user_id + "'");
                    return Json(new { message = "Your profile image has been removed." });
                }
                else
                {
                    return Json(new { message = "Invalid User" });
                }
            }             
            return Json(new { message = "Please log in first." });
        }


        [HttpPost]
        public ActionResult ImageUpload(string id)
        {

            if (Request.Files.Count > 0)
            {
                try
                {
                    //  Get all files from Request object  
                    HttpFileCollectionBase files = Request.Files;
                    string savedurl = "~/images/uploads/" + id + "/";
                    string path = "/images/uploads/" + id + "/";
                    string file_name = "";
                    for (int i = 0; i < files.Count; i++)
                    {
                        //string path = AppDomain.CurrentDomain.BaseDirectory + "Uploads/";  
                        //string filename = Path.GetFileName(Request.Files[i].FileName);  

                        HttpPostedFileBase file = files[i];
                        string fname;

                        // Checking for Internet Explorer  
                        if (Request.Browser.Browser.ToUpper() == "IE" || Request.Browser.Browser.ToUpper() == "INTERNETEXPLORER")
                        {
                            string[] testfiles = file.FileName.Split(new char[] { '\\' });
                            fname = testfiles[testfiles.Length - 1];
                        }
                        else
                        {
                            fname = file.FileName;
                            file_name = file.FileName;//just for path
                        }

                        bool exists = System.IO.Directory.Exists(Server.MapPath(savedurl));

                        // if Folder doesn't exists create it
                        if (!exists)
                        {
                            System.IO.Directory.CreateDirectory(Server.MapPath(savedurl));
                        }
                        // Get the complete folder path and store the file inside it.  
                        fname = Path.Combine(Server.MapPath(savedurl), fname);

                        //save file on server
                        file.SaveAs(fname);

                        db.Database.ExecuteSqlCommand("update AspNetUsers set image='" + path + file.FileName + "' where Id='" + id + "'");
                        //saving it in database
                    }
                    // Returns message that successfully uploaded  
                    return Json(new { MSG = "Image Uploaded Successfully!", SavedUrl = path + file_name }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception ex)
                {
                    return Json("Error occurred. Error details: " + ex.Message);
                }
            }
            else
            {
                return Json("No files selected.");
            }
        }
    }
}