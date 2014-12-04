using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;
using Data.Entities;
using Data.Interfaces;
using Data.Repositories;
using Data.States;
using KwasantCore.Managers;
using KwasantCore.Managers.APIManagers.Packagers.Kwasant;
using KwasantWeb.ViewModels;
using StructureMap;
using Utilities;
using System.Linq;
using KwasantCore.Services;

namespace KwasantWeb.Controllers
{
    [KwasantAuthorize]
    public class EmailController : Controller
    {
        private IUnitOfWork _uow;
        private IBookingRequestDORepository curBookingRequestRepository;
        private KwasantPackager API;
        private readonly BookingRequest _br;


        public EmailController()
        {
            _uow = ObjectFactory.GetInstance<IUnitOfWork>();
            curBookingRequestRepository = _uow.BookingRequestRepository;
            API = new KwasantPackager();
            _br = new BookingRequest();
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

        public PartialViewResult GetInfo(int emailId, bool? readonlyView = null)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                EmailDO curEmail;

                List<EmailDO> conversationEmails = new List<EmailDO>();
                
                var emailDO = uow.EmailRepository.GetByKey(emailId);

                var bookingRequestDO = emailDO as BookingRequestDO;
                if (bookingRequestDO != null)
                {
                    curEmail = bookingRequestDO;
                    conversationEmails.Add(bookingRequestDO);
                    conversationEmails.AddRange(bookingRequestDO.ConversationMembers);
                }
                else
                {
                    curEmail = emailDO;
                    if (emailDO.Conversation != null)
                    {
                        conversationEmails.Add(emailDO.Conversation);
                        conversationEmails.AddRange(emailDO.Conversation.ConversationMembers);
                    }
                    else
                    {
                        conversationEmails.Add(curEmail);
                    }
                }

                //var curEmail = uow.EmailRepository.GetAll().Where(e => e.Id == emailId || e.ConversationId == emailId).OrderByDescending(e => e.DateReceived).First();
                const string fileViewURLStr = "/Api/GetAttachment.ashx?AttachmentID={0}";

                var attachmentInfo = String.Join("<br />",
                            curEmail.Attachments.Where(a => String.IsNullOrEmpty(a.ContentID)).Select(
                                attachment =>
                                "<a href='" + String.Format(fileViewURLStr, attachment.Id) + "' target='" +
                                attachment.OriginalName + "'>" + attachment.OriginalName + "</a>"));

                string booker = "none";
                if (bookingRequestDO != null)
                {
                    if (bookingRequestDO.Booker != null)
                    {
                        if (bookingRequestDO.Booker.EmailAddress != null)
                            booker = bookingRequestDO.Booker.EmailAddress.Address;
                        else
                            booker = bookingRequestDO.Booker.FirstName;
                    }
                }
                
                BookingRequestAdminVM bookingInfo = new BookingRequestAdminVM
                {
                    Conversations = conversationEmails.OrderBy(c => c.DateReceived).Select(e =>
                    {
                        var bodyText = e.HTMLText;
                        if (String.IsNullOrEmpty(bodyText))
                            bodyText = e.PlainText;
                        if (String.IsNullOrEmpty(bodyText))
                            bodyText = "[No Body]";
                        return new ConversationVM
                        {
                            FromEmailAddress = String.Format("From: {0}", e.From.Address),
                            DateRecieved = String.Format("{0}", e.DateReceived.TimeAgo()),
                            Body = bodyText
                        };
                    }).ToList(),
                    FromName = curEmail.From.ToDisplayName(),
                    Subject = String.IsNullOrEmpty(curEmail.Subject) ? "[No Subject]" : curEmail.Subject,
                    BookingRequestId = emailId,
                    EmailTo = String.Join(", ", curEmail.To.Select(a => a.Address)),
                    EmailCC = String.Join(", ", curEmail.CC.Select(a => a.Address)),
                    EmailBCC = String.Join(", ", curEmail.BCC.Select(a => a.Address)),
                    EmailAttachments = attachmentInfo,
                    ReadOnly = readonlyView.HasValue && readonlyView.Value,
                    Booker = booker,
                    LastUpdated = curEmail.LastUpdated
                };

                return PartialView("Show", bookingInfo);
            }
        }

        [HttpPost]
        public JsonResult GetConversationMembers(int emailID)
        {
            var view = GetInfo(emailID);
            var model = view.Model as BookingRequestAdminVM;
            if (model == null)
                return Json(null);

            return Json(model.Conversations);
        }

        private void SetCachedCallback(HttpSessionStateBase session, string token, Func<IUnitOfWork, EmailDO, ActionResult> callback)
        {
            session[token] = callback;
        }

        private Func<IUnitOfWork, EmailDO, ActionResult> GetCachedCallback(string token)
        {
            return Session[token] as Func<IUnitOfWork, EmailDO, ActionResult>;
        }

        public ViewResult DisplayEmail(HttpSessionStateBase session, CreateEmailVM emailVM, Func<IUnitOfWork, EmailDO, ActionResult> callback)
        {
            var token = Guid.NewGuid().ToString();
            emailVM.CallbackToken = token;
            SetCachedCallback(session, token, callback);
            return View("~/Views/Email/Send.cshtml", emailVM);
        }

        [HttpPost]
        public ActionResult HandleSend(SendEmailVM vm)
        {
            KwasantPackagedMessage verifyCheckoutMessage = _br.VerifyCheckOut(vm.BookingRequestId, this.GetUserId());
            if (verifyCheckoutMessage.Name == "valid")
            {
                var cachedCallback = GetCachedCallback(vm.CallbackToken);
                if (cachedCallback != null)
                {
                    using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
                    {
                        var emailDO = new EmailDO();

                        var configRepository = ObjectFactory.GetInstance<IConfigRepository>();
                        string fromAddress = configRepository.Get("EmailAddress_GeneralInfo");

                        emailDO.From = uow.EmailAddressRepository.GetOrCreateEmailAddress(fromAddress);
                        foreach (var to in vm.ToAddresses)
                            emailDO.AddEmailRecipient(EmailParticipantType.To, uow.EmailAddressRepository.GetOrCreateEmailAddress(to));
                        foreach (var cc in vm.CCAddresses)
                            emailDO.AddEmailRecipient(EmailParticipantType.Cc, uow.EmailAddressRepository.GetOrCreateEmailAddress(cc));
                        foreach (var bcc in vm.BCCAddresses)
                            emailDO.AddEmailRecipient(EmailParticipantType.Bcc, uow.EmailAddressRepository.GetOrCreateEmailAddress(bcc));

                        emailDO.HTMLText = vm.Body;
                        emailDO.PlainText = vm.Body;
                        emailDO.Subject = vm.Subject;

                        uow.EmailRepository.Add(emailDO);
                        cachedCallback(uow, emailDO);
                        return Json(verifyCheckoutMessage);
                    }
                }
            }
            return Json(verifyCheckoutMessage);
        }

        [HttpGet]
        public ActionResult Send()
        {
            return DisplayEmail(Session, new CreateEmailVM
            {
                ToAddresses = new List<string> { "rjrudman@gmail.com", "temp@gmail.com" },
                CCAddresses = new List<string> { "alex@gmail.com" },
                BCCAddresses = new List<string> { "max@gmail.com" },
                AddressBook = new List<string> { "kate@gmail.com", "temp@gmail.com" },
                RecipientsEditable = false,
                BCCHidden = true,
                CCHidden = true,
                InsertLinks = new List<CreateEmailVM.InsertLink>
                {
                    new CreateEmailVM.InsertLink
                    {
                        Id = "negLink",
                        DisplayName = "Insert Negotiation",
                        TextToInsert = "<negotiationLink />"
                    }
                },
                Subject = "New negotiation",
                Body = "Some text..",
            }, Send);
        }
        
        [HttpPost]
        public ActionResult Send(IUnitOfWork uow, EmailDO emailDO)
        {
            return Json(true);
        }
    }
}
