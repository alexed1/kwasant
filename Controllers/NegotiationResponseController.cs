using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Data.Interfaces;
using KwasantCore.Managers;
using KwasantCore.Services;
using KwasantWeb.ViewModels;
using Microsoft.AspNet.Identity;
using StructureMap;

namespace KwasantWeb.Controllers
{
    public class NegotiationResponseController : Controller
    {
        private const bool EnforceUserInAttendees = true;

        [KwasantAuthorize(Roles = "Customer")]
        public ActionResult View(int negotiationID)
        {
            AuthenticateUser(negotiationID);

            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var negotiationDO = uow.NegotiationsRepository.GetQuery().FirstOrDefault(n => n.Id == negotiationID);
                if (negotiationDO == null)
                    throw new HttpException(404, "Negotiation not found.");

                var answerIDs = negotiationDO.Questions.SelectMany(q => q.Answers.Select(a => a.Id)).ToList();
                var userAnswerIDs = uow.QuestionResponseRepository.GetQuery().Where(qr => answerIDs.Contains(qr.AnswerID)).Select(a => a.AnswerID).ToList();

                var model = new NegotiationResponseVM
                {
                    Id = negotiationDO.Id,
                    Name = negotiationDO.Name,
                    BookingRequestID = negotiationDO.BookingRequestID,

                    Attendees = negotiationDO.Attendees.Select(a => a.Name).ToList(),
                    Questions = negotiationDO.Questions.Select(q =>
                    {
                        return (NegotiationQuestionVM) new NegotiationResponseQuestionVM
                        {
                            AnswerType = q.AnswerType,
                            Id = q.Id,
                            Text = q.Text,
                            
                            NegotiationId = negotiationDO.Id,
                            
                            Answers = q.Answers.Select(a =>
                                (NegotiationAnswerVM) new NegotiationResponseAnswerVM
                                {
                                    Id = a.Id,
                                    Selected = userAnswerIDs.Contains(a.Id),
                                    CalendarEvents = a.Calendar == null ? new List<QuestionCalendarEventVM>() : a.Calendar.Events.Select(e => new QuestionCalendarEventVM
                                    {
                                        StartDate = e.StartDate,
                                        EndDate = e.EndDate
                                    }).ToList(),

                                    CalendarID = a.CalendarID,

                                    QuestionId = q.Id,
                                    Text = a.Text,
                                }).ToList()
                        };
                    }).ToList()
                };

                return View(model);
            }
        }

        [KwasantAuthorize(Roles = "Customer")]
        [HttpPost]
        public ActionResult ProcessResponse(NegotiationVM value)
        {
            if (!value.Id.HasValue)
                throw new HttpException(400, "Invalid parameter");

            AuthenticateUser(value.Id.Value);

            var userID = User.Identity.GetUserId();

            return View();
        }

        public void AuthenticateUser(int negotiationID)
        {
            //If this is a regular customer, verify that they're an attendee
            var userID = User.Identity.GetUserId();
            var user = new User();
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                if (!user.VerifyMinimumRole("Booker", userID, uow))
                    ConfirmUserInAttendees(uow, negotiationID);
            }
        }


        //verify that the person trying to view this negotiation is one of the attendees.
        public void ConfirmUserInAttendees(IUnitOfWork uow, int negotiationID)
        {
            if (!EnforceUserInAttendees)
                return;

            var negotiationDO = uow.NegotiationsRepository.GetByKey(negotiationID);
            if (negotiationDO == null)
                throw new HttpException(404, "Negotiation not found.");

            var attendees = negotiationDO.Attendees;
            var currentUserID = User.Identity.GetUserId();

            var existingUserDO = uow.UserRepository.GetQuery().FirstOrDefault(u => u.Id == currentUserID);
            if (existingUserDO == null)
                throw new HttpException(404, "We don't have a User record for you. ");

            var currentUserEmail = existingUserDO.EmailAddress.Address.ToLower();

            foreach (var attendee in attendees)
                if (attendee.EmailAddress.Address.ToLower() == currentUserEmail)
                    return;

            throw new HttpException(404, "You're not authorized to view information about this Negotiation");
        }

	}
}