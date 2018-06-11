using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using RVP.Models;
using System.Data.Entity.Infrastructure;
using System.Data.Entity;

namespace RVP.Controllers
{
    public class DashboardApiController : ApiController
    {
        private BOSEMEntities db = new BOSEMEntities();
        HttpResponseMessage response;
        [HttpGet]
        public HttpResponseMessage GetNoOfRequestHistories(int? id = null)
        {
            /* id will be taken as an indicator for year or month */
            if (id == 0 || id == null) //id=0 when monthly
            {
                List<NoOfRequestPerMonth> req_list = db.Database.SqlQuery<NoOfRequestPerMonth>("select MONTH(request_date) as Month,YEAR(request_date) as Year, count(*) as no_of_req" +
                    " from RequestHistories where payment_status='paid' group by MONTH(request_date),YEAR(request_date) order by MONTH(request_date),YEAR(request_date)").ToList();

                response = Request.CreateResponse(HttpStatusCode.OK,
                            new
                            {
                                Message = "Request successful.",
                                list = ViewNoOfRequestPerMonth.ConvertToViewList(req_list)
                            });
            }
            else if (id == 1)
            {// id=1 when yearly
                List<NoOfRequestPerYear> req_list = db.Database.SqlQuery<NoOfRequestPerYear>("select YEAR(request_date) as Year, count(*) as no_of_req" +
                    " from RequestHistories where payment_status='paid' group by YEAR(request_date) order by YEAR(request_date)").ToList();
                response = Request.CreateResponse(HttpStatusCode.OK,
                            new
                            {
                                Message = "Request successful.",
                                list = req_list
                            });
            }
            else
            {
                response = Request.CreateResponse(HttpStatusCode.BadRequest,
                            new
                            {
                                Message = "Invalid request."
                            });
            }
            return response;
        }

        [HttpGet]
        public HttpResponseMessage GetNoOfTransactions(int? id = null)
        {
            /* id will be taken as an indicator for year or month */
            if (id == 0 || id == null) //id=0 when monthly
            {
                List<NoOfTransactionPerMonth> req_list = db.Database.SqlQuery<NoOfTransactionPerMonth>("select MONTH(create_at) as Month,YEAR(create_at) as Year, count(*) as No_of_txn" +
                    " from TransactionHistory group by MONTH(create_at),YEAR(create_at) order by MONTH(create_at),YEAR(create_at)").ToList();

                response = Request.CreateResponse(HttpStatusCode.OK,
                            new
                            {
                                Message = "Request successful.",
                                list = ViewNoOfTransactionsPerMonth.ConvertToViewList(req_list)
                            });
            }
            else if (id == 1)
            {// id=1 when yearly
                List<NoOfTransactionPerYear> req_list = db.Database.SqlQuery<NoOfTransactionPerYear>("select YEAR(create_at) as year, count(*) as no_of_txn" +
                    " from TransactionHistory group by YEAR(create_at) order by YEAR(create_at)").ToList();
                response = Request.CreateResponse(HttpStatusCode.OK,
                            new
                            {
                                Message = "Request successful.",
                                list = req_list
                            });
            }
            else
            {
                response = Request.CreateResponse(HttpStatusCode.BadRequest,
                            new
                            {
                                Message = "Invalid request."
                            });
            }
            return response;
        }
    }
}
