using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Data.Entities;
using Data.Interfaces;
using KwasantCore.Exceptions;
using KwasantCore.Managers;
using KwasantCore.Services;
using KwasantWeb.ViewModels;
using AutoMapper;
using StructureMap;

namespace KwasantWeb.Controllers
{
    public class NegotiationResponseController : Controller
    {
        [KwasantAuthorize(Roles = "Customer")]
        public ActionResult View(int negotiationID)
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
        public ActionResult ProcessResponse(NegotiationResponseViewModel value)
        {
            return View();
        }
	}
}