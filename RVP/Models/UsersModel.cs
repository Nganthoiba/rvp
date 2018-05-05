using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace RVP.Models
{
    public class UsersModel
    {
        public string Id { get; set; }
        public string Email { get; set; }
        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; }
        [Display(Name = "Username")]
        public string UserName { get; set; }
        public string image { get; set; }
    }
}