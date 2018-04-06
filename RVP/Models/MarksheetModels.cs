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

    public class MarksheetModel
    {
        public string sub_name { get; set; }//name of the subject
        public decimal? ext_full_mark { get; set; }//external full mark
        public decimal? ext_pass_mark { get; set; }//external pass mark
        public decimal? int_full_mark { get; set; }//internal full mark
        public decimal? int_pass_mark { get; set; }//internal pass mark
        public decimal? ext_scored_mark { get; set; }//external scored mark
        public decimal? int_scored_mark { get; set; }//internal scored mark
        public decimal? grace_mark { get; set; }//grace mark
        public decimal? total { get; set; }
        public MarksheetModel() { }
        public MarksheetModel(string sub_name, decimal? ext_fm, decimal? ext_pm, decimal? int_fm, decimal? int_pm, decimal? ext_sc_mark, decimal? int_sc_mark, decimal? grace_mark, decimal? total)
        {
            this.sub_name = sub_name == null ? "No Sub" : sub_name;
            this.ext_full_mark = ext_fm == null ? 0 : ext_fm;
            this.ext_pass_mark = ext_pm == null ? 0 : ext_pm;
            this.int_full_mark = int_fm == null ? 0 : int_fm;
            this.int_pass_mark = int_pm == null ? 0 : int_pm;
            this.int_scored_mark = int_sc_mark == null ? 0 : int_sc_mark;
            this.ext_scored_mark = ext_sc_mark == null ? 0 : ext_sc_mark;
            this.grace_mark = grace_mark == null ? 0 : grace_mark;
            this.total = total == null ? 0 : total;
        }
    }

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
        public string sub_field { get; set; }
        public int seq_cd { get; set; }
        public int no_of_fields { get; set; }
        public string sub_type { get; set; }
    }
}