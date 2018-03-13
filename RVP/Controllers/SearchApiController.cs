using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using RVP.Models;

namespace RVP.Controllers
{
    //test class
    public class test {
        public string name { get; set; }
    }

    public class img {
        public string image { get; set; }
    }
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

        [HttpPost]
        public HttpResponseMessage world(test t)
        {
            response = Request.CreateResponse(HttpStatusCode.OK,
                                new
                                {
                                    message = "Hello! You have sent: "+t.name
                                });
            return response;
        }

        [HttpPost]
        public HttpResponseMessage cropped_img(img i)
        {
            response = Request.CreateResponse(HttpStatusCode.OK,
                                new
                                {
                                    message = "Hello! You have sent: " + i.image
                                });
            return response;
        }
    }
}
