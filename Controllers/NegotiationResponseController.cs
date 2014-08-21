using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Data.Entities;
using Data.Interfaces;
using KwasantCore.Managers;
using KwasantWeb.ViewModels;
using Microsoft.AspNet.Identity;
using StructureMap;

namespace KwasantWeb.Controllers
{
    public class NegotiationResponseController : Controller
    {
        [KwasantAuthorize(Roles = "Customer")]
        public ActionResult View(int negotiationID)
        {
            var userID = User.Identity.GetUserId();

            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var negotiationDO = uow.NegotiationsRepository.GetQuery().FirstOrDefault(n => n.Id == negotiationID);
                if (negotiationDO == null)
                    throw new ApplicationException("Negotiation with ID " + negotiationID + " does not exist.");

                var questionIDs = negotiationDO.Questions.Select(q => q.Id).Distinct().ToList();
                var alreadyAnsweredQuestions = uow.QuestionResponseRepository.GetQuery().Where(qr => qr.UserID == userID && questionIDs.Contains(qr.QuestionID)).ToList();
                if (alreadyAnsweredQuestions.Select(aq => aq.QuestionID).Distinct().Count() == questionIDs.Count)
                {
                    //Answered everything already
                    return View("~/Views/ClarificationResponse/AllAnswered.cshtml");
                }

                var model = new NegotiationViewModel
                {
                    Id = negotiationDO.Id,
                    Name = negotiationDO.Name,
                    BookingRequestID = negotiationDO.BookingRequestID,
                    State = negotiationDO.NegotiationState,

                    Attendees = negotiationDO.Attendees.Select(a => a.Name).ToList(),
                    Questions = negotiationDO.Questions.Select(q =>
                        new NegotiationQuestionViewModel
                        {
                            AnswerType = q.AnswerType,
                            Id = q.Id,
                            Status = q.QuestionStatus,
                            Text = q.Text,
                            NegotiationId = negotiationDO.Id,
                            CalendarEvents = q.Calendar == null ? new List<QuestionCalendarEventViewModel>() : q.Calendar.Events.Select(e => new QuestionCalendarEventViewModel
                            {
                                StartDate = e.StartDate,
                                EndDate = e.EndDate
                            }).ToList(),

                            CalendarID = q.CalendarID,
                            Answers = q.Answers.Select(a =>
                                new NegotiationAnswerViewModel
                                {
                                    Status = a.AnswerStatus,
                                    Id = a.Id,
                                    QuestionId = q.Id,
                                    Text = a.Text
                                }).ToList()
                        }
                        ).ToList()
                };

                return View(model);
            }
        }

        [KwasantAuthorize(Roles = "Customer")]
        [HttpPost]
        public ActionResult ProcessResponse(NegotiationResponseViewModel value)
        {
            var userID = User.Identity.GetUserId();

            if (value.Responses != null)
            {
                using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
                {
                    foreach (var response in value.Responses)
                    {
                        var questionResponse = uow.QuestionResponseRepository.GetQuery().FirstOrDefault(qr => qr.QuestionID == response.QuestionID);
                        if (questionResponse == null)
                        {
                            questionResponse = new QuestionResponseDO();
                            uow.QuestionResponseRepository.Add(questionResponse);
                        }

                        questionResponse.QuestionID = response.QuestionID;
                        questionResponse.AnswerID = response.AnswerID;
                        questionResponse.Text = response.Response;
                        questionResponse.UserID = userID;
                    }
                    uow.SaveChanges();
                }
            }

            return View();
        }
	}
}