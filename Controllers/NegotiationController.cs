using System;
using System.Linq;
using System.Web.Mvc;
using Data.Entities;
using Data.Interfaces;
using KwasantCore.Services;
using KwasantWeb.ViewModels;
using StructureMap;
using ViewModel.Models;


namespace KwasantWeb.Controllers
{
    //[KwasantAuthorize(Roles = "Admin")]
    public class NegotiationController : Controller
    {
        private Negotiation _negotiation;
        private Attendee _attendee;

        public NegotiationController()
        {
            _negotiation = new Negotiation();
            _attendee = new Attendee();
        }

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

        //This is completely broken. Static means that it's stored for _all_ connections, including different users!!!
        //public ActionResult Create(NegotiationViewModel viewModel)
        //{
        //    using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
        //    {
        //        BookingRequestDO emailDO = uow.BookingRequestRepository.FindOne(el => el.Id == BookingRequestID);
        //        UserDO userDO = uow.UserRepository.FindOne(ur => ur.EmailAddressID == emailDO.FromID);

        //        //NEED TO CHECK HERE TO SEE IF THERE ALREADY IS ONE. SOMETHING LIKE:
        //        NegotiationDO negotiationDO = uow.NegotiationsRepository.FindOne(n => n.BookingRequestID == BookingRequestID && n.NegotiationState != NegotiationState.Resolved);
        //        if (negotiationDO != null)
        //            throw new ApplicationException("tried to create a negotiation when one already existed");

        //        negotiationDO = new NegotiationDO
        //        {
        //            Name = viewModel.Name,
        //            BookingRequestID = BookingRequestID,
        //            NegotiationState = NegotiationState.InProcess,
        //            BookingRequest = emailDO
        //        };
        //        uow.NegotiationsRepository.Add(negotiationDO);
        //        uow.SaveChanges();
        //        var result = new { Success = "True", NegotiationId = negotiationDO.Id };
        //        return Json(result, JsonRequestBehavior.AllowGet);
        //    }
        //}

        public JsonResult DeleteQuestion(int questionId = 0)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                CalendarDO calendarDO = uow.CalendarRepository.FindOne(c => c.QuestionId == questionId);
                if (calendarDO != null)
                    uow.CalendarRepository.Remove(calendarDO);

                uow.QuestionsRepository.Remove(uow.QuestionsRepository.FindOne(q => q.Id == questionId));
                uow.SaveChanges();
                return Json("Success", JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult DeleteAnswer(int answerId = 0)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                CalendarDO calendarDO = uow.CalendarRepository.FindOne(c => c.QuestionId == answerId);
                if (calendarDO != null)
                    uow.CalendarRepository.Remove(calendarDO);

                uow.AnswersRepository.Remove(uow.AnswersRepository.FindOne(q => q.Id == answerId));
                uow.SaveChanges();
                return Json("Success", JsonRequestBehavior.AllowGet);
            }
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