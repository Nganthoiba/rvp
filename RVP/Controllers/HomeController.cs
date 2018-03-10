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
            ViewBag.Message = "Your contact page.";

            return View();
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
                    string savedurl = "~/images/uploads/"+id+"/";
                    string path = "/images/uploads/"+id+"/";
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
                        
                        db.Database.ExecuteSqlCommand("update AspNetUsers set image='"+ path + file.FileName + "' where Id='"+id+"'");
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