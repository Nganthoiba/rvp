using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net.Mail;

namespace RVP.Models
{
    public class MailModel
    {
        
        public string To{get;set;}
        public string Subject{get;set;}
        public string Body{get;set;}
        public string except { get; set; }
        public MailModel(string to, string sub, string body) {
            this.To = to;
            this.Subject = sub;
            this.Body = body;
        }

        public Boolean SendMail() {
            try
            {
                MailMessage mail = new MailMessage();
                mail.To.Add(this.To);
                mail.From = new MailAddress("resultverify@gmail.com");
                mail.Subject = this.Subject;
                string Body = this.Body;
                mail.Body = Body;
                mail.IsBodyHtml = true;
                SmtpClient smtp = new SmtpClient();
                smtp.Host = "smtp.gmail.com";
                smtp.Port = 587;
                smtp.UseDefaultCredentials = false;
                smtp.Credentials = new System.Net.NetworkCredential("resultverify@gmail.com", "Temp@123"); // Enter sender's User name and password  
                smtp.EnableSsl = true;
                smtp.Send(mail);
                //smtp.SendMailAsync(mail);
                return true;
            }
            catch (Exception except) {
                this.except = except.ToString();
            }
            return false;
        }
    }
}