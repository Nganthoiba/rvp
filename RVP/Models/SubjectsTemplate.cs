//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace RVP.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class SubjectsTemplate
    {
        public int year { get; set; }
        public string sub_name { get; set; }
        public string sub_fields { get; set; }
        public string field_meaning { get; set; }
        public string sub_type { get; set; }
        public int seq_cd { get; set; }
        public Nullable<decimal> pass_mark { get; set; }
        public Nullable<decimal> full_mark { get; set; }
        public Nullable<int> include_in_total { get; set; }
    }
}