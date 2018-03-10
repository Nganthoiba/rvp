using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI.WebControls;

namespace RVP.Models
{
    public class ResultSet
    {
        /* For getting Request History in data table*/
        public List<requested_mark> GetResult(string search, string sortOrder, int start, int length, List<requested_mark> dtResult, List<string> columnFilters, string user_id)
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

        public int Count(string search, List<requested_mark> dtResult, List<string> columnFilters, string user_id)
        {
            return FilterResult(search, dtResult, columnFilters, user_id).Count();
        }

        private IQueryable<requested_mark> FilterResult(string search, List<requested_mark> dtResult, List<string> columnFilters, string user_id)
        {
            IQueryable<requested_mark> results = dtResult.AsQueryable();
            
            results = results.Where(p => (search == null || (p.request_date != null && p.request_date.ToString("ddd, dd MMM yyyy, hh:mm tt").Contains(search)
                || p.roll != null && p.roll.ToString().ToLower().Contains(search.ToLower()) || p.exam_year != null && p.exam_year.ToLower().Contains(search.ToLower()) || p.dob != null && p.dob.ToLower().Contains(search.ToLower()) || p.txn_id != null && p.txn_id.ToLower().Contains(search.ToLower())
                ))
                && p.user_id == user_id
                && p.payment_status == "paid"
                && (columnFilters[0] == null || (p.request_date != null && p.request_date.ToString("ddd, dd MMM yyyy, hh:mm tt").Contains(columnFilters[0])))
                && (columnFilters[1] == null || (p.roll != null && p.roll.ToString().ToLower().Contains(columnFilters[1].ToLower())))
                && (columnFilters[2] == null || (p.exam_year != null && p.exam_year.ToLower().Contains(columnFilters[2].ToLower())))
                && (columnFilters[3] == null || (p.dob != null && p.dob.ToLower().Contains(columnFilters[3].ToLower())))
                && (columnFilters[4] == null || (p.txn_id != null && p.txn_id.ToLower().Contains(columnFilters[4].ToLower())))

                );                
           
            return results;
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