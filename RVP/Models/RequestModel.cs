using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace RVP.Models
{
    public class RequestModel
    {
        public int roll { get; set; }
        public string exam_year { get; set; }
        public string dob { get; set; }
        public string user_id { get; set; }
    }

    public class DeleteRequestModel
    {
        public string id { get; set; }
        public string user_id { get; set; }
    }

    
    public partial class RequestViewModel
    {
        public string id { get; set; }
        [Display(Name = "Date & Time")]
        public string request_date { get; set; }
        public string user_id { get; set; }
        [Display(Name ="Roll Number")]
        public Nullable<int> roll { get; set; }
        [Display(Name = "Examination Year")]
        public string exam_year { get; set; }
        [Display(Name = "Date of Birth")]
        public string dob { get; set; }
        

        public RequestViewModel() { }

        public RequestViewModel(requested_mark req)
        {
            this.id = req.id;
            this.request_date = req.request_date.ToString("ddd, dd MMM yyyy, hh:mm tt");
            //this.request_date = req.request_date.ToString("dd/MM/yyyy hh:mm:ss tt");          
            this.user_id = req.user_id;
            this.roll = req.roll;
            this.exam_year = req.exam_year;
            this.dob = req.dob;
        }
    }

    /*Model for viewing requested student data*/
    public partial class RequestHistModel
    {
        public string id { get; set; } //Request Id
        public string request_date { get; set; }//Date and time of request
        public string user_id { get; set; } // The Id of the user who request student marksheet. (This 'user_id' is a foreign key that refers to the primary key 'Id' of the table 'AspNetUsers')
        public string payment_status { get; set; }//Payment Status that says whether payment has been completed or not. Only two possible:- 'paid' or 'unpaid'
        public int exam_result_id { get; set; }//this is the Marksheet Id. (Foreign key that refers to the 'Id' primary key of the 'hslc' table) 
        public Nullable<int> roll { get; set; }// Student roll number
        public string exam_year { get; set; }// Year of the exmination
        public string dob { get; set; }// Student Date of birth
        public string txn_id { get; set; }// Transaction ID which will be set after successful payment
        public string download_link { get; set; }// link for downloading student marksheet

        public RequestHistModel() { }
        //converting to RequestHistModel from requested_mark
        public RequestHistModel(requested_mark req) {
            this.id = req.id;
            this.request_date = req.request_date.ToString("ddd, dd MMM yyyy, hh:mm tt");
            //this.request_date = req.request_date.ToString("dd/MM/yyyy hh:mm:ss tt");          
            this.user_id = req.user_id;
            this.payment_status = req.payment_status;
            this.roll = req.roll;
            this.exam_result_id = req.exam_result_id;
            this.exam_year = req.exam_year;
            this.dob = req.dob;
            this.txn_id = req.txn_id;
            
            if (req.payment_status.Equals("unpaid") || req.txn_id == null || req.txn_id.Trim().Length == 0)
            {
                this.download_link = "No download available";
            }
            else {
                string link = "/Request/GenerateMarksheet/" + req.exam_result_id;
                this.download_link = "<a href='" + link + "' target='_blank'>Download</a>";
            }
        }
    }
    /*Transaction History View Model*/
    /*This model is used to display in PaymentHist view of Request controller*/
    public partial class TxnHistViewModel
    {
        public string txn_id { get; set; }
        public string status { get; set; }
        public Nullable<decimal> amount { get; set; }
        public string create_at { get; set; }
        public TxnHistViewModel() {
            //empty constructor
        }
        public TxnHistViewModel(TransactionHistory txn)
        {
            this.txn_id = txn.txn_id;
            this.status = txn.status;
            this.amount = txn.amount;
            //this.create_at = txn.create_at.ToString("dd/MM/yyyy hh:mm:ss tt");
            this.create_at = txn.create_at.ToString("ddd, dd MMM yyyy, hh:mm tt");       
        }
    }
}