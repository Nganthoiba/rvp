using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace RVP.App_Start
{
    public class ValuesController : ApiController
    {
        [HttpGet]
        public string GET() {
            return "Hello World!";
        }
    }
}
