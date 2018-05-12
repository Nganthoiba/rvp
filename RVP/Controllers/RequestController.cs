using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using RVP.Models;
using System.Configuration;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.Reflection;
using System.Data;
using System.Data.SqlClient;
using Microsoft.AspNet.Identity.EntityFramework;

namespace RVP.Controllers
{
    
    public class RequestController : Controller
    {
        private BOSEMEntities db = new BOSEMEntities();
        public SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString);
        private ApplicationDbContext context = new ApplicationDbContext();

        public Boolean isAdminUser()
        {
            if (User.Identity.IsAuthenticated)
            {
                var user = User.Identity;

                var UserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context));
                var s = UserManager.GetRoles(user.GetUserId());
                if (s[0].ToString() == "Admin")
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            return false;
        }

        [Authorize(Roles = "Other")]
        public ActionResult Index() {
            if (!Request.IsAuthenticated) {
                return RedirectToAction("Login", "Account");
            }
            return View();
        }
        [Authorize(Roles = "Other")]
        // GET: Request
        public ActionResult RequestMark()
        {
            if (Request.IsAuthenticated)
            {
                return View();
            }      
            else
                return RedirectToAction("Login", "Account");
        }
        [Authorize(Roles = "Other")]
        public ActionResult RequestHistory() {
            if (Request.IsAuthenticated)
            {
                string user_id = User.Identity.GetUserId();
                
                List<requested_mark> res = db.requested_mark.SqlQuery("select TOP 10 * from requested_mark where user_id='"+user_id+"' and payment_status!='unpaid' order by request_date desc").ToList();
                ViewBag.count = res.Count();
                List<RequestHistModel> new_data = new List<RequestHistModel>();
                foreach (requested_mark row_data in res)
                {
                    new_data.Add(new RequestHistModel(row_data));
                }
                return View(new_data);
            }
            else
            {
                return RedirectToAction("Login", "Account");
            }
            
        }
        [Authorize(Roles = "Other")]
        public ActionResult GenerateInvoice() {
            
            if (Request.IsAuthenticated)
            {
                string user_id = User.Identity.GetUserId();
                //Request List
                List<requested_mark> request_list = db.requested_mark.Where(x => x.user_id == user_id && x.payment_status == "unpaid").OrderByDescending(m=>m.request_date).ToList();
                
                //Number of request
                ViewBag.no_of_item = request_list.Count();

                ViewBag.amt_per_item = Convert.ToInt32(ConfigurationManager.AppSettings["amt_per_unit"]);//amount payable in each request for verification
                ViewBag.total = ViewBag.no_of_item * ViewBag.amt_per_item;//Total amount payable
                
                List<RequestViewModel> invoice_list = new List<RequestViewModel>();
                foreach (requested_mark row in request_list) {
                    invoice_list.Add(new RequestViewModel(row));
                }
                return View(invoice_list);
            }
            else
            {
                return RedirectToAction("Login", "Account");
            }
        }
        [Authorize(Roles = "Other,Admin")]
        public ActionResult PaymentHist() {
            if (Request.IsAuthenticated)
            {
                string user_id = User.Identity.GetUserId();
                List<TransactionHistory> txn_list = new List<TransactionHistory>();
                if (!isAdminUser())
                {
                    txn_list = db.Database.SqlQuery<TransactionHistory>("select * from TransactionHistory where txn_id in (" +
                        "select distinct txn_id from requested_mark where txn_id is not NULL and user_id = '" + user_id + "'" +
                        ") order by create_at desc").OrderByDescending(m => m.create_at).ToList();
                    ViewBag.is_admin = "no";

                    ViewBag.TotalTxn = txn_list.Count();
                    decimal? sum = 0;
                    foreach (TransactionHistory txn_hist in txn_list) {
                        sum += txn_hist.amount;
                    }

                    ViewBag.TotalTxnAmount = sum;
                }
                else {
                    txn_list = db.Database.SqlQuery<TransactionHistory>("select TOP 10 * from TransactionHistory where txn_id in (" +
                        "select distinct txn_id from requested_mark where txn_id is not NULL" +
                        ") order by create_at desc").OrderByDescending(m => m.create_at).ToList();
                    ViewBag.is_admin = "yes";

                    ViewBag.TotalTxn = db.TransactionHistories.ToList().Count();
                    ViewBag.TotalTxnAmount = db.TransactionHistories.Sum(m=>m.amount);
                }
                ViewBag.count = txn_list.Count();
                return View(txn_list);
            }
            else
            {
                return RedirectToAction("Login", "Account");
            }
        }
        [Authorize(Roles = "Other")]
        public ActionResult GenerateMarksheet(int? id=null)
        {
            if (!Request.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }
            else
            {
                string user_id = User.Identity.GetUserId();
                if (id == null)
                {
                    ViewBag.ErrorMsg = "Invalid Request";
                    return View("Error");
                }
                else
                {
                    hslc result = db.hslcs.Find(id);
                    if (result != null)
                    {
                        requested_mark req_mark = db.requested_mark.Where(x => x.payment_status == "paid" && x.user_id == user_id && x.exam_result_id == result.id).FirstOrDefault();
                        if (req_mark == null)
                        {
                            ViewBag.ErrorMsg = "Please complete your payment.";
                            return View("Error");
                        }
                        else
                        {
                            decimal? year = result.exm_year;
                            if (year != null && year >= 2016)
                            {
                                //generate_2016_model(result); 
                                generate_marksheet(result);
                            }
                            else if (year >= 2010 && year <= 2015) {
                                generate_2010_to_2015_model(result);
                            }
                            else if (year>=2004 && year<=2009) {
                                generate_2004_to_2009_model(result);
                            }
                            else
                            {
                                ViewBag.ErrorMsg = "Under development." + year;
                                return View("Error");
                            }
                            
                        }
                    }
                    else
                    {
                        ViewBag.ErrorMsg = "Requested record does not exist in our database.";
                        return View("Error");
                    }
                }
            }
            return RedirectToAction("GenerateInvoice");
        }
        [Authorize(Roles = "Other")]
        /*Marksheet format from the year 2016*/
        public void generate_marksheet(hslc res)
        {
            string name = res.name.ToUpper();
            string roll_no = res.roll.ToString();
            string year = res.exm_year.ToString();
            string dob = res.dob;

            //setting template pdf file path
            //string TemplateFile = "http://localhost:15059/Content/files/format_new.pdf";
            string TemplateFile = Convert.ToString(ConfigurationManager.AppSettings["template_path"]);
            // open the reader
            PdfReader reader = new PdfReader(TemplateFile);
            iTextSharp.text.Rectangle size = reader.GetPageSizeWithRotation(1);

            //Pdf Stamper
            PdfStamper stamper = new PdfStamper(reader, Response.OutputStream);
            // Modifying the pdf content
            PdfContentByte cb = stamper.GetOverContent(1);
            // select the font properties
            BaseFont bf = BaseFont.CreateFont(BaseFont.TIMES_ROMAN, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
            cb.SetColorFill(BaseColor.DARK_GRAY);
            cb.SetFontAndSize(bf, 12);

            // write the text in the pdf content
            cb.BeginText();
            // put the alignment and coordinates here
            cb.ShowTextAligned(Element.ALIGN_LEFT, name, 90, 647, 0);
            cb.EndText();

            cb.BeginText();
            // put the alignment and coordinates here
            cb.ShowTextAligned(Element.ALIGN_LEFT, roll_no, 120, 627, 0);
            cb.EndText();

            cb.BeginText();
            // put the alignment and coordinates here
            cb.ShowTextAligned(Element.ALIGN_LEFT, res.father_name, 200, 608, 0);
            cb.EndText();

            cb.BeginText();
            // put the alignment and coordinates here
            cb.ShowTextAligned(Element.ALIGN_LEFT, res.mother_name, 150, 589, 0);
            cb.EndText();

            cb.BeginText();
            // put the alignment and coordinates here
            cb.ShowTextAligned(2, dob, 180, 569, 0);
            cb.EndText();

            cb.BeginText();
            // put the alignment and coordinates here
            cb.ShowTextAligned(Element.ALIGN_LEFT, year, 80, 529, 0);

            // put the alignment and coordinates here
            cb.ShowTextAligned(Element.ALIGN_LEFT, res.school.ToUpper(), 170, 529, 0);
            cb.EndText();

            PdfContentByte marks_print = stamper.GetOverContent(1);
            BaseFont basefont = BaseFont.CreateFont(BaseFont.TIMES_ROMAN, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
            cb.SetColorFill(BaseColor.DARK_GRAY);
            marks_print.SetFontAndSize(basefont, 11);

            //Getting subject wise marks
            
            //finding the list of subjects pescribed by the BoSEM for the year
            List<MarkModel> subs_inc_total = get_marks(res, true);//subjects included in grand total
            List<MarkModel> subs_not_inc_total = get_marks(res, false);//subjects not included in grand total

            int i = 0;//iteration
            int height = 450;//height
            int width = 40;//width

            for (;i<subs_inc_total.Count();i++) {
                marks_print.BeginText();
                // printing paper name
                marks_print.ShowTextAligned(Element.ALIGN_LEFT, (i + 1) + ". " + subs_inc_total.ElementAt(i).subject.ToUpper(), width + 10, height, 0);
                List<FieldModel> fields = subs_inc_total.ElementAt(i).fields;
                if (fields.Count() == 1) {
                    marks_print.ShowTextAligned(Element.ALIGN_RIGHT, fields.ElementAt(0).pass_mark.ToString(), width + 350, height, 0);
                    marks_print.ShowTextAligned(Element.ALIGN_RIGHT, fields.ElementAt(0).full_mark.ToString(), width + 405, height, 0);
                    marks_print.ShowTextAligned(Element.ALIGN_RIGHT, fields.ElementAt(0).scored_mark.ToString(), width + 490, height, 0);
                    height = height - 20;
                }
                else
                {
                    foreach(var field in fields)
                    {
                        marks_print.ShowTextAligned(Element.ALIGN_LEFT, field.field_name, width + 200, height, 0);
                        marks_print.ShowTextAligned(Element.ALIGN_RIGHT, field.pass_mark.ToString(), width + 350, height, 0);
                        marks_print.ShowTextAligned(Element.ALIGN_RIGHT, field.full_mark.ToString(), width + 405, height, 0);
                        if (field.field_name.Equals("TOTAL"))
                        {
                            marks_print.ShowTextAligned(Element.ALIGN_RIGHT, field.scored_mark.ToString(), width + 490, height, 0);
                        }
                        else
                        {
                            marks_print.ShowTextAligned(Element.ALIGN_RIGHT, field.scored_mark.ToString(), width + 460, height, 0);
                        }
                        height = height - 13;
                    }
                    height -= 7;
                }
                marks_print.EndText();
            }
                        
            /* PRINTING MARKSHEET */       
            // drawing a line
            /***************************/
            height += 15;
            /*** Dash Line ***/
            cb.MoveTo(45, height);
            cb.SetLineDash(5, 2, 0);
            cb.LineTo(size.Width - 45, height);
            cb.Stroke();
            /*****************/
            //Grand Total
            marks_print.BeginText();
            marks_print.ShowTextAligned(Element.ALIGN_LEFT, "TOTAL:", width + 10, height - 13, 0);
            marks_print.ShowTextAligned(Element.ALIGN_RIGHT, res.gtotal + "", width + 490, height - 13, 0);
            marks_print.EndText();

            /*** Dash Line ***/
            cb.MoveTo(45, height - 18);
            cb.SetLineDash(5, 2, 0);
            cb.LineTo(size.Width - 45, height - 18);
            cb.Stroke();
            /*****************/
            /**** printing marks which are not includedin total ****/
            height = height - 35;
            
            for (int j=0; j < subs_not_inc_total.Count(); j++)
            {
                marks_print.BeginText();
                // printing paper name
                marks_print.ShowTextAligned(Element.ALIGN_LEFT, (++i) + ". " + subs_not_inc_total.ElementAt(j).subject.ToUpper(), width + 10, height, 0);
                List<FieldModel> fields = subs_not_inc_total.ElementAt(j).fields;
                if (fields.Count() == 1)
                {
                    marks_print.ShowTextAligned(Element.ALIGN_RIGHT, fields.ElementAt(0).pass_mark.ToString(), width + 350, height, 0);
                    marks_print.ShowTextAligned(Element.ALIGN_RIGHT, fields.ElementAt(0).full_mark.ToString(), width + 405, height, 0);
                    marks_print.ShowTextAligned(Element.ALIGN_RIGHT, fields.ElementAt(0).scored_mark.ToString(), width + 490, height, 0);
                    height = height - 20;
                }
                else
                {
                    foreach (var field in fields)
                    {
                        marks_print.ShowTextAligned(Element.ALIGN_LEFT, field.field_name, width + 200, height, 0);
                        marks_print.ShowTextAligned(Element.ALIGN_RIGHT, field.pass_mark.ToString(), width + 350, height, 0);
                        marks_print.ShowTextAligned(Element.ALIGN_RIGHT, field.full_mark.ToString(), width + 405, height, 0);
                        if (field.field_name.Equals("TOTAL"))
                        {
                            marks_print.ShowTextAligned(Element.ALIGN_RIGHT, field.scored_mark.ToString(), width + 490, height, 0);
                        }
                        else
                        {
                            marks_print.ShowTextAligned(Element.ALIGN_RIGHT, field.scored_mark.ToString(), width + 460, height, 0);
                        }
                        height = height - 13;
                    }
                    height -= 7;
                }
                marks_print.EndText();
            }
            // print division
            string result = "";
            switch (res.divi)
            {
                case 1: result = "First"; break;
                case 2: result = "Second"; break;
                case 3: result = "Third"; break;
                case 4: result = "Simple Passed"; break;
                case 5: result = "Failed"; break;
                case 6: result = "Expelled"; break;
            }

            cb.BeginText();
            cb.SetFontAndSize(bf, 12);
            // put the alignment and coordinates here
            cb.ShowTextAligned(Element.ALIGN_LEFT, result, 185, 507, 0);
            cb.EndText();

            stamper.Close();
            reader.Close();
            Response.Buffer = true;
            Response.ContentType = "application/pdf";
            Response.AddHeader("content-disposition", "attachment;filename=" + roll_no + year + ".pdf");
            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            Response.Write(stamper);
            Response.End();
        }

        public List<MarkModel> get_marks(hslc res,Boolean incl_in_tot) {
            /*incl_in_tot: subjects to be included in grand total or not*/
            int include_in_total = incl_in_tot == true ? 1 : 0;
            List<Sub_taken> sub_taken_list = db.Database.SqlQuery<Sub_taken>("SELECT DISTINCT sub_name,seq_cd,count(*) as no_of_fields,sub_type FROM SubjectsTemplate WHERE year=" + res.exm_year + " and include_in_total="+ include_in_total+ " GROUP BY sub_name,seq_cd,sub_type ORDER BY seq_cd").ToList();
            List<SubjectsTemplate> subject_template_list;
            List<MarkModel> mark_list = new List<MarkModel>();

            /*** database connection ****/
            con.Open();
            string query = "select * from hslc where id="+res.id;
            SqlCommand cmd = new SqlCommand(query, con);
            cmd.CommandType = CommandType.Text;
            SqlDataReader reader = cmd.ExecuteReader();
            reader.Read();
            

            /*******************************/
            //for every subject taken by the student
            foreach (var item in sub_taken_list)
            {
                subject_template_list = db.SubjectsTemplate.Where(x => x.year == res.exm_year && x.sub_name == item.sub_name).OrderBy(x => x.field_meaning).ThenBy( x=> x.seq_cd).ToList();

                MarkModel mark = new MarkModel();
                Type classType = res.GetType();
                //PropertyInfo propertyInfo;
                string field_name;

                if (item.sub_type.Trim().Equals("a") || item.sub_type.Trim().Equals("f"))
                {
                    //propertyInfo = classType.GetProperty(item.sub_name);
                    field_name = reader[item.sub_name.Trim()].ToString();//(string)propertyInfo.GetValue(res, null);
                    mark.subject = get_sub_name(field_name);
                }
                else
                {
                    mark.subject = item.sub_name;
                }
                mark.sub_type = item.sub_type.Trim();
                List<FieldModel> fields = new List<FieldModel>();

                foreach (var field in subject_template_list)
                {
                    //propertyInfo = classType.GetProperty(field.sub_fields);
                    FieldModel fieldModel = new FieldModel();
                    fieldModel.field_name = field.field_meaning.Trim();
                    fieldModel.pass_mark = field.pass_mark;
                    fieldModel.full_mark = field.full_mark;
                    
                    //fieldModel.scored_mark = Convert.ToDecimal(propertyInfo.GetValue(res, null));
                    
                    fieldModel.scored_mark = Convert.ToDecimal(reader[field.sub_fields.Trim()]);
                    fields.Add(fieldModel);
                }
                mark.fields = fields;
                mark_list.Add(mark);
            }

            con.Close();// closing connection
            return mark_list;
        }

        public List<MarkModel> get_subjects(hslc res, Boolean incl_in_tot)
        {
            /*incl_in_tot: subjects to be included in grand total or not*/
            int include_in_total = incl_in_tot == true ? 1 : 0;











            List<Sub_taken> sub_taken_list = db.Database.SqlQuery<Sub_taken>("SELECT DISTINCT sub_name,seq_cd,count(*) as no_of_fields,sub_type FROM SubjectsTemplate WHERE year=" + res.exm_year + " and include_in_total=" + include_in_total + " GROUP BY sub_name,seq_cd,sub_type ORDER BY seq_cd").ToList();
            List<SubjectsTemplate> subject_template_list;
            List<MarkModel> mark_list = new List<MarkModel>();

            /*** database connection ****/
            con.Open();
            string query = "select * from hslc where id=" + res.id;
            SqlCommand cmd = new SqlCommand(query, con);
            cmd.CommandType = CommandType.Text;
            SqlDataReader reader = cmd.ExecuteReader();
            reader.Read();


            /*******************************/
            //for every subject taken by the student
            foreach (var item in sub_taken_list)
            {
                subject_template_list = db.SubjectsTemplate.Where(x => x.year == res.exm_year && x.sub_name == item.sub_name).OrderBy(x => x.field_meaning).ThenBy(x => x.seq_cd).ToList();

                MarkModel mark = new MarkModel();
                Type classType = res.GetType();
                //PropertyInfo propertyInfo;
                string field_name;

                if (item.sub_type.Trim().Equals("a") || item.sub_type.Trim().Equals("f"))
                {
                    //propertyInfo = classType.GetProperty(item.sub_name);
                    field_name = reader[item.sub_name.Trim()].ToString();//(string)propertyInfo.GetValue(res, null);
                    mark.subject = get_sub_name(field_name);
                }
                else
                {
                    mark.subject = item.sub_name;
                }
                mark.sub_type = item.sub_type.Trim();
                List<FieldModel> fields = new List<FieldModel>();

                foreach (var field in subject_template_list)
                {
                    //propertyInfo = classType.GetProperty(field.sub_fields);
                    FieldModel fieldModel = new FieldModel();
                    fieldModel.field_name = field.field_meaning.Trim();
                    fieldModel.pass_mark = field.pass_mark;
                    fieldModel.full_mark = field.full_mark;

                    //fieldModel.scored_mark = Convert.ToDecimal(propertyInfo.GetValue(res, null));

                    fieldModel.scored_mark = Convert.ToDecimal(reader[field.sub_fields.Trim()]);
                    fields.Add(fieldModel);
                }
                mark.fields = fields;
                mark_list.Add(mark);
            }

            con.Close();// closing connection
            return mark_list;
        }

        [Authorize(Roles = "Other")]
        /*Marksheet format from the year 2016*/
        public void generate_2016_model(hslc res)
        {
            string name = res.name.ToUpper();
            string roll_no = res.roll.ToString();
            string year = res.exm_year.ToString();
            string dob = res.dob;

            //setting template pdf file path
            //string TemplateFile = "http://localhost:15059/Content/files/format_new.pdf";
            string TemplateFile = Convert.ToString(ConfigurationManager.AppSettings["template_path"]);
            // open the reader
            PdfReader reader = new PdfReader(TemplateFile);
            iTextSharp.text.Rectangle size = reader.GetPageSizeWithRotation(1);

            //Pdf Stamper
            PdfStamper stamper = new PdfStamper(reader, Response.OutputStream);
            // Modifying the pdf content
            PdfContentByte cb = stamper.GetOverContent(1);
            // select the font properties
            BaseFont bf = BaseFont.CreateFont(BaseFont.HELVETICA, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
            cb.SetColorFill(BaseColor.DARK_GRAY);
            cb.SetFontAndSize(bf, 12);

            // write the text in the pdf content
            cb.BeginText();
            // put the alignment and coordinates here
            cb.ShowTextAligned(Element.ALIGN_LEFT, name, 90, 647, 0);
            cb.EndText();

            cb.BeginText();
            // put the alignment and coordinates here
            cb.ShowTextAligned(Element.ALIGN_LEFT, roll_no, 120, 627, 0);
            cb.EndText();

            cb.BeginText();
            // put the alignment and coordinates here
            cb.ShowTextAligned(Element.ALIGN_LEFT, res.father_name, 200, 608, 0);
            cb.EndText();

            cb.BeginText();
            // put the alignment and coordinates here
            cb.ShowTextAligned(Element.ALIGN_LEFT, res.mother_name, 150, 589, 0);
            cb.EndText();

            cb.BeginText();
            // put the alignment and coordinates here
            cb.ShowTextAligned(2, dob, 180, 569, 0);
            cb.EndText();

            cb.BeginText();
            // put the alignment and coordinates here
            cb.ShowTextAligned(Element.ALIGN_LEFT, year, 80, 529, 0);

            // put the alignment and coordinates here
            cb.ShowTextAligned(Element.ALIGN_LEFT, res.school.ToUpper(), 170, 529, 0);
            cb.EndText();

            PdfContentByte marks_print = stamper.GetOverContent(1);
            BaseFont basefont = BaseFont.CreateFont(BaseFont.TIMES_ROMAN, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
            cb.SetColorFill(BaseColor.DARK_GRAY);
            marks_print.SetFontAndSize(basefont, 11);

            //Getting subject wise marks
            
            MarksheetModel[] compulsory_subjects = {
                new MarksheetModel(get_sub_name(res.opt1), 80, 20, 20, 8, res.mil_ext, res.mil_int,res.lg,res.mil_total),
                new MarksheetModel("ENGLISH", 80, 20, 20, 8, res.eng_ext, res.eng_int, res.eg,res.eng_total),
                new MarksheetModel("MATHMETICS", 80, 20, 20, 8, res.math_ext, res.math_int,res.mg,res.math_total),
                new MarksheetModel("SCIENCE", 80, 20, 20, 8, res.sc_ext, res.sc_int,res.scg,res.sc_total),
                new MarksheetModel("SOCIAL SCIENCE", 80, 20, 20, 8, res.ssc_ext, res.ssc_int,res.ssg,res.ssc_total),
                new MarksheetModel(get_sub_name(res.asub), 80, 20, 20, 8, res.addl_ext, res.addl_int,res.ag,res.addl_total)
            };
            //subjects not included in total mark
            MarksheetModel[] extra_subjects =
            {
                new MarksheetModel("PHYSICAL EDUCATION", 0, 0, 100, 33, 0, res.phe, 0,res.phe),
                new MarksheetModel("WORK EXPERIENCE", 0, 0, 100, 33, 0, res.we, 0,  res.we)
            };
            /* PRINTING MARKSHEET */
            int width = 40;
            int height = 450;
            int i = 0;
            for (; i < compulsory_subjects.Length; i++)
            {

                marks_print.BeginText();
                // printing paper name
                marks_print.ShowTextAligned(Element.ALIGN_LEFT, (i + 1) + ". " + compulsory_subjects[i].sub_name.ToUpper(), width + 10, height, 0);

                //printing external mark
                marks_print.ShowTextAligned(Element.ALIGN_LEFT, "EXTERNAL MARK", width + 200, height, 0);
                marks_print.ShowTextAligned(Element.ALIGN_RIGHT, compulsory_subjects[i].ext_pass_mark.ToString(), width + 350, height, 0);
                marks_print.ShowTextAligned(Element.ALIGN_RIGHT, compulsory_subjects[i].ext_full_mark.ToString(), width + 405, height, 0);
                marks_print.ShowTextAligned(Element.ALIGN_RIGHT, compulsory_subjects[i].ext_scored_mark + "", width + 460, height, 0);

                //printing internal mark
                marks_print.ShowTextAligned(Element.ALIGN_LEFT, "INTERNAL MARK", width + 200, height - 13, 0);
                marks_print.ShowTextAligned(Element.ALIGN_RIGHT, compulsory_subjects[i].int_pass_mark + "", width + 350, height - 13, 0);
                marks_print.ShowTextAligned(Element.ALIGN_RIGHT, compulsory_subjects[i].int_full_mark + "", width + 405, height - 13, 0);
                marks_print.ShowTextAligned(Element.ALIGN_RIGHT, compulsory_subjects[i].int_scored_mark + "", width + 460, height - 13, 0);

                //printing total mark
                marks_print.ShowTextAligned(Element.ALIGN_LEFT, "TOTAL:", width + 200, height - 26, 0);
                marks_print.ShowTextAligned(Element.ALIGN_RIGHT, "33", width + 350, height - 26, 0);
                marks_print.ShowTextAligned(Element.ALIGN_RIGHT, "100", width + 405, height - 26, 0);

                decimal? total_marks = compulsory_subjects[i].total;
                marks_print.ShowTextAligned(Element.ALIGN_RIGHT, total_marks + "", width + 490, height - 26, 0);

                marks_print.EndText();
                height -= 47;
            }
            // drawing a line
            /***************************/
            height += 5;
            /*** Dash Line ***/
            cb.MoveTo(45, height);
            cb.SetLineDash(5, 2, 0);
            cb.LineTo(size.Width - 45, height);
            cb.Stroke();
            /*****************/
            //Grand Total
            marks_print.BeginText();
            marks_print.ShowTextAligned(Element.ALIGN_LEFT, "TOTAL:", width + 10, height - 12, 0);
            marks_print.ShowTextAligned(Element.ALIGN_RIGHT, res.gtotal + "", width + 490, height - 12, 0);
            marks_print.EndText();

            /*** Dash Line ***/
            cb.MoveTo(45, height - 18);
            cb.SetLineDash(5, 2, 0);
            cb.LineTo(size.Width - 45, height - 18);
            cb.Stroke();
            /*****************/
            /**** printing marks which are not includedin total ****/
            height = height - 30;

            for (int j = 0; j < extra_subjects.Length; j++)
            {
                marks_print.BeginText();
                // printing paper name
                marks_print.ShowTextAligned(Element.ALIGN_LEFT, (++i) + ". " + extra_subjects[j].sub_name.ToUpper(), width + 10, height, 0);

                marks_print.ShowTextAligned(Element.ALIGN_RIGHT, extra_subjects[j].int_pass_mark + "", width + 350, height, 0);
                marks_print.ShowTextAligned(Element.ALIGN_RIGHT, extra_subjects[j].int_full_mark + "", width + 405, height, 0);
                marks_print.ShowTextAligned(Element.ALIGN_RIGHT, extra_subjects[j].int_scored_mark + "", width + 490, height, 0);
                marks_print.EndText();
                height -= 15;
            }
            // print division
            string result = "";
            switch (res.divi)
            {
                case 1: result = "First"; break;
                case 2: result = "Second"; break;
                case 3: result = "Third"; break;
                case 4: result = "Simple Passed"; break;
                case 5: result = "Failed"; break;
                case 6: result = "Expelled"; break;
            }

            cb.BeginText();
            cb.SetFontAndSize(bf, 12);
            // put the alignment and coordinates here
            cb.ShowTextAligned(Element.ALIGN_LEFT, result, 185, 507, 0);
            cb.EndText();

            stamper.Close();
            reader.Close();
            Response.Buffer = true;
            Response.ContentType = "application/pdf";
            Response.AddHeader("content-disposition", "attachment;filename=" + roll_no + year + ".pdf");
            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            Response.Write(stamper);
            Response.End();
        }

        [Authorize(Roles = "Other")]
        /** Marksheet format for the year 2010 to 2015 **/
        public void generate_2010_to_2015_model(hslc res)
        {
            string name = res.name.ToUpper();
            string roll_no = res.roll.ToString();
            string year = res.exm_year.ToString();
            string dob = res.dob;

            //setting template pdf file path
            //string TemplateFile = "http://localhost:15059/Content/files/format_new.pdf";
            string TemplateFile = Convert.ToString(ConfigurationManager.AppSettings["template_path"]);
            // open the reader
            PdfReader reader = new PdfReader(TemplateFile);
            iTextSharp.text.Rectangle size = reader.GetPageSizeWithRotation(1);

            //Pdf Stamper
            PdfStamper stamper = new PdfStamper(reader, Response.OutputStream);
            // Modifying the pdf content
            PdfContentByte cb = stamper.GetOverContent(1);
            // select the font properties
            BaseFont bf = BaseFont.CreateFont(BaseFont.TIMES_ROMAN, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
            cb.SetColorFill(BaseColor.DARK_GRAY);
            cb.SetFontAndSize(bf, 12);

            // write the text in the pdf content
            cb.BeginText();
            // put the alignment and coordinates here
            cb.ShowTextAligned(Element.ALIGN_LEFT, name, 90, 647, 0);
            cb.EndText();

            cb.BeginText();
            // put the alignment and coordinates here
            cb.ShowTextAligned(Element.ALIGN_LEFT, roll_no, 120, 627, 0);
            cb.EndText();

            cb.BeginText();
            // put the alignment and coordinates here
            cb.ShowTextAligned(Element.ALIGN_LEFT, res.father_name, 200, 608, 0);
            cb.EndText();

            cb.BeginText();
            // put the alignment and coordinates here
            cb.ShowTextAligned(Element.ALIGN_LEFT, res.mother_name, 150, 589, 0);
            cb.EndText();

            cb.BeginText();
            // put the alignment and coordinates here
            cb.ShowTextAligned(2, dob, 180, 569, 0);
            cb.EndText();

            cb.BeginText();
            // put the alignment and coordinates here
            cb.ShowTextAligned(Element.ALIGN_LEFT, year, 80, 529, 0);

            // put the alignment and coordinates here
            cb.ShowTextAligned(Element.ALIGN_LEFT, res.school.ToUpper(), 170, 529, 0);
            cb.EndText();

            PdfContentByte marks_print = stamper.GetOverContent(1);
            BaseFont basefont = BaseFont.CreateFont(BaseFont.TIMES_ROMAN, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
            cb.SetColorFill(BaseColor.DARK_GRAY);
            marks_print.SetFontAndSize(basefont, 11);
            //Getting subject wise marks
            MarksheetModel[] compulsory_subjects = {
                new MarksheetModel(get_sub_name(res.opt1), 80, 20, 20, 8, res.mil_ext, res.mil_int,res.lg,res.mil_total),
                new MarksheetModel("ENGLISH", 80, 20, 20, 8, res.eng_ext, res.eng_int, res.eg,res.eng_total),
                new MarksheetModel("MATHMETICS", 80, 20, 20, 8, res.math_ext, res.math_int,res.mg,res.math_total),
                new MarksheetModel("SCIENCE", 80, 20, 20, 8, res.sc_ext, res.sc_int,res.scg,res.sc_total),
                new MarksheetModel("SOCIAL SCIENCE", 80, 20, 20, 8, res.ssc_ext, res.ssc_int,res.ssg,res.ssc_total)
            };
            //subjects not included in total mark
            MarksheetModel[] extra_subjects =
            {
                new MarksheetModel("PHYSICAL EDUCATION", 0, 0, 100, 33, 0, res.phe, 0,res.phe),
                new MarksheetModel("WORK EXPERIENCE", 0, 0, 100, 33, 0, res.we, 0,  res.we)
            };
            /* PRINTING MARKSHEET */
            int width = 40;
            int height = 450;
            int i = 0;
            for (; i < compulsory_subjects.Length; i++)
            {

                marks_print.BeginText();
                // printing paper name
                marks_print.ShowTextAligned(Element.ALIGN_LEFT, (i + 1) + ". " + compulsory_subjects[i].sub_name.ToUpper(), width + 10, height, 0);

                //printing external mark
                marks_print.ShowTextAligned(Element.ALIGN_LEFT, "EXTERNAL MARK", width + 200, height, 0);
                marks_print.ShowTextAligned(Element.ALIGN_RIGHT, compulsory_subjects[i].ext_pass_mark.ToString(), width + 350, height, 0);
                marks_print.ShowTextAligned(Element.ALIGN_RIGHT, compulsory_subjects[i].ext_full_mark.ToString(), width + 405, height, 0);
                marks_print.ShowTextAligned(Element.ALIGN_RIGHT, compulsory_subjects[i].ext_scored_mark + "", width + 460, height, 0);

                //printing internal mark
                marks_print.ShowTextAligned(Element.ALIGN_LEFT, "INTERNAL MARK", width + 200, height - 13, 0);
                marks_print.ShowTextAligned(Element.ALIGN_RIGHT, compulsory_subjects[i].int_pass_mark + "", width + 350, height - 13, 0);
                marks_print.ShowTextAligned(Element.ALIGN_RIGHT, compulsory_subjects[i].int_full_mark + "", width + 405, height - 13, 0);
                marks_print.ShowTextAligned(Element.ALIGN_RIGHT, compulsory_subjects[i].int_scored_mark + "", width + 460, height - 13, 0);

                //printing total mark
                marks_print.ShowTextAligned(Element.ALIGN_LEFT, "TOTAL:", width + 200, height - 26, 0);
                marks_print.ShowTextAligned(Element.ALIGN_RIGHT, "33", width + 350, height - 26, 0);
                marks_print.ShowTextAligned(Element.ALIGN_RIGHT, "100", width + 405, height - 26, 0);

                decimal? total_marks = compulsory_subjects[i].total;
                marks_print.ShowTextAligned(Element.ALIGN_RIGHT, total_marks + "", width + 490, height - 26, 0);

                marks_print.EndText();
                height -= 47;
            }
            // drawing a line
            /***************************/
            height += 5;
            /*** Dash Line ***/
            cb.MoveTo(45, height);
            cb.SetLineDash(5, 2, 0);
            cb.LineTo(size.Width - 45, height);
            cb.Stroke();
            /*****************/
            //Grand Total
            marks_print.BeginText();
            marks_print.ShowTextAligned(Element.ALIGN_LEFT, "Total Without Additional Subject -", width + 10, height - 12, 0);
            marks_print.ShowTextAligned(Element.ALIGN_RIGHT, res.total + "", width + 490, height - 12, 0);
            marks_print.EndText();

            /*** Dash Line ***/
            cb.MoveTo(45, height - 18);
            cb.SetLineDash(5, 2, 0);
            cb.LineTo(size.Width - 45, height - 18);
            cb.Stroke();
            /*****************/
            height -= 20;
            /*** Dash Line ***/
            cb.MoveTo(45, height - 18);
            cb.SetLineDash(5, 2, 0);
            cb.LineTo(size.Width - 45, height - 18);
            cb.Stroke();
            /*****************/
            /**** printing marks which are not includedin total ****/
            height = height - 30;

            for (int j = 0; j < extra_subjects.Length; j++)
            {
                marks_print.BeginText();
                // printing paper name
                marks_print.ShowTextAligned(Element.ALIGN_LEFT, (++i) + ". " + extra_subjects[j].sub_name.ToUpper(), width + 10, height, 0);

                marks_print.ShowTextAligned(Element.ALIGN_RIGHT, extra_subjects[j].int_pass_mark + "", width + 350, height, 0);
                marks_print.ShowTextAligned(Element.ALIGN_RIGHT, extra_subjects[j].int_full_mark + "", width + 405, height, 0);
                marks_print.ShowTextAligned(Element.ALIGN_RIGHT, extra_subjects[j].int_scored_mark + "", width + 490, height, 0);
                marks_print.EndText();
                height -= 15;
            }
            // print division
            string result = "";
            switch (res.divi)
            {
                case 1: result = "First"; break;
                case 2: result = "Second"; break;
                case 3: result = "Third"; break;
                case 4: result = "Simple Passed"; break;
                case 5: result = "Failed"; break;
                case 6: result = "Expelled"; break;
            }

            cb.BeginText();
            cb.SetFontAndSize(bf, 12);
            // put the alignment and coordinates here
            cb.ShowTextAligned(Element.ALIGN_LEFT, result, 185, 507, 0);
            cb.EndText();

            stamper.Close();
            reader.Close();
            Response.Buffer = true;
            Response.ContentType = "application/pdf";
            Response.AddHeader("content-disposition", "attachment;filename=" + roll_no + year + ".pdf");
            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            Response.Write(stamper);
            Response.End();
        }

        [Authorize(Roles = "Other")]
        public void generate_2004_to_2009_model(hslc res)
        {
            string name = res.name.ToUpper();
            string roll_no = res.roll.ToString();
            string year = res.exm_year.ToString();
            string dob = res.dob;

            //setting template pdf file path
            //string TemplateFile = "http://localhost:15059/Content/files/format_new.pdf";
            string TemplateFile = Convert.ToString(ConfigurationManager.AppSettings["template_path"]);
            // open the reader
            PdfReader reader = new PdfReader(TemplateFile);
            iTextSharp.text.Rectangle size = reader.GetPageSizeWithRotation(1);

            //Pdf Stamper
            PdfStamper stamper = new PdfStamper(reader, Response.OutputStream);
            // Modifying the pdf content
            PdfContentByte cb = stamper.GetOverContent(1);
            // select the font properties
            BaseFont bf = BaseFont.CreateFont(BaseFont.TIMES_ROMAN, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
            cb.SetColorFill(BaseColor.DARK_GRAY);
            cb.SetFontAndSize(bf, 12);

            // write the text in the pdf content
            cb.BeginText();
            // put the alignment and coordinates here
            cb.ShowTextAligned(Element.ALIGN_LEFT, name, 90, 647, 0);
            cb.EndText();

            cb.BeginText();
            // put the alignment and coordinates here
            cb.ShowTextAligned(Element.ALIGN_LEFT, roll_no, 120, 627, 0);
            cb.EndText();

            cb.BeginText();
            // put the alignment and coordinates here
            cb.ShowTextAligned(Element.ALIGN_LEFT, res.father_name.ToUpper(), 200, 608, 0);
            cb.EndText();

            cb.BeginText();
            // put the alignment and coordinates here
            cb.ShowTextAligned(Element.ALIGN_LEFT, res.mother_name.ToUpper(), 150, 589, 0);
            cb.EndText();

            cb.BeginText();
            // put the alignment and coordinates here
            cb.ShowTextAligned(2, dob, 180, 569, 0);
            cb.EndText();

            cb.BeginText();
            // put the alignment and coordinates here
            cb.ShowTextAligned(Element.ALIGN_LEFT, year, 80, 529, 0);

            // put the alignment and coordinates here
            cb.ShowTextAligned(Element.ALIGN_LEFT, res.school.ToUpper(), 170, 529, 0);
            cb.EndText();

            PdfContentByte marks_print = stamper.GetOverContent(1);
            BaseFont basefont = BaseFont.CreateFont(BaseFont.TIMES_ROMAN, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
            cb.SetColorFill(BaseColor.DARK_GRAY);
            marks_print.SetFontAndSize(basefont, 11);

            //Getting subject wise marks

            //finding the list of subjects pescribed by the BoSEM for the year
            List<MarkModel> subs_inc_total = get_marks(res, true);//subjects included in grand total
            List<MarkModel> subs_not_inc_total = get_marks(res, false);//subjects not included in grand total

            int i = 0;//iteration
            int height = 450;//height
            int width = 40;//width

            for (; i < subs_inc_total.Count(); i++)
            {
                marks_print.BeginText();
                // printing paper name
                marks_print.ShowTextAligned(Element.ALIGN_LEFT, (i + 1) + ". " + subs_inc_total.ElementAt(i).subject.ToUpper(), width + 10, height, 0);
                List<FieldModel> fields = subs_inc_total.ElementAt(i).fields;
                if (fields.Count() == 1)
                {
                    marks_print.ShowTextAligned(Element.ALIGN_RIGHT, fields.ElementAt(0).pass_mark.ToString(), width + 350, height, 0);
                    marks_print.ShowTextAligned(Element.ALIGN_RIGHT, fields.ElementAt(0).full_mark.ToString(), width + 405, height, 0);
                    marks_print.ShowTextAligned(Element.ALIGN_RIGHT, fields.ElementAt(0).scored_mark.ToString(), width + 490, height, 0);
                    height = height - 15;
                }
                else
                {
                    
                    foreach (var field in fields)
                    {
                        height = height - 15;
                        marks_print.ShowTextAligned(Element.ALIGN_LEFT, field.field_name, width+20, height, 0);
                        marks_print.ShowTextAligned(Element.ALIGN_RIGHT, field.pass_mark.ToString(), width + 350, height, 0);
                        marks_print.ShowTextAligned(Element.ALIGN_RIGHT, field.full_mark.ToString(), width + 405, height, 0);
                        if (field.field_name.Equals("TOTAL"))
                        {
                            marks_print.ShowTextAligned(Element.ALIGN_RIGHT, field.scored_mark.ToString(), width + 490, height, 0);
                        }
                        else
                        {
                            marks_print.ShowTextAligned(Element.ALIGN_RIGHT, field.scored_mark.ToString(), width + 460, height, 0);
                        }
                        
                    }
                    
                    height = height - 15;
                }
                
                marks_print.EndText();
            }

            /* PRINTING MARKSHEET */
            // drawing a line
            /***************************/
            height += 5;
            /*** Dash Line ***/
            cb.MoveTo(45, height);
            cb.SetLineDash(5, 2, 0);
            cb.LineTo(size.Width - 45, height);
            cb.Stroke();
            /*****************/
            height -= 15;
            //Total without additional subject
            marks_print.BeginText();
            marks_print.ShowTextAligned(Element.ALIGN_LEFT, "Total Without Additional Subject -", width + 10, height, 0);
            marks_print.ShowTextAligned(Element.ALIGN_RIGHT, res.total + "", width + 490, height, 0);
            marks_print.EndText();

            /*** Dash Line ***/
            height -= 10;
            cb.MoveTo(45, height);
            cb.SetLineDash(5, 2, 0);
            cb.LineTo(size.Width - 45, height);
            cb.Stroke();
            /*****************/


            /**** printing marks which are not included in total ****/
            height = height - 16;

            for (int j = 0; j < subs_not_inc_total.Count(); j++)
            {
                MarkModel mark = subs_not_inc_total.ElementAt(j);
                marks_print.BeginText();
                // printing paper name
                if (mark.sub_type.Equals("a")) {
                    marks_print.ShowTextAligned(Element.ALIGN_LEFT, (++i) + ". " + mark.subject.ToUpper()+" AND EXCESS MARKS", width + 10, height, 0);
                }
                else
                {
                    marks_print.ShowTextAligned(Element.ALIGN_LEFT, (++i) + ". " + mark.subject.ToUpper(), width + 10, height, 0);
                }
               
                List<FieldModel> fields = mark.fields;
                if (fields.Count() == 1)
                {
                    marks_print.ShowTextAligned(Element.ALIGN_RIGHT, fields.ElementAt(0).pass_mark.ToString(), width + 350, height, 0);
                    marks_print.ShowTextAligned(Element.ALIGN_RIGHT, fields.ElementAt(0).full_mark.ToString(), width + 405, height, 0);
                    marks_print.ShowTextAligned(Element.ALIGN_RIGHT, fields.ElementAt(0).scored_mark.ToString(), width + 490, height, 0);
                    if (mark.sub_type.Equals("a")) {
                        decimal? exceeding_mark = fields.ElementAt(0).scored_mark - fields.ElementAt(0).pass_mark;
                        if(exceeding_mark>0)
                        marks_print.ShowTextAligned(Element.ALIGN_RIGHT, exceeding_mark.ToString(), width + 510, height, 0);
                    }
                    height = height - 15;
                }
                else
                {
                    foreach (var field in fields)
                    {
                        height = height - 15;
                        marks_print.ShowTextAligned(Element.ALIGN_LEFT, field.field_name, width + 20, height, 0);
                        marks_print.ShowTextAligned(Element.ALIGN_RIGHT, field.pass_mark.ToString(), width + 350, height, 0);
                        marks_print.ShowTextAligned(Element.ALIGN_RIGHT, field.full_mark.ToString(), width + 405, height, 0);
                        if (field.field_name.Equals("TOTAL"))
                        {
                            marks_print.ShowTextAligned(Element.ALIGN_RIGHT, field.scored_mark.ToString(), width + 490, height, 0);
                        }
                        else
                        {
                            marks_print.ShowTextAligned(Element.ALIGN_RIGHT, field.scored_mark.ToString(), width + 460, height, 0);
                        }
                        
                    }
                    height = height - 15;
                }
                marks_print.EndText();


                height = height + 5;
                if (mark.sub_type.Equals("a")) {

                    /*** Dash Line ***/
                    cb.MoveTo(45, height);
                    cb.SetLineDash(5, 2, 0);
                    cb.LineTo(size.Width - 45, height);
                    cb.Stroke();
                    /*****************/

                    height -= 15;
                    marks_print.BeginText();
                    marks_print.ShowTextAligned(Element.ALIGN_LEFT, "GRAND TOTAL:", width + 60, height, 0);
                    marks_print.ShowTextAligned(Element.ALIGN_RIGHT, res.gtotal + "", width + 490, height, 0);
                    marks_print.EndText();
                    height -= 9;

                    /*** Dash Line ***/
                    cb.MoveTo(45, height);
                    cb.SetLineDash(5, 2, 0);
                    cb.LineTo(size.Width - 45, height);
                    cb.Stroke();
                    height -= 10;
                    /*****************/
                }
                height -= 7;
            }
            // print division
            string result = "";
            switch (res.divi)
            {
                case 1: result = "First"; break;
                case 2: result = "Second"; break;
                case 3: result = "Third"; break;
                case 4: result = "Simple Passed"; break;
                case 5: result = "Failed"; break;
                case 6: result = "Expelled"; break;
            }

            cb.BeginText();
            cb.SetFontAndSize(bf, 12);
            // put the alignment and coordinates here
            cb.ShowTextAligned(Element.ALIGN_LEFT, result, 185, 507, 0);
            cb.EndText();

            stamper.Close();
            reader.Close();
            Response.Buffer = true;
            Response.ContentType = "application/pdf";
            Response.AddHeader("content-disposition", "attachment;filename=" + roll_no + year + ".pdf");
            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            Response.Write(stamper);
            Response.End();
        }

        public string get_sub_name(string abbr) {
            Subject sub = db.Subjects.Where(m => m.abbrevation == abbr).FirstOrDefault();
            if (sub == null)
                return "NOT FOUND";
            return sub.name.ToUpper();
        }

        
        [Authorize(Roles = "Other")]
        public ActionResult RequestHistData(DTParameters param)
        {
            if (Request.IsAuthenticated)
            {
                try
                {
                    string user_id = User.Identity.GetUserId();

                    List<requested_mark> dtsource = new List<requested_mark>();//data source   
                    using (BOSEMEntities dc = new BOSEMEntities())
                    {
                        dtsource = dc.requested_mark.Where(m=>m.user_id==user_id && m.payment_status!="unpaid").OrderByDescending(m=>m.request_date).ToList();
                    }
                    
                    List<String> columnSearch = new List<string>();

                    foreach (var col in param.Columns)
                    {
                        columnSearch.Add(col.Search.Value);
                    }

                    List<requested_mark> data = new ResultSet().GetResult(param.Search.Value, param.SortOrder, param.Start, param.Length, dtsource, columnSearch, user_id);

                    List<RequestHistModel> new_data = new List<RequestHistModel>();
                    foreach (requested_mark row_data in data) {
                        new_data.Add(new RequestHistModel(row_data));
                    }

                    int count = new ResultSet().Count(param.Search.Value, dtsource, columnSearch, user_id);
                    DTResult<RequestHistModel> result = new DTResult<RequestHistModel>
                    {
                        draw = param.Draw,
                        data = new_data,
                        recordsFiltered = count,
                        recordsTotal = count
                    };
                    return Json(result);
                }
                catch (Exception ex)
                {
                    return Json(new { error = ex.Message });
                }
            }
            else
            {
                return RedirectToAction("Login", "Account");
            }        
        }

        [Authorize(Roles = "Other,Admin")]
        public ActionResult PaymentHistData(DTParameters param)
        {
            if (Request.IsAuthenticated)
            {
                try
                {
                    string user_id = User.Identity.GetUserId();
                    var dtsource = new List<TransactionHistory>();//data source for payment history
                    using (BOSEMEntities dc = new BOSEMEntities())
                    {
                        if (isAdminUser())
                        {
                            dtsource = db.Database.SqlQuery<TransactionHistory>("select * from TransactionHistory where txn_id in (" +
                        "select distinct txn_id from requested_mark where txn_id is not NULL" +
                        ") order by create_at desc").OrderByDescending(m => m.create_at).ToList();
                        }
                        else
                        {
                            dtsource = db.Database.SqlQuery<TransactionHistory>("select * from TransactionHistory where txn_id in (" +
                        "select distinct txn_id from requested_mark where txn_id is not NULL and user_id = '" + user_id + "'" +
                        ") order by create_at desc").OrderByDescending(m => m.create_at).ToList();
                        }
                    }

                    List<String> columnSearch = new List<string>();

                    foreach (var col in param.Columns)
                    {
                        columnSearch.Add(col.Search.Value);
                    }

                    List<TransactionHistory> data = new ResultSet().GetTxnResult(param.Search.Value, param.SortOrder, param.Start, param.Length, dtsource, columnSearch);
                    List<TxnHistViewModel> new_data = new List<TxnHistViewModel>();
                    foreach (TransactionHistory row in data) {
                        new_data.Add(new TxnHistViewModel(row));
                    }

                    int count = new ResultSet().TxnCount(param.Search.Value, dtsource, columnSearch);
                    DTResult<TxnHistViewModel> result = new DTResult<TxnHistViewModel>
                    {
                        draw = param.Draw,
                        data = new_data,
                        recordsFiltered = count,
                        recordsTotal = count
                    };
                    return Json(result);
                }
                catch (Exception ex)
                {
                    return Json(new { error = ex.Message });
                }
            }
            else
            {
                return RedirectToAction("Login", "Account");
            }
        }

    }
}