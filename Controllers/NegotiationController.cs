using System;
using System.Linq;
using System.Web.Mvc;
using Data.Entities;
using Data.Interfaces;
using Data.States;
using KwasantWeb.ViewModels;
using StructureMap;


namespace KwasantWeb.Controllers
{
    //[KwasantAuthorize(Roles = "Admin")]
    public class NegotiationController : Controller
    {
        
        public ActionResult Edit(int negotiationID)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var negotiationDO = uow.NegotiationsRepository.GetQuery().FirstOrDefault(n => n.Id == negotiationID);
                if (negotiationDO == null)
                    throw new ApplicationException("Negotiation with ID " + negotiationID + " does not exist.");

                var model = new NegotiationViewModel
                {
                    Id = negotiationDO.Id,
                    Name = negotiationDO.Name,
                    BookingRequestID = negotiationDO.BookingRequestID,
                    State = negotiationDO.NegotiationState,
                    Questions = negotiationDO.Questions.Select(q =>
                        new QuestionViewModel
                        {
                            AnswerType = q.AnswerType,
                            Id = q.Id,
                            Status = q.QuestionStatus,
                            Text = q.Text,
                            NegotiationId = negotiationDO.Id,
                            Answers = q.Answers.Select(a =>
                            new AnswerViewModel
                            {
                                AnswerState = a.AnswerStatus,
                                Id = a.Id,
                                QuestionID = q.Id,
                                Text = a.Text,
                                ObjectsType = a.ObjectsType,
                                CalendarID = a.CalendarID
                            }).ToList()
                        }
                        ).ToList()
                };

                return View(model);
            }
        }


        public ActionResult Create(int bookingRequestID)
        {
            return View("~/Views/Negotiation/Edit.cshtml", new NegotiationViewModel
            {
                Name = "Negotiation 1",
                BookingRequestID = bookingRequestID,
                State = NegotiationState.InProcess,
            });
        }

        [HttpPost]
        public JsonResult ProcessSubmittedForm(NegotiationViewModel value)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                NegotiationDO negotiationDO;
                if (value.Id == 0)
                {
                    negotiationDO = new NegotiationDO();
                    uow.NegotiationsRepository.Add(negotiationDO);
                }
                else
                    negotiationDO = uow.NegotiationsRepository.GetByKey(value.Id);

                negotiationDO.Name = value.Name;
                negotiationDO.NegotiationState = value.State;
                negotiationDO.BookingRequestID = value.BookingRequestID;

                var proposedQuestionIDs = value.Questions.Select(q => q.Id);
                //Delete the existing questions which no longer exist in our proposed negotiation
                var existingQuestions = negotiationDO.Questions.ToList();
                foreach (var existingQuestion in existingQuestions.Where(q => !proposedQuestionIDs.Contains(q.Id)))
                {
                    uow.QuestionsRepository.Remove(existingQuestion);
                }

                //Here we add/update questions based on our proposed negotiation
                foreach (var question in value.Questions)
                {
                    QuestionDO questionDO;
                    if (question.Id == 0)
                    {
                        questionDO = new QuestionDO();
                        uow.QuestionsRepository.Add(questionDO);
                    }
                    else
                        questionDO = uow.QuestionsRepository.GetByKey(question.Id);

                    questionDO.Negotiation = negotiationDO;
                    questionDO.AnswerType = question.AnswerType;
                    questionDO.QuestionStatus = question.Status;
                    questionDO.Text = question.Text;

                    var proposedAnswerIDs = question.Answers.Select(a => a.Id);
                    //Delete the existing answers which no longer exist in our proposed negotiation
                    var existingAnswers = questionDO.Answers.ToList();
                    foreach (var existingAnswer in existingAnswers.Where(a => !proposedAnswerIDs.Contains(a.Id)))
                    {
                        uow.AnswersRepository.Remove(existingAnswer);
                    }

                    foreach (var answer in question.Answers)
                    {
                        AnswerDO answerDO;
                        if (answer.Id == 0)
                        {
                            answerDO = new AnswerDO();
                            uow.AnswersRepository.Add(answerDO);
                        }
                        else
                            answerDO = uow.AnswersRepository.GetByKey(answer.Id);

                        answerDO.Question = questionDO;
                        answerDO.AnswerStatus = answer.AnswerState;
                        answerDO.CalendarID = answer.CalendarID;
                        answerDO.Text = answer.Text;
                        answer.ObjectsType = answer.ObjectsType;
                    }
                }

                uow.SaveChanges();
            }

            return Json(true, JsonRequestBehavior.AllowGet);
        }
    }
}