using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RVP.Models
{
    public class SubjectYearCombinationViewModel
    {
        public int id { get; set; }//combination id
        public int sub_id { get; set; }// subject id
        public string subject_name { get; set; }// name of subject
        public Nullable<int> seq_cd { get; set; }// subject sequence code
        public string sub_type { get; set; }// subject type
        public Nullable<int> year { get; set; }//year
        public Nullable<bool> incl_in_total { get; set; }// whether include in grand total mark or not

        public SubjectYearCombinationViewModel() { }

        public SubjectYearCombinationViewModel(SubjectYearCombinations model) {
            if (model != null)
            {
                BOSEMEntities db = new BOSEMEntities();
                this.id = model.id;
                Subject sub = db.Subjects.Find(model.sub_id);
                this.subject_name = sub.name;
                this.seq_cd = sub.seq_cd;
                this.sub_type = sub.sub_type;
                this.sub_id = model.sub_id;
                this.year = model.year;
                this.incl_in_total = model.incl_in_total;
            }    
        }

        public static List<SubjectYearCombinationViewModel> ConvertToSubjectYearCombinationViewModel(List<SubjectYearCombinations> combinationList) {
            List<SubjectYearCombinationViewModel> subYearList = new List<SubjectYearCombinationViewModel>();
            if (combinationList != null && combinationList.Count() > 0) {
                foreach (SubjectYearCombinations item in combinationList) {
                    subYearList.Add(new SubjectYearCombinationViewModel(item));
                }
            }
            return subYearList;
        }
    }
}