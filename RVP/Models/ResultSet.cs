using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI.WebControls;

namespace RVP.Models
{
    public class ResultSet
    {
        /* For getting Request History in data table*/
        public List<RequestHistories> GetResult(string search, string sortOrder, int start, int length, List<RequestHistories> dtResult, List<string> columnFilters, string user_id)
        {
            if (sortOrder.Equals("download_link"))
            {
                return FilterResult(search, dtResult, columnFilters, user_id).Skip(start).Take(length).ToList();
            }
            else
            {
                return FilterResult(search, dtResult, columnFilters, user_id).SortBy(sortOrder).Skip(start).Take(length).ToList();

            }
        }

        public int Count(string search, List<RequestHistories> dtResult, List<string> columnFilters, string user_id)
        {
            return FilterResult(search, dtResult, columnFilters, user_id).Count();
        }

        private IQueryable<RequestHistories> FilterResult(string search, List<RequestHistories> dtResult, List<string> columnFilters, string user_id)
        {
            IQueryable<RequestHistories> results = dtResult.AsQueryable();

            results = results.Where(p => (search == null  || 
                (
                   p.request_date != null && p.request_date.ToString("ddd, dd MMM yyyy, hh:mm tt").Contains(search)
                || p.roll.ToString().ToLower().Contains(search.ToLower()) 
                || p.exam_year != null && p.exam_year.ToString().ToLower().Contains(search.ToLower()) 
                || p.dob != null && p.dob.ToLower().Contains(search.ToLower()) 
                || p.txn_id != null && p.txn_id.ToLower().Contains(search.ToLower())
                
                ))

                && p.user_id == user_id
                && p.payment_status != "unpaid"
                && (columnFilters[0] == null || (p.request_date != null && p.request_date.ToString("ddd, dd MMM yyyy, hh:mm tt").Contains(columnFilters[0])))
                && (columnFilters[1] == null || (p.roll.ToString().ToLower().Contains(columnFilters[1].ToLower())))
                && (columnFilters[2] == null || (p.exam_year != null && p.exam_year.ToString().ToLower().Contains(columnFilters[2].ToLower())))
                && (columnFilters[3] == null || (p.dob != null && p.dob.ToLower().Contains(columnFilters[3].ToLower())))
                && (columnFilters[4] == null || (p.txn_id != null && p.txn_id.ToLower().Contains(columnFilters[4].ToLower())))
                
                );                
           
            return results;
        }

        
        
        
        /* For getting All Request History in data table. This is for Admin User only*/
        public List<RequestHistories> GetAllRequestResult(string search, string sortOrder, int start, int length, List<RequestHistories> dtResult, List<string> columnFilters)
        { 
           return FilterAllRequestResult(search, dtResult, columnFilters).SortBy(sortOrder).Skip(start).Take(length).ToList();  
        }

        public int CountAllRequest(string search, List<RequestHistories> dtResult, List<string> columnFilters)
        {
            return FilterAllRequestResult(search, dtResult, columnFilters).Count();
        }

        private IQueryable<RequestHistories> FilterAllRequestResult(string search, List<RequestHistories> dtResult, List<string> columnFilters)
        {
            IQueryable<RequestHistories> results = dtResult.AsQueryable();
            string status = columnFilters[5]!=null?( columnFilters[5].ToLower().Equals("not paid")?"unpaid": columnFilters[5].ToLower()):null;

            results = results.Where(p => (search == null || (
                   ( p.request_date != null && p.request_date.ToString("ddd, dd MMM yyyy, hh:mm tt").Contains(search))
                || (p.roll.ToString().ToLower().Contains(search.ToLower())) 
                || (p.exam_year != null && p.exam_year.ToString().ToLower().Contains(search.ToLower())) 
                || (p.dob != null && p.dob.ToLower().Contains(search.ToLower())) 
                || (p.txn_id != null && p.txn_id.ToLower().Contains(search.ToLower())) 
                || (p.payment_status!=null && p.payment_status.ToLower().Equals(payment_status(search)))
                || (p.email != null && p.email.ToLower().Contains(search.ToLower()))
                ))
                && (columnFilters[0] == null || (p.request_date != null && p.request_date.ToString("ddd, dd MMM yyyy, hh:mm tt").Contains(columnFilters[0])))
                && (columnFilters[1] == null || (p.roll.ToString().ToLower().Contains(columnFilters[1].ToLower())))
                && (columnFilters[2] == null || (p.exam_year != null && p.exam_year.ToString().ToLower().Contains(columnFilters[2].ToLower())))
                && (columnFilters[3] == null || (p.dob != null && p.dob.ToLower().Contains(columnFilters[3].ToLower())))
                && (columnFilters[4] == null || (p.txn_id != null && p.txn_id.ToLower().Contains(columnFilters[4].ToLower())))
                && (columnFilters[5] == null || (p.payment_status != null && p.payment_status.ToLower().Equals(status.ToLower())))
                && (columnFilters[6] == null || (p.email != null && p.email.ToLower().Contains(columnFilters[6].ToLower())))
                );

            return results;
        }
        private string payment_status(string search) {
            if (search.Trim().ToLower().Equals("not paid"))
                return "unpaid";
            else {
                return search.Trim().ToLower();
            }
        }




        /* For getting Payment History in data table*/
        public List<TransactionHistory> GetTxnResult(string search, string sortOrder, int start, int length, List<TransactionHistory> dtResult, List<string> columnFilters)
        {  
            return TxnFilterResult(search, dtResult, columnFilters).SortBy(sortOrder).Skip(start).Take(length).ToList();
        }

        public int TxnCount(string search, List<TransactionHistory> dtResult, List<string> columnFilters)
        {
            return TxnFilterResult(search, dtResult, columnFilters).Count();
        }

        /*Transaction or payment history result*/
        private IQueryable<TransactionHistory> TxnFilterResult(string search, List<TransactionHistory> dtResult, List<string> columnFilters)
        {
            IQueryable<TransactionHistory> results = dtResult.AsQueryable();

            results = results.Where(p => (search == null || (p.create_at != null && p.create_at.ToString("ddd, dd MMM yyyy, hh:mm tt").Contains(search)
                || p.txn_id != null && p.txn_id.ToString().ToLower().Contains(search.ToLower()) || p.status != null && p.status.ToLower().Contains(search.ToLower()) 
                || p.amount != null && p.amount.ToString().Contains(search)
                ))
                
                && (columnFilters[0] == null || (p.create_at != null && p.create_at.ToString("ddd, dd MMM yyyy, hh:mm tt").Contains(columnFilters[0])))
                && (columnFilters[1] == null || (p.txn_id != null && p.txn_id.ToString().ToLower().Contains(columnFilters[1].ToLower())))
                && (columnFilters[2] == null || (p.status != null && p.status.ToLower().Contains(columnFilters[2].ToLower())))
                && (columnFilters[3] == null || (p.amount != null && p.amount.ToString().Contains(columnFilters[3])))
                
           );

            return results;
        }
    }
}