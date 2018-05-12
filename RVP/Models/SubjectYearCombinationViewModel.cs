using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RVP.Models
{
    public class SubjectYearCombinationViewModel
    {
        public int id { get; set; }
        public int sub_id { get; set; }
        public string subject_name { get; set; }
        public Nullable<int> year { get; set; }
        public Nullable<bool> incl_in_total { get; set; }

        public SubjectYearCombinationViewModel() { }

        public SubjectYearCombinationViewModel(SubjectYearCombinations model) {
            if (model != null)
            {
                BOSEMEntities db = new BOSEMEntities();
                this.id = model.id;
                Subject sub = db.Subjects.Find(model.sub_id);
                this.subject_name = sub.name;
                this.sub_id = model.sub_id;
                this.year = model.year;
                this.incl_in_total = model.incl_in_total;
            }    
        }
    }
}