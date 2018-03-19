using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RVP.Models
{
    /*Profile Image Class*/
    public class ProfileImage
    {
        /* User Id */
        public string user_id { get; set; }
        /*Image data base64 encoded format*/
        public string img_data { get; set; }
    }

    public class RemoveImage {
        public string user_id { get; set; }
    }
}