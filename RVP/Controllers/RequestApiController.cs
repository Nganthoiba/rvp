using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using RVP.Models;

namespace RVP.Controllers
{
    public class RequestApiController : ApiController
    {
        private BOSEMEntities db = new BOSEMEntities();
        HttpResponseMessage response;

        [HttpGet]
        public HttpResponseMessage GET(string id = null)
        {
            try {
                if (id != null) {
                    List<RequestHistories> req_list = db.RequestHistories.Where(m => m.payment_status == "unpaid" && m.user_id == id).ToList();
                    response = Request.CreateResponse(HttpStatusCode.OK,
                                new
                                {
                                    message = "Request successful.",
                                    req_list = req_list
                                });
                }
                else
                {
                    List<RequestHistories> req_list = db.RequestHistories.Where(m=>m.payment_status=="unpaid").ToList();
                    response = Request.CreateResponse(HttpStatusCode.OK,
                                new
                                {
                                    message = "Request successful.",
                                    req_list = req_list
                                });
                }
            }catch(Exception exception)
            {
                response = Request.CreateResponse(HttpStatusCode.InternalServerError,"An error occur "+exception.ToString());
            }
            return response;
        }


        [HttpPost]
        public HttpResponseMessage Post(RequestModel req)
        {
            if (!ModelState.IsValid)
            {
                response = Request.CreateResponse(HttpStatusCode.BadRequest, "Bad Request: Improper Data Passed" );
            }
            else
            {                
                hslc exam_result = db.hslcs.Where(x => x.roll == req.roll && x.exm_year.ToString() == req.exam_year && x.dob == req.dob).FirstOrDefault();
                if (exam_result != null)
                {
                    //int exam_res_id = db.Database.SqlQuery<int>("select id from students where roll_no='" + req.roll_no + "' and year='" + req.year + "' and dob='" + req.dob + "'").FirstOrDefault();
                    if (db.requested_mark.Any(m => m.exam_result_id == exam_result.id && m.user_id == req.user_id))
                    {
                        response = Request.CreateResponse(HttpStatusCode.Conflict, "Duplicate request found! You have already requested the same. Please check in the request history.");
                    }
                    else
                    {
                        requested_mark requested_mark = new requested_mark();

                        requested_mark.request_date = DateTime.Now;
                        requested_mark.payment_status = "unpaid";
                        requested_mark.exam_result_id = exam_result.id;
                        requested_mark.user_id = req.user_id;
                        requested_mark.id = generateRandomString(26);
                        requested_mark.txn_id = null;

                        db.requested_mark.Add(requested_mark);
                        db.SaveChanges();
                        response = Request.CreateResponse(HttpStatusCode.OK,
                            new
                            {
                                message = "Marksheet has been requested, please click proceed button for payment.",
                                req_list = db.RequestHistories.Where(x => x.user_id == req.user_id && x.payment_status == "unpaid").ToList()
                            });
                    }

                }
                else
                {
                    response = Request.CreateResponse(HttpStatusCode.NotFound, "No record found for the input specification.");
                }
            }
            return response;
        }
        [HttpOptions]
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

        [HttpDelete]
        public HttpResponseMessage delete(DeleteRequestModel req) {
            if (req != null) {
                requested_mark req_mark = db.requested_mark.Find(req.id);
                if (req_mark == null)
                {
                    response = Request.CreateResponse(HttpStatusCode.NotFound, "Not Found");
                }
                else
                {
                    db.requested_mark.Remove(req_mark);
                    db.SaveChanges();
                    response = Request.CreateResponse(HttpStatusCode.OK,
                                    new
                                    {
                                        message = "Request deleted",
                                        req_list = db.requested_mark.Where(x => x.user_id == req.user_id && x.payment_status=="unpaid").ToList()
                                    });
                }
            }
            else
            {
                response = Request.CreateResponse(HttpStatusCode.BadRequest, "Bad Request empty parameter" );
            }
            return response;
        }
    }
}