using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Data.Interfaces;
using KwasantCore.Managers;
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
            ConfirmUserInAttendees(negotiationID);

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

                            CalendarEvents = new List<QuestionCalendarEventVM>(),

                            CalendarID = q.CalendarID,
                            Answers = q.Answers.Select(a =>
                                (NegotiationAnswerVM) new NegotiationResponseAnswerVM
                                {
                                    Id = a.Id,
                                    Selected = userAnswerIDs.Contains(a.Id),
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
        public ActionResult ProcessResponse(NegotiationResponsePostData value)
        {
            ConfirmUserInAttendees(value.NegotiationID);

            var userID = User.Identity.GetUserId();

            //To be re-done
            //if (value.Responses != null)
            //{
            //    using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            //    {
            //        var negDO = uow.NegotiationsRepository.GetByKey(value.NegotiationID);
            //        var availableAnswers = negDO.
            //        foreach (var response in value.Responses)
            //        {
                        
            //            var questionResponse = uow.QuestionResponseRepository.GetQuery().FirstOrDefault(qr => qr.QuestionID == response.QuestionID);
            //            if (questionResponse == null)
            //            {
            //                questionResponse = new QuestionResponseDO();
            //                uow.QuestionResponseRepository.Add(questionResponse);
            //            }

            //            questionResponse.QuestionID = response.QuestionID;
            //            questionResponse.AnswerID = response.AnswerID;
            //            questionResponse.UserID = userID;
            //        }
            //        uow.SaveChanges();
            //    }
            //}

            return View();
        }

        public void ConfirmUserInAttendees(int negotiationID)
        {
            if (!EnforceUserInAttendees)
                return;

            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var negotiationDO = uow.NegotiationsRepository.GetByKey(negotiationID);
                if(negotiationDO == null)
                    throw new HttpException(404, "Negotiation not found.");

                var attendees = negotiationDO.Attendees;
                var currentUserID = User.Identity.GetUserId();

                var existingUserDO = uow.UserRepository.GetQuery().FirstOrDefault(u => u.Id == currentUserID);
                if (existingUserDO == null)
                    throw new HttpException(404, "User not found.");

                var currentUserEmail = existingUserDO.EmailAddress.Address.ToLower();

                foreach(var attendee in attendees)
                    if (attendee.EmailAddress.Address.ToLower() == currentUserEmail)
                        return;

                throw new HttpException(404, "Negotiation not found.");
            }
        }

        public class NegotiationResponsePostData
        {
            public int NegotiationID { get; set; }
            public List<NegotiationQuestionAnswerPair> Responses { get; set; }
        }

        public class NegotiationQuestionAnswerPair
        {
            public int QuestionID { get; set; }
            public int AnswerID { get; set; }
        }
	}
}