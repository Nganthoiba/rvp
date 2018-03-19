using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using RVP.Models;
using System.Configuration;
using Microsoft.AspNet.Identity;
using System.Net;
using System.Text;
using System.IO;
using System.Globalization;

namespace RVP.Controllers
{
    public class PaypalController : Controller
    {
        // GET: Paypal
        private BOSEMEntities db = new BOSEMEntities();
        public ActionResult RedirectFromPaypal()
        {
            if (Request.IsAuthenticated)
            {
                string user_id = User.Identity.GetUserId();
                if (isValidRedirection())
                {
                    try
                    {
                        TransactionHistory txn_hist = new TransactionHistory();
                        
                        // Transaction verification form
                        var formVals = new Dictionary<string, string>();
                        formVals.Add("cmd", "_notify-synch"); //notify-synch_notify-validate
                        formVals.Add("at", Convert.ToString(ConfigurationManager.AppSettings["IdentityToken"])); // this has to be adjusted
                        formVals.Add("tx", Request["tx"]);

                        bool useSandbox = Convert.ToBoolean(ConfigurationManager.AppSettings["IsSandbox"]);

                        /* 
                        GetPayPalResponse will verify whether the transaction details extracted from 
                        the return url are correct or not. 
                        */
                        string response = GetPayPalResponse(formVals, useSandbox);

                        if (response.Contains("SUCCESS"))
                        {

                            txn_hist.txn_id = Request.QueryString["tx"]; 
                            txn_hist.amount = decimal.Parse(GetPDTValue(response, "mc_gross"));
                            txn_hist.status = GetPDTValue(response, "payment_status");
                            txn_hist.create_at = DateTime.Now;

                            //txn_hist.amount = decimal.Parse(Request.QueryString["amt"]);
                            //txn_hist.status = Request.QueryString["st"];
                            //DateTime paymentDate;
                            //DateTime.TryParseExact(HttpUtility.UrlDecode(GetPDTValue(response, "payment_date")), "HH:mm:ss MMM dd, yyyy PST", CultureInfo.InvariantCulture, DateTimeStyles.None, out paymentDate);
                            //txn_hist.create_at = paymentDate;
                            

                            if (db.TransactionHistories.Any(m => m.txn_id == txn_hist.txn_id))
                            {
                                ViewBag.error = 1;
                                ViewBag.PaymentResult = "Transaction already exist.";  
                            }
                            else
                            {
                                /****Adding transaction record*******/
                                db.TransactionHistories.Add(txn_hist);
                                db.SaveChanges();

                                /***** Verifying whether the user has paid the right amount *****/
                                int no_of_item = db.requested_mark.Where(x => x.user_id == user_id && x.payment_status == "unpaid").Count();
                                int amt_per_item = Convert.ToInt32(ConfigurationManager.AppSettings["amt_per_unit"]);//amount payable in each request for verification
                                decimal total = no_of_item * amt_per_item;//actual amount payable by the payer
                                /*If the user has not paid the right enough amount i.e. if the amount paid by the user is less than the actual amount payable, 
                                then the status will be set to 'error', and hence he/she will not be allowed to download the marksheet. */
                                string payment_status = (txn_hist.status.Equals("Completed")) ?(txn_hist.amount < total ?"error": "paid"): "unpaid";
                                
                                /***** updating payment status in requested_mark table *******/
                                db.Database.ExecuteSqlCommand("update requested_mark set txn_id='" + txn_hist.txn_id + "', payment_status='" + payment_status + "' where payment_status='unpaid' and user_id='" + user_id + "'");
                                ViewBag.PaymentResult = "";
                                ViewBag.error = 0;
                                ViewBag.Transaction = txn_hist;

                                List<requested_mark> txn_list = db.requested_mark.Where(m=>m.txn_id == txn_hist.txn_id && m.payment_status=="paid").ToList();
                                List<RequestHistModel> req_list = new List<RequestHistModel>();
                                foreach (requested_mark row in txn_list) {
                                    req_list.Add(new RequestHistModel(row));
                                }
                                if (txn_hist.amount < total)
                                {
                                    ViewBag.error = 1;// error exists
                                    ViewBag.warning = "Since, you have paid an amount of Rs " + txn_hist.amount + " which is less than the actual payable amount of Rs " + total +
                                        ", you won't be allowed to download marksheet. Please consult with administrator to cancel the order and start from the beginning.";
                                }
                                else {
                                    ViewBag.error = 0;//no error
                                    ViewBag.warning = "";//no warning
                                }
                                return View(req_list);
                            }
                            
                        }
                        else if (response.Contains("FAIL")) {
                            ViewBag.error = 1;
                            ViewBag.PaymentResult = "Wrong transaction record....";
                        }
                        else
                        {
                            ViewBag.error = 1;
                            ViewBag.PaymentResult = "Something went wrong...: " + response;
                        }
                        //just for printing whatever in the response
                        //ViewBag.PaymentResult = response;
                    }
                    catch (Exception ex)
                    {
                        ViewBag.error = 1;
                        ViewBag.PaymentResult = "An error occurs: " + ex.ToString();
                    }
                }
                else
                {
                    ViewBag.error = 1;
                    ViewBag.PaymentResult = "Invalid redirection.";
                }
            }
            else
            {
                return RedirectToAction("Login", "Account");
            }
            return View();
        }

        private Boolean isValidRedirection() {
            if (Request.QueryString["tx"] == null || Request.QueryString["tx"].Equals(""))
                return false;
            if (Request.QueryString["st"] == null || Request.QueryString["st"].Equals(""))
                return false;
            if (Request.QueryString["amt"] == null || Request.QueryString["amt"].Equals(""))
                return false;
            return true;
        }
        public ActionResult CancelFromPaypal()
        {
            return View();
        }
        public ActionResult NotifyFromPaypal()
        {
            return View();
        }
        [ValidateAntiForgeryToken]
        public ActionResult ValidateCommand(string totalPrice)
        {
            bool useSandbox = Convert.ToBoolean(ConfigurationManager.AppSettings["IsSandbox"]);
            var paypal = new PaypalModel(useSandbox);
            paypal.item_name = "marksheet";
            paypal.amount = totalPrice;
            return View(paypal);
        }
        private string GetPDTValue(string pdt, string key)
        {

            string[] keys = pdt.Split('\n');
            string thisVal = "";
            string thisKey = "";
            foreach (string s in keys)
            {
                string[] bits = s.Split('=');
                if (bits.Length > 1)
                {
                    thisVal = bits[1];
                    thisKey = bits[0];
                    if (thisKey.Equals(key, StringComparison.InvariantCultureIgnoreCase))
                        break;
                }
            }
            return thisVal;

        }
        private string GetPayPalResponse(Dictionary<string, string> formVals, bool useSandbox)
        {
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            string paypalUrl = useSandbox ? "https://www.sandbox.paypal.com/cgi-bin/webscr"
                : "https://www.paypal.com/cgi-bin/webscr";

            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(paypalUrl);

            // Set values for the request back
            req.Method = "POST";
            req.ContentType = "application/x-www-form-urlencoded";

            byte[] param = Request.BinaryRead(Request.ContentLength);
            string strRequest = Encoding.ASCII.GetString(param);

            StringBuilder sb = new StringBuilder();
            sb.Append(strRequest);

            foreach (string key in formVals.Keys)
            {
                sb.AppendFormat("&{0}={1}", key, formVals[key]);
            }
            strRequest += sb.ToString();
            req.ContentLength = strRequest.Length;

            //for proxy
            //WebProxy proxy = new WebProxy(new Uri("http://urlort#");
            //req.Proxy = proxy;
            //Send the request to PayPal and get the response
            string response = "";
            using (StreamWriter streamOut = new StreamWriter(req.GetRequestStream(), System.Text.Encoding.ASCII))
            {

                streamOut.Write(strRequest);
                streamOut.Close();
                using (StreamReader streamIn = new StreamReader(req.GetResponse().GetResponseStream()))
                {
                    response = streamIn.ReadToEnd();
                    streamIn.Close();
                }
            }

            return response;
        }
    }
}