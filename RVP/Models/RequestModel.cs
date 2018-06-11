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
        [Display(Name = "Roll Number")]
        public Nullable<int> roll { get; set; }
        [Display(Name = "Examination Year")]
        public string exam_year { get; set; }
        [Display(Name = "Date of Birth")]
        public string dob { get; set; }

        /*Default Constructor*/
        public RequestViewModel() { }

        public RequestViewModel(RequestHistories req)
        {
            this.id = req.id;
            this.request_date = req.request_date.ToString("ddd, dd MMM yyyy, hh:mm tt");
            //this.request_date = req.request_date.ToString("dd/MM/yyyy hh:mm:ss tt");          
            this.user_id = req.user_id;
            this.roll = req.roll;
            this.exam_year = req.exam_year.ToString();
            this.dob = req.dob;
        }
    }
    /*Model for viewing all request history (for Admin use only)*/
    public partial class AllRequestHistModel
    {
        public string id { get; set; } //Request Id
        [Display(Name = "Date of request")]
        public string request_date { get; set; }//Date and time of request
        public string user_id { get; set; } // The Id of the user who request student marksheet. (This 'user_id' is a foreign key that refers to the primary key 'Id' of the table 'AspNetUsers')
        [Display(Name = "User Name")]
        public string username { get; set; }

        [Display(Name = "Email")]
        public string email { get; set; }
        [Display(Name = "Payment Status")]
        public string payment_status { get; set; }//Payment Status that says whether payment has been completed or not. Only two possible:- 'paid' or 'unpaid'
        public int exam_result_id { get; set; }//this is the Marksheet Id. (Foreign key that refers to the 'Id' primary key of the 'hslc' table) 
        [Display(Name = "Roll No.")]
        public Nullable<int> roll { get; set; }// Student roll number
        [Display(Name = "Exam Year")]
        public string exam_year { get; set; }// Year of the exmination
        [Display(Name = "DOB")]
        public string dob { get; set; }// Student Date of birth
        [Display(Name = "Transaction ID")]
        public string txn_id { get; set; }// Transaction ID which will be set after successful payment

        public AllRequestHistModel() {

        }

        public AllRequestHistModel(RequestHistories req)
        {
            BOSEMEntities db = new BOSEMEntities();

            this.id = req.id;
            this.request_date = req.request_date.ToString("ddd, dd MMM yyyy, hh:mm tt");
            //this.request_date = req.request_date.ToString("dd/MM/yyyy hh:mm:ss tt");          
            this.user_id = req.user_id;
            AspNetUsers user = db.AspNetUsers.Find(req.user_id);
            this.username = user.UserName;
            this.email = user.Email;

            this.payment_status = req.payment_status.Trim().Equals("unpaid") ? "NOT PAID" : req.payment_status.ToUpper();
            this.roll = req.roll;
            this.exam_result_id = req.exam_result_id;
            this.exam_year = req.exam_year.ToString();
            this.dob = req.dob;
            this.txn_id = req.txn_id;
        }
    }

    /*Model for viewing history for requested student data*/
    public partial class RequestHistModel
    {
        public string id { get; set; } //Request Id
        public string request_date { get; set; }//Date and time of request
        public string user_id { get; set; } // The Id of the user who request student marksheet. (This 'user_id' is a foreign key that refers to the primary key 'Id' of the table 'AspNetUsers')
        public string payment_status { get; set; }//Payment Status that says whether payment has been completed or not. Only two possible:- 'paid' or 'unpaid'
        public int exam_result_id { get; set; }//this is the Marksheet Id. (Foreign key that refers to the 'Id' primary key of the 'hslc' table) 
        public Nullable<int> roll { get; set; }// Student roll number
        public Nullable<decimal> exam_year { get; set; }// Year of the exmination
        public string dob { get; set; }// Student Date of birth
        public string txn_id { get; set; }// Transaction ID which will be set after successful payment
        public string download_link { get; set; }// link for downloading student marksheet

        public RequestHistModel() { }
        //converting to RequestHistModel from requested_mark
        public RequestHistModel(RequestHistories req) {
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

            if (req.payment_status.Equals("error") || req.payment_status.Equals("unpaid") || req.txn_id == null || req.txn_id.Trim().Length == 0)
            {
                this.download_link = "Download Unavailable";
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

    /***** No of Requests *****/
    public class NoOfRequestPerMonth {
        public int Month { get; set; }
        public int Year { get; set; }
        public int No_of_req { get; set; } // Number of successful request
        
    }

    public class ViewNoOfRequestPerMonth{
        public string month { get; set; }
        public int no_of_req { get; set; } // Number of successful request
        public ViewNoOfRequestPerMonth(){}
        public ViewNoOfRequestPerMonth(NoOfRequestPerMonth req) {
            month = GetMonthName.getMonthName(req.Month) + " " + req.Year;
            no_of_req = req.No_of_req;
        }

        public static List<ViewNoOfRequestPerMonth> ConvertToViewList(List<NoOfRequestPerMonth> req_list) {
            List<ViewNoOfRequestPerMonth> list = new List<ViewNoOfRequestPerMonth>();
            foreach (NoOfRequestPerMonth row in req_list) {
                list.Add(new ViewNoOfRequestPerMonth(row));
            }
            return list;
        }
    }

    public class NoOfRequestPerYear
    {
        public int year { get; set; }
        public int no_of_req { get; set; } // Number of successful request
    }

    /***** Transaction ******/
    public class NoOfTransactionPerMonth
    {
        public int month { get; set; }
        public int year { get; set; }
        public int no_of_txn { get; set; } // Number of successful transaction per month
    }

    public class ViewNoOfTransactionsPerMonth {
        public string month { get; set; }
        public int no_of_txn { get; set; }

        public ViewNoOfTransactionsPerMonth() { }
        public ViewNoOfTransactionsPerMonth(NoOfTransactionPerMonth obj) {
            this.month = GetMonthName.getMonthName(obj.month) + " " + obj.year;
            this.no_of_txn = obj.no_of_txn;
        }

        public static List<ViewNoOfTransactionsPerMonth> ConvertToViewList(List<NoOfTransactionPerMonth> txn_list) {
            List<ViewNoOfTransactionsPerMonth> list = new List<ViewNoOfTransactionsPerMonth>();
            foreach(NoOfTransactionPerMonth row in txn_list)
            {
                list.Add(new ViewNoOfTransactionsPerMonth(row));
            }
            return list;
        }
    }

    public class NoOfTransactionPerYear
    {
        public int year { get; set; }
        public int no_of_txn { get; set; } // Number of successful transaction per year
    }

    public class GetMonthName
    {
        public static string getMonthName(int i) {
            string month = null;
            switch (i) {
                case 1: month = "JAN"; break;
                case 2: month = "FEB"; break;
                case 3: month = "MAR"; break;
                case 4: month = "APR"; break;
                case 5: month = "MAY"; break;
                case 6: month = "JUN"; break;
                case 7: month = "JUL"; break;
                case 8: month = "AUG"; break;
                case 9: month = "SEP"; break;
                case 10: month = "OCT"; break;
                case 11: month = "NOV"; break;
                case 12: month = "DEC"; break;
            }
            return month;
        }
    }

}