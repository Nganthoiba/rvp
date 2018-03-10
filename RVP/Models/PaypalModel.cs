using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Configuration;

namespace RVP.Models
{
    public class PaypalModel
    {
        public string cmd { get; set; }
        public string business { get; set; }
        public string no_shipping { get; set; }
        public string @return { get; set; }
        public string cancel_return { get; set; }
        public string notify_url { get; set; }
        public string currency_code { get; set; }//currency 
        public string item_name { get; set; }//item name
        public int? item_number { get; set; }//number of items
        public string amount { get; set; }// total amount
        public string actionURL { get; set; }

        public PaypalModel(bool useSandbox)
        {
            this.cmd = "_xclick";
            this.item_number = 1;
            this.business = ConfigurationManager.AppSettings["business"];
            this.cancel_return = ConfigurationManager.AppSettings["cancel_return"];
            this.@return = ConfigurationManager.AppSettings["return"];
            if (useSandbox)
            {
                this.actionURL = ConfigurationManager.AppSettings["test_url"];
            }
            else
            {
                this.actionURL = ConfigurationManager.AppSettings["Prod_url"];
            }
            // We can add parameters here, for example OrderId, CustomerId, etc….
            this.notify_url = ConfigurationManager.AppSettings["notify_url"];
            // We can add parameters here, for example OrderId, CustomerId, etc….
            this.currency_code = ConfigurationManager.AppSettings["currency_code"];
        }
    }
}
