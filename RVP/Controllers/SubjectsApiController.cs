using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using RVP.Models;
using System.Data.Entity.Infrastructure;
using System.Data.Entity;

namespace RVP.Controllers
{
    public class SubjectsApiController : ApiController
    {
        private BOSEMEntities db = new BOSEMEntities();
        HttpResponseMessage response;

        /****** APIs for Subject Year Combination *******/

        [HttpGet]
        public HttpResponseMessage GET(int? id=null)
        {
            try
            {
                if (id != null)
                {
                    
                    SubjectYearCombinationViewModel req_list = db.SubjectYearCombinations.Find(id)==null?null:new SubjectYearCombinationViewModel(db.SubjectYearCombinations.Find(id));
                    response = Request.CreateResponse(HttpStatusCode.OK,
                                new
                                {
                                    Message = "Request successful.",
                                    list = req_list,
                                    subjects = SubjectViewModel.GetModelList(db.Subjects.OrderBy(m => m.name).ToList())
                                });
                }
                else
                {
                    List<SubjectYearCombinations> req_list = db.SubjectYearCombinations.OrderBy(m=>m.year).ToList();
                    response = Request.CreateResponse(HttpStatusCode.OK,
                                new
                                {
                                    Message = "Request successful.",
                                    list = SubjectYearCombinationViewModel.ConvertToSubjectYearCombinationViewModel(req_list),
                                    subjects = SubjectViewModel.GetModelList(db.Subjects.OrderBy(m => m.name).ToList())
                                });
                }
            }
            catch (Exception exception)
            {
                response = Request.CreateResponse(HttpStatusCode.InternalServerError,new { Message = "An error occur " + exception.ToString() });
            }
            return response;
        }

        // POST: api/SubjectYearCombinationsApi
        /*[ResponseType(typeof(SubjectYearCombinations))]*/
        [HttpPost]
        public HttpResponseMessage PostSubjectYearCombinations(SubjectYearCombinations subjectYearCombinations)
        {

            if (!ModelState.IsValid)
            {
                response = Request.CreateResponse(HttpStatusCode.BadRequest, "Bad Request: Improper Data Passed");
            }
            else
            {

                if (db.SubjectYearCombinations.Any(m => m.sub_id == subjectYearCombinations.sub_id && m.year == subjectYearCombinations.year))
                {
                    response = Request.CreateResponse(HttpStatusCode.OK,
                           new
                           {
                               message = "You have already added.",
                               list = SubjectYearCombinationViewModel.ConvertToSubjectYearCombinationViewModel(db.SubjectYearCombinations.Where(m => m.year == subjectYearCombinations.year).ToList()),
                               subjects = SubjectViewModel.GetModelList(db.Subjects.OrderBy(m => m.name).ToList())
                           });
                }
                else
                {
                    db.SubjectYearCombinations.Add(subjectYearCombinations);
                    try
                    {
                        db.SaveChanges();
                    }
                    catch (DbUpdateException exception)
                    {
                        if (SubjectYearCombinationsExists(subjectYearCombinations.id))
                        {
                            response = Request.CreateResponse(HttpStatusCode.Conflict,
                           new
                           {
                               message = "Conflict occurs.",
                               list = SubjectYearCombinationViewModel.ConvertToSubjectYearCombinationViewModel(db.SubjectYearCombinations.Where(m => m.year == subjectYearCombinations.year).ToList()),
                               subjects = SubjectViewModel.GetModelList(db.Subjects.OrderBy(m => m.name).ToList())
                           });

                        }
                        else
                        {
                            response = Request.CreateResponse(HttpStatusCode.InternalServerError, new
                            {
                                message = "Failed to update." + exception.GetBaseException()
                            });
                        }
                    }
                    response = Request.CreateResponse(HttpStatusCode.OK,
                           new
                           {
                               message = "You have successfully added.",
                               list = SubjectYearCombinationViewModel.ConvertToSubjectYearCombinationViewModel(db.SubjectYearCombinations.Where(m => m.year == subjectYearCombinations.year).ToList()),
                               subjects = SubjectViewModel.GetModelList(db.Subjects.OrderBy(m => m.name).ToList())
                           });
                }
            }

            return response;
        }

        [HttpPut]
        public HttpResponseMessage PutSubjectYearCombinations(SubjectYearCombinations subjectYearCombinations)
        {

            if (!ModelState.IsValid)
            {
                response = Request.CreateResponse(HttpStatusCode.BadRequest, new { Message = "Bad Request" });
            }
            else
            {

                if (db.SubjectYearCombinations.Any(m => m.sub_id == subjectYearCombinations.sub_id && m.year == subjectYearCombinations.year && m.incl_in_total==subjectYearCombinations.incl_in_total))
                {
                    response = Request.CreateResponse(HttpStatusCode.OK,
                           new
                           {
                               Message = "Duplicate found. The same combination of subject and year already exist before.",
                               list = SubjectYearCombinationViewModel.ConvertToSubjectYearCombinationViewModel(db.SubjectYearCombinations.Where(m => m.year == subjectYearCombinations.year).OrderBy(m => m.year).ToList()),
                               subjects = SubjectViewModel.GetModelList(db.Subjects.OrderBy(m => m.name).ToList())
                           });
                }
                else
                {
                    db.Entry(subjectYearCombinations).State = EntityState.Modified;  
                    try
                    {
                        db.SaveChanges();
                    }
                    catch (DbUpdateException exception)
                    {
                        if (SubjectYearCombinationsExists(subjectYearCombinations.id))
                        {
                            response = Request.CreateResponse(HttpStatusCode.Conflict,
                           new
                           {
                               Message = "Conflict occurs.",
                               list = SubjectYearCombinationViewModel.ConvertToSubjectYearCombinationViewModel(db.SubjectYearCombinations.Where(m => m.year == subjectYearCombinations.year).OrderBy(m => m.year).ToList()),
                               subjects = SubjectViewModel.GetModelList(db.Subjects.OrderBy(m => m.name).ToList())
                           });

                        }
                        else
                        {
                            response = Request.CreateResponse(HttpStatusCode.InternalServerError, new
                            {
                                Message = "Failed to update." + exception.GetBaseException()
                            });
                        }
                    }
                    response = Request.CreateResponse(HttpStatusCode.OK,
                           new
                           {
                               Message = "You have successfully updated.",
                               list = SubjectYearCombinationViewModel.ConvertToSubjectYearCombinationViewModel(db.SubjectYearCombinations.Where(m => m.year == subjectYearCombinations.year).OrderBy(m => m.year).ToList()),
                               subjects = SubjectViewModel.GetModelList(db.Subjects.OrderBy(m => m.name).ToList())
                           });
                }
            }

            return response;
        }

        [HttpDelete]
        public HttpResponseMessage DeleteSubjectYearCombinations(int id)
        {
            if (!ModelState.IsValid)
            {
                response = Request.CreateResponse(HttpStatusCode.BadRequest, new { Message = "Bad Request" });
            }
            else if(SubjectYearCombinationsExists(id))
            {
                SubjectYearCombinations subjectYearCombinations = db.SubjectYearCombinations.Find(id);
                db.SubjectYearCombinations.Remove(subjectYearCombinations);
                try
                {
                    db.SaveChanges();
                    response = Request.CreateResponse(HttpStatusCode.OK, new { Message = "Successfully removed.",
                        list = SubjectYearCombinationViewModel.ConvertToSubjectYearCombinationViewModel(db.SubjectYearCombinations.Where(m => m.year == subjectYearCombinations.year).OrderBy(m => m.year).ToList()),
                        subjects = SubjectViewModel.GetModelList(db.Subjects.OrderBy(m => m.name).ToList())
                    });
                }
                catch (Exception exception) {
                    response = Request.CreateResponse(HttpStatusCode.InternalServerError, new { Message = "An error has occured."+exception.GetBaseException() });
                }
            }
            return response;
        }

        private bool SubjectYearCombinationsExists(int id)
        {
            return db.SubjectYearCombinations.Count(e => e.id == id) > 0;
        }

        /*
        private List<SubjectYearCombinationViewModel> GetSubjectsYearCombinationList(List<SubjectYearCombinations> combination_list) {
            List<SubjectYearCombinationViewModel> list = new List<SubjectYearCombinationViewModel>();
            foreach (SubjectYearCombinations item in combination_list) {   
                list.Add(new SubjectYearCombinationViewModel(item));
            }
            return list;
        }
        */

        /***** APIs for Subject Fields *****/
        [HttpGet]// for getting subject fields
        public HttpResponseMessage GetSubjectFields(int? id=null) {
            if (!ModelState.IsValid || id==null)
            {
                response = Request.CreateResponse(HttpStatusCode.BadRequest, new { message = "Bad Request: Improper Data Passed" });
            }
            else{
                List<SubjectFields> fields = db.SubjectFields.Where(m => m.sub_year_id == id).OrderBy(m=>m.field_seq).ToList();
                List<SubjectFieldModel> model_fields = new List<SubjectFieldModel>();
                foreach(SubjectFields field in fields)
                {
                    model_fields.Add(new SubjectFieldModel(field));
                }
                response = Request.CreateResponse(HttpStatusCode.OK, new { message = "List of Subject fields", list = model_fields });
            }
            return response;
        }

        [HttpPost]// for inserting new subject fields
        public HttpResponseMessage PostSubjectFields(SubjectFieldModel model) {
            if (!ModelState.IsValid)
            {
                response = Request.CreateResponse(HttpStatusCode.BadRequest,new { message = "Bad Request" });
            }
            else
            {
                if (db.SubjectFields.Any(x => x.sub_year_id == model.sub_year_id && x.field_name == model.field_name))
                {
                    response = Request.CreateResponse(HttpStatusCode.Conflict, new { message = "Duplicate Record Found!" });
                }
                else {
                    model.id = generateRandomString(26);
                    db.SubjectFields.Add(model.getSubjectField());
                    try
                    {
                        db.SaveChanges();
                        List<SubjectFields> fields = db.SubjectFields.Where(m => m.sub_year_id == model.sub_year_id).OrderBy(m => m.field_seq).ToList();
                        List<SubjectFieldModel> model_fields = new List<SubjectFieldModel>();
                        foreach (SubjectFields field in fields)
                        {
                            model_fields.Add(new SubjectFieldModel(field));
                        }

                        SubjectYearCombinationViewModel sub_year_combination = new SubjectYearCombinationViewModel(db.SubjectYearCombinations.Find(model.sub_year_id));
                        
                        response = Request.CreateResponse(HttpStatusCode.OK, new { message = "Saved successfully",list = model_fields,sub_year_combination=sub_year_combination });
                    }
                    catch(Exception exp)
                    {
                        response = Request.CreateResponse(HttpStatusCode.InternalServerError, new { message = "Error in saving "+exp.GetBaseException() });
                    }
                }
            }
            return response;
        }
        //
        [HttpPut]// For updating subject fields
        public HttpResponseMessage PutSubjectFields(string id, SubjectFieldModel model)
        {
            if (!ModelState.IsValid || id==null || model==null || model.id==null || (id!=model.id))
            {
                response = Request.CreateResponse(HttpStatusCode.BadRequest, new { Message = "Bad Request" });
            }
            else {
                SubjectFields subjectFields = model.getSubjectField();
                db.Entry(subjectFields).State = EntityState.Modified;
                try
                {
                    db.SaveChanges();
                    
                    List<SubjectFields> fields = db.SubjectFields.Where(m => m.sub_year_id == subjectFields.sub_year_id).OrderBy(m => m.field_seq).ToList();
                    List<SubjectFieldModel> model_fields = new List<SubjectFieldModel>();
                    foreach (SubjectFields field in fields)
                    {
                        model_fields.Add(new SubjectFieldModel(field));
                    }

                    response = Request.CreateResponse(HttpStatusCode.OK, new { Message = "Saved successfully",list = model_fields });
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SubjectFieldsExists(id))
                    {
                        response = Request.CreateResponse(HttpStatusCode.NotFound, new { Message = "Subject field not found." });
                        
                    }
                    else
                    {
                        response = Request.CreateResponse(HttpStatusCode.InternalServerError, new { Message = "An error occurs while updating." });
                    }
                }
            }
            return response;
        }

        [HttpDelete]// For deleting subject field
        public HttpResponseMessage DeleteSubjectFields(string id) {
            if (!ModelState.IsValid || id == null)
            {
                response = Request.CreateResponse(HttpStatusCode.BadRequest, new { Message = "Bad Request" });
            }
            else
            {
                SubjectFields subjectFields = db.SubjectFields.Find(id);
                if (subjectFields == null)
                {
                    response = Request.CreateResponse(HttpStatusCode.NotFound, new { Message = "Not Found" });
                }
                else {
                    db.SubjectFields.Remove(subjectFields);
                    db.SaveChanges();

                    List<SubjectFields> fields = db.SubjectFields.Where(m => m.sub_year_id == subjectFields.sub_year_id).OrderBy(m => m.field_seq).ToList();
                    List<SubjectFieldModel> model_fields = new List<SubjectFieldModel>();
                    foreach (SubjectFields field in fields)
                    {
                        model_fields.Add(new SubjectFieldModel(field));
                    }

                    response = Request.CreateResponse(HttpStatusCode.OK, new { Message = "Successfully deleted.", list = model_fields });
                }
            }
            return response;
        }





        [HttpOptions]
        protected string generateRandomString(int length)
        {
            string alphabets = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            string small_alphabets = "abcdefghijklmnopqrstuvwxyz";
            string numbers = "1234567890";
            string characters = numbers + alphabets + small_alphabets + numbers;
            string otp = string.Empty;

            for (int i = 0; i < length; i++)
            {
                string character = string.Empty;
                do
                {
                    int index = new Random().Next(0, characters.Length);
                    character = characters.ToCharArray()[index].ToString();
                } while (otp.IndexOf(character) != -1);
                otp += character;
            }
            return otp;
        }
        private bool SubjectFieldsExists(string id)
        {
            return db.SubjectFields.Count(e => e.id == id) > 0;
        }
    }
}
