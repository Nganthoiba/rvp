using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RVP.Models
{
    /*public class MarksheetModels
    {
    }*/

    /*marksheet model for the years from 2015 to 2018*/   
    
    public class MarkModel
    {
        public string subject { get; set; }
        public List<FieldModel> fields { get; set; }
        public string sub_type { get; set; }
    }

    public class FieldModel
    {
        public string field_name { get; set; }
        public decimal? full_mark { get; set; }
        public decimal? pass_mark { get; set; }
        public decimal? scored_mark { get; set; }
    }

    public class Sub_taken//subjects taken 
    {
        public string sub_name { get; set; }
        public string abbr { get; set; }
        //public string sub_field { get; set; }
        public int seq_cd { get; set; }
        //public int no_of_fields { get; set; }
        public string sub_type { get; set; }
        public int sub_year_id { get; set; }
    }
}