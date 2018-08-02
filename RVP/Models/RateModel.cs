using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RVP.Models
{
    public partial class RateModel
    {
        public System.DateTime created_at { get; set; }
        public decimal amount { get; set; }
        public string order_no { get; set; }
        public string order_date { get; set; }
        public string created_by { get; set; }
        public string id { get; set; }
        public RateModel() { }
        public PaymentRates ToPaymentRateModel() {
            PaymentRates paymentRate = new PaymentRates();
            paymentRate.id = this.id;
            paymentRate.created_at = this.created_at;
            paymentRate.amount = this.amount;
            paymentRate.order_date = DateTime.ParseExact(this.order_date, "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture);
            paymentRate.order_no = this.order_no;
            paymentRate.created_by = this.created_by;

            return paymentRate;
        }

        public static decimal getCurrentRate() {
            BOSEMEntities db = new BOSEMEntities();
            List<PaymentRates> rates = db.PaymentRates.OrderByDescending(m => m.order_date).ToList();
            return rates.Count == 0 ? 0 : rates[0].amount;
        }
    }
}