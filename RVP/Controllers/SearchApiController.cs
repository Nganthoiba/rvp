using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using RVP.Models;

namespace RVP.Controllers
{
    public class SearchApiController : ApiController
    {
        private BOSEMEntities db = new BOSEMEntities();
        HttpResponseMessage response;
        [HttpGet]
        public HttpResponseMessage hello()
        {
            response = Request.CreateResponse(HttpStatusCode.OK,
                                new
                                {
                                    message = "Hello! This is search api."
                                });
            return response;
        }
        [HttpGet]
        public HttpResponseMessage world()
        {
            response = Request.CreateResponse(HttpStatusCode.OK,
                                new
                                {
                                    message = "Hello! This is world."
                                });
            return response;
        }
    }
}
