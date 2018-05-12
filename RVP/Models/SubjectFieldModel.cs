using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RVP.Models
{
    public class SubjectFieldModel
    {
        public string id { get; set; }
        public int sub_year_id { get; set; }
        public string field_name { get; set; }
        public string field_meaning { get; set; }
        public Nullable<int> pass_mark { get; set; }
        public Nullable<int> full_mark { get; set; }
        public int field_seq { get; set; }

        public SubjectFieldModel() { }
        public SubjectFieldModel(SubjectFields field) {
            this.id = field.id;
            this.sub_year_id = field.sub_year_id;
            this.field_name = field.field_name;
            this.field_meaning = field.field_meaning;
            this.pass_mark = field.pass_mark;
            this.full_mark = field.full_mark;
            this.field_seq = field.field_seq;
        }

        public SubjectFields getSubjectField() {
            SubjectFields fields = new SubjectFields();
            fields.id = this.id;
            fields.sub_year_id = this.sub_year_id;
            fields.field_name = this.field_name;
            fields.field_meaning = this.field_meaning;
            fields.pass_mark = this.pass_mark;
            fields.full_mark = this.full_mark;
            fields.field_seq = this.field_seq;
            return fields;
        }

        
    }
}