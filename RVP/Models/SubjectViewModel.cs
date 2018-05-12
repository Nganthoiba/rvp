using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace RVP.Models
{
    public class SubjectViewModel
    {
        [Required]
        [Display(Name = "Subject Code")]
        public int sub_code { get; set; }

        [Required]
        [Display(Name = "Subject Name")]
        public string name { get; set; }

        [Required]
        [Display(Name = "Abbrevation")]
        public string abbrevation { get; set; }

        [Required]
        [Display(Name = "Sequence Code")]
        public Nullable<int> seq_cd { get; set; }

        [Required]
        [Display(Name = "Subject Type")]
        public string sub_type { get; set; }

        [Key]
        public int id { get; set; }

        public SubjectViewModel() { }

        public SubjectViewModel(Subject sub) {
            this.id = sub.id;
            this.sub_code = sub.sub_code;
            this.name = sub.name;
            this.seq_cd = sub.seq_cd;
            this.sub_type = sub.sub_type;
            this.abbrevation = sub.abbrevation;

        }

        public static List<SubjectViewModel> GetModelList(List<Subject> subjects) {
            List<SubjectViewModel> modelList = new List<SubjectViewModel>();
            if (subjects != null) {
                foreach (Subject sub in subjects) {
                    modelList.Add(new SubjectViewModel(sub));
                }
            }
            return modelList;
        }
    }
}