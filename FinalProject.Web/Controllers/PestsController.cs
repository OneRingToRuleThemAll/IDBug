﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using FinalProject.Web.Models;

namespace FinalProject.Web.Controllers
{
    public class PestsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();


        public ActionResult StartOver()
        {
            Session["CurrentQuestionId"] = null;
            Session["AllAnswers"] = null;

            return Redirect("ShowNextQuestion");
        }
        public ActionResult HomePage()
        {
            return View();
        }

        public ActionResult Results()
        {
            var answerIds = GetAnswers();

            var pests = GetPests(answerIds).ToList();
            return View(pests);
        }

        [HttpGet]
        public ActionResult ShowNextQuestion()
        {
            //grab the question id out of the session, if it's not set assume question id of 1
            //then find that question in the db
            //create the view model to dispaly the question and answer to the user.
            var answerIds = GetAnswers();
            int questionId = GetQuestionId();
            var question = db.Questions.Find(questionId);

            var model = new QuestionViewModel()
            {
                QuestionId = question.Id,
                QuestionText = question.Text,
                Answers = question.Answers.ToDictionary(x => x.Id, x => x.Text),
                NumberOfResults = GetPests(answerIds).Count()
            };

            return View(model);
        }

        private IQueryable<Pest> GetPests(IEnumerable<int> answerIds)
        {
            var numofAnswers = answerIds.Count();
            var pests = db.Answers.Where(a => answerIds.Contains(a.Id)).SelectMany(x => x.AssociatedPest);

            var matches = pests.GroupBy(p => p).Where(x => x.Count() == numofAnswers).Select(x => x.Key);
            return matches;
        }

        private int GetQuestionId()
        {
            int questionId = 1;
            if (Session["CurrentQuestionId"] != null)
            {
                questionId = (int)Session["CurrentQuestionId"];
            }
            else
            {
                Session["CurrentQuestionId"] = 1;
            }

            return questionId;
        }

        //public List<Answer> GetAnswers()
        //{

        //    List<int> AnswerIds = (List<int>)Session["AllAnswers"];
        //    db.Answers.Include(x=>x.AssociatedPest).Where(x => AnswerIds.Contains(x.Id));
        //    Answers.Select(x => x.AssociatedPest).ToList();
        //    return(Answers);
        //}


        [HttpPost]
        public ActionResult ShowNextQuestion(int? SelectedAnswer)
        {
            if (SelectedAnswer == null)
            {
                return RedirectToAction("ShowNextQuestion");
            }

            StoreAnswer(SelectedAnswer.Value);//if the answer's next question is null then redirect them to a results page or something


            //SelectedAnswer is the id of the answer they selected. Find it and find that answer's next question id .
            //also store the selected answer in the session
            //redirect them back to ShowNextQuestion
            var nextquestion = db.Answers.Find(SelectedAnswer).NextQuestion;

            if (nextquestion == null)
            {
                return RedirectToAction("Results");
            }

            Session["CurrentQuestionId"] = nextquestion.Id;
            return RedirectToAction("ShowNextQuestion");
        }


        private List<int> GetAnswers()
        {
            List<int> AnswerIds = (List<int>)Session["AllAnswers"];
            if (AnswerIds == null)
                AnswerIds = new List<int>();

            return AnswerIds;
        }

        private void StoreAnswer(int SelectedAnswer)
        {
            List<int> AnswerIds = (List<int>)Session["AllAnswers"];
            if (AnswerIds == null)
                AnswerIds = new List<int>();

            AnswerIds.Add(SelectedAnswer);
            Session["AllAnswers"] = AnswerIds;
        }



        // GET: Pests
        public ActionResult Index()
        {
            return View(db.Pests.ToList());
        }

        // GET: Pests/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Pest pest = db.Pests.Find(id);
            if (pest == null)
            {
                return HttpNotFound();
            }
            return View(pest);
        }

        // GET: Pests/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Pests/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Pest pest)
        {
            if (ModelState.IsValid)
            {
                db.Pests.Add(pest);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(pest);
        }

        // GET: Pests/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Pest pest = db.Pests.Find(id);
            if (pest == null)
            {
                return HttpNotFound();
            }
            return View(pest);
        }

        // POST: Pests/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Pest pest)
        {
            if (ModelState.IsValid)
            {
                db.Entry(pest).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(pest);
        }

        // GET: Pests/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Pest pest = db.Pests.Find(id);
            if (pest == null)
            {
                return HttpNotFound();
            }
            return View(pest);
        }

        // POST: Pests/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Pest pest = db.Pests.Find(id);
            db.Pests.Remove(pest);
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
    }
}
