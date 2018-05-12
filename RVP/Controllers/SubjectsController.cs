using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using RVP.Models;

namespace RVP.Controllers
{
    public class SubjectsController : Controller
    {
        private BOSEMEntities db = new BOSEMEntities();

        [Authorize(Roles = "Admin")]
        // GET: Subjects
        public ActionResult Index()
        {
            return View(db.Subjects.ToList());
        }

        [Authorize(Roles = "Admin")]
        // GET: Subjects/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Subject subject = db.Subjects.Find(id);
            if (subject == null)
            {
                return HttpNotFound();
            }
            return View(subject);
        }

        [Authorize(Roles = "Admin")]
        // GET: Subjects/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Subjects/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public ActionResult Create([Bind(Include = "sub_code,name,abbrevation,seq_cd,sub_type,id")] Subject subject)
        {
            if (ModelState.IsValid)
            {
                subject.sub_type = subject.sub_type.ToUpper();
                db.Subjects.Add(subject);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            
            return View(subject);
        }

        // GET: Subjects/Edit/5
        [Authorize(Roles = "Admin")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Subject subject = db.Subjects.Find(id);
            if (subject == null)
            {
                return HttpNotFound();
            }
            return View(subject);
        }

        // POST: Subjects/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public ActionResult Edit([Bind(Include = "sub_code,name,abbrevation,seq_cd,sub_type,id")] Subject subject)
        {
            if (ModelState.IsValid)
            {
                subject.sub_type = subject.sub_type.ToUpper();
                db.Entry(subject).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(subject);
        }

        // GET: Subjects/Delete/5
        [Authorize(Roles = "Admin")]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Subject subject = db.Subjects.Find(id);
            if (subject == null)
            {
                return HttpNotFound();
            }
            return View(subject);
        }

        // POST: Subjects/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public ActionResult DeleteConfirmed(int id)
        {
            Subject subject = db.Subjects.Find(id);
            db.Subjects.Remove(subject);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        [Authorize(Roles = "Admin")]
        public ActionResult SubjectYear()
        {
            return View(db.Subjects.OrderBy(m=>m.name).ToList());
        }

        [Authorize(Roles = "Admin")]
        public ActionResult SubjectYearCreate([Bind(Include = "sub_id,year,incl_in_total")] SubjectYearCombinations model) {
            if (ModelState.IsValid)
            {
                try
                {
                    if (db.SubjectYearCombinations.Any(m => m.sub_id == model.sub_id && m.year == model.year))
                    {
                        return Json(new { status = "success", message = "Already added.", list = db.SubjectYearCombinations.Where(m => m.year == model.year).ToList() });
                    }
                    else
                    {
                        
                        db.SubjectYearCombinations.Add(model);
                        db.SaveChanges();
                        return Json(new { status = "success", message = "Saved successfully.", list = db.SubjectYearCombinations.ToList() });
                        
                    }
                }
                catch (Exception exception) {
                    return Json(new { status = "error", message = "Failed in saving. " + exception.GetBaseException() });
                }
            }
            else
            {
                return Json(new { status = "error", message = "Error in saving" });
            }
        }
    }
}
