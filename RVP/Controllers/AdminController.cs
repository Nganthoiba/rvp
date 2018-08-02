using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using iTextSharp.text;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using RVP.Models;
using Microsoft.AspNet.Identity.EntityFramework;

namespace RVP.Controllers
{
    public class AdminController : Controller
    {
        BOSEMEntities context = new BOSEMEntities();
        ApplicationDbContext cont = new ApplicationDbContext();

        public Boolean isAdmin(string user_id)
        {
            var UserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(cont));
            var s = UserManager.GetRoles(user_id);
            if (s[0].ToString() == "Admin")
            {
                return true;
            }
            else
            {
                return false;
            }     
        }
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
                    if (!isAdmin(row.Id))
                    {
                        user.Id = row.Id;
                        user.image = row.image == null ? "/images/user.png" : row.image;
                        user.Email = row.Email;
                        user.PhoneNumber = row.PhoneNumber;
                        user.UserName = row.UserName;
                        users.Add(user);
                    }
                }
                return View(users);
            }
            return RedirectToAction("Login", "Account");
        }

        [Authorize(Roles = "Admin")]
        public ActionResult AllRequestHistory() {
            List<RequestHistories> requested_mark_list = context.RequestHistories.Take(5).ToList();
            List<AllRequestHistModel> all_request_list = new List<AllRequestHistModel>();
            foreach (RequestHistories request in requested_mark_list) {
                AllRequestHistModel modal = new AllRequestHistModel(request);
                all_request_list.Add(modal);
            }
            return View(all_request_list);
        }

        [Authorize(Roles = "Admin")]
        public ActionResult AllRequestHistData(DTParameters param)
        {
            if (Request.IsAuthenticated)
            {
                try
                {
                    List<RequestHistories> dtsource = new List<RequestHistories>();//data source   
                    using (BOSEMEntities dc = new BOSEMEntities())
                    {
                        dtsource = dc.RequestHistories.OrderByDescending(m => m.request_date).ToList();
                    }  
                    List<String> columnSearch = new List<string>();

                    foreach (var col in param.Columns)
                    {
                        columnSearch.Add(col.Search.Value);
                    }
                    
                    List<RequestHistories> data = new ResultSet().GetAllRequestResult(param.Search.Value, param.SortOrder, param.Start, param.Length, dtsource, columnSearch);
                    
                    List<AllRequestHistModel> new_data = new List<AllRequestHistModel>();
                    foreach (RequestHistories row_data in data)
                    {
                        new_data.Add(new AllRequestHistModel(row_data));
                    }
                    
                    int count = new ResultSet().CountAllRequest(param.Search.Value, dtsource, columnSearch);
                       
                    DTResult<AllRequestHistModel> result = new DTResult<AllRequestHistModel>
                    {
                        draw = param.Draw,
                        data = new_data,
                        recordsFiltered = count,
                        recordsTotal = count
                    };
                    return Json(result);
                }
                catch (Exception ex)
                {
                    return Json(new { error = "Something is wrong --"+ex.Message });
                }
            }
            else
            {
                return RedirectToAction("Login", "Account");
            }
        }

        [Authorize(Roles = "Admin")]
        public ActionResult PaymentRate() {
            List<PaymentRates> rate = context.PaymentRates.OrderByDescending(m => m.order_date).ToList();
            ViewBag.old_amount = rate.Count == 0?0:rate[0].amount;
            return View();
        }
        /*Method for changing in payment rare*/
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RecordPaymentRate(RateModel rate) {
            rate.id = generateRandomString(26);
            rate.created_at = DateTime.Now;
            PaymentRates paymentRate = rate.ToPaymentRateModel();
            context.PaymentRates.Add(paymentRate);
            context.SaveChanges();
            List<PaymentRates> rate_list = context.PaymentRates.OrderByDescending(m => m.order_date).ToList();
            return Json(new { msg = "You have successfully set new amount.", list = rate_list });
        }

        public ActionResult GetPaymentRates() {
            List<PaymentRates> rate_list = context.PaymentRates.OrderByDescending(m=>m.created_at).ToList();
            return Json(new { msg="List of records of change in payment rate",list = rate_list});
        }

        protected string generateRandomString(int length)
        {
            string alphabets = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            string small_alphabets = "abcdefghijklmnopqrstuvwxyz";
            string numbers = "1234567890";
            string characters = numbers + alphabets + small_alphabets + numbers;
            string otp = string.Empty;

            for (int i = 0; i < length; i++)
            {
                string character = string.Empty;
                do
                {
                    int index = new Random().Next(0, characters.Length);
                    character = characters.ToCharArray()[index].ToString();
                } while (otp.IndexOf(character) != -1);
                otp += character;
            }
            return otp;
        }
    }
}