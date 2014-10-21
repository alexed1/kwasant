using System;
using System.Collections.Generic;
using System.Web.Mvc;
using Data.Entities;
using Data.Interfaces;
using Data.Repositories;
using KwasantCore.Managers;
using KwasantCore.Managers.APIManagers.Packagers.Kwasant;
using KwasantWeb.ViewModels;
using StructureMap;
using Utilities;
using System.Linq;

namespace KwasantWeb.Controllers
{
    [KwasantAuthorize(Roles = "Booker")]
    public class EmailController : Controller
    {
        private IUnitOfWork _uow;
        private IBookingRequestDORepository curBookingRequestRepository;
        private KwasantPackager API;


        public EmailController()
        {
            _uow = ObjectFactory.GetInstance<IUnitOfWork>();
            curBookingRequestRepository = _uow.BookingRequestRepository;
            API = new KwasantPackager();
        }

        // GET: /Email/
        [HttpGet]
        public string ProcessGetEmail(string requestString)
        {
            Dictionary<string, string> param = new Dictionary<string, string>();
            API.UnpackGetEmail(requestString, out param);
            EmailDO thisEmailDO = curBookingRequestRepository.GetByKey(param["Id"].ToInt());
            return API.PackResponseGetEmail(thisEmailDO);
        }

        public ActionResult GetInfo(int emailId)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var curEmail = uow.EmailRepository.GetAll().Where(e => e.Id == emailId || e.ConversationId == emailId).OrderByDescending(e => e.DateReceived).First();
                const string fileViewURLStr = "/Api/GetAttachment.ashx?AttachmentID={0}";

                var attachmentInfo = String.Join("<br />",
                            curEmail.Attachments.Select(
                                attachment =>
                                "<a href='" + String.Format(fileViewURLStr, attachment.Id) + "' target='" +
                                attachment.OriginalName + "'>" + attachment.OriginalName + "</a>"));

                string booker = "none";
                string bookerId = uow.BookingRequestRepository.GetByKey(emailId).BookerID;
                if (bookerId != null)
                {
                    var curbooker = uow.UserRepository.GetByKey(bookerId);
                    if (curbooker.EmailAddress != null)
                        booker = curbooker.EmailAddress.Address;
                    else
                        booker = curbooker.FirstName;
                }

                BookingRequestAdminVM bookingInfo = new BookingRequestAdminVM
                {
                    BookingRequestId = emailId,
                    CurEmailData = new EmailDO
                    {
                        Attachments = curEmail.Attachments,
                        From = curEmail.From,
                        Recipients = curEmail.Recipients,
                        HTMLText = curEmail.HTMLText,
                        Id = curEmail.Id,
                        FromID = curEmail.FromID,
                        DateCreated = curEmail.DateCreated,
                        Subject = curEmail.Subject
                    },
                    EmailTo = String.Join(", ", curEmail.To.Select(a => a.Address)),
                    EmailCC = String.Join(", ", curEmail.CC.Select(a => a.Address)),
                    EmailBCC = String.Join(", ", curEmail.BCC.Select(a => a.Address)),
                    EmailAttachments = attachmentInfo,
                    Booker = booker
                };

                return PartialView("Show", bookingInfo);
            }
        }
    }
}
