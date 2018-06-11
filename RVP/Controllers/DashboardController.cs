using RVP.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace RVP.Controllers
{
    public class DashboardController : Controller
    {
        private BOSEMEntities db = new BOSEMEntities();
        // GET: Dashboard
        public ActionResult Index()
        {
            ViewBag.no_of_users = db.AspNetUsers.Count();
            ViewBag.no_of_req = db.RequestHistories.Where(m=>m.payment_status=="paid").Count();
            ViewBag.no_of_txn = db.TransactionHistories.Count();
            return View();
        }
    }
}