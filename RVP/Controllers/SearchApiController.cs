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

    public class Subjects
    {
        public string sub_name { get; set; }
        public int seq_cd { get; set; }
        public int no_of_fields { get; set; }
    }

    public class ResSet
    {
        public string sub_name { get; set; }
        public int seq_cd { get; set; }
        public List<SubjectsTemplate> fields { get; set; }
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

        [HttpGet]
        public HttpResponseMessage get_subjects(int id)
        {
            int year = id;
            List<Subjects> res = db.Database.SqlQuery<Subjects>("SELECT DISTINCT sub_name,seq_cd,count(*) as no_of_fields FROM SubjectsTemplate WHERE year="+year+ " GROUP BY sub_name,seq_cd ORDER BY seq_cd").ToList();
            List<SubjectsTemplate> subject_fields;
            List<ResSet> resset = new List<ResSet>();
            List<MarksheetModel> compul_subs = new List<MarksheetModel>();
            foreach (var row in res)
            {
                MarksheetModel compul_sub = new MarksheetModel();
                subject_fields = db.SubjectsTemplate.Where(x => x.year == year && x.sub_name == row.sub_name).OrderBy(x => x.seq_cd).ToList();
                
                ResSet rr = new ResSet();
                rr.sub_name = row.sub_name;
                rr.seq_cd = row.seq_cd;
                rr.fields = subject_fields;
                resset.Add(rr);
            }

            response = Request.CreateResponse(HttpStatusCode.OK,
                                new
                                {
                                    message = "Request granted.",
                                    Results = resset
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
