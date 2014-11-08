﻿using System;
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
                    Conversations = conversationEmails.OrderBy(c => c.DateReceived).Select(e => new ConversationVM
                    {
                        FromEmailAddress = String.Format("From: {0}", e.From.Address),
                        DateRecieved = String.Format("{0}", e.DateReceived.TimeAgo()),
                        Body = e.HTMLText
                    }).ToList(),
                    FromName = curEmail.From.ToDisplayName(),
                    Subject = curEmail.Subject,
                    BookingRequestId = emailId,
                    EmailTo = String.Join(", ", curEmail.To.Select(a => a.Address)),
                    EmailCC = String.Join(", ", curEmail.CC.Select(a => a.Address)),
                    EmailBCC = String.Join(", ", curEmail.BCC.Select(a => a.Address)),
                    EmailAttachments = attachmentInfo,
                    ReadOnly = readonlyView.HasValue && readonlyView.Value,
                    Booker = booker
                };

                return PartialView("Show", bookingInfo);
            }
        }

        public JsonResult GetConversationMembers(int emailID)
        {
            var view = GetInfo(emailID);
            var model = view.Model as BookingRequestAdminVM;
            if (model == null)
                return new JsonResult { Data = null, JsonRequestBehavior = JsonRequestBehavior.AllowGet };

            return new JsonResult { Data = model.Conversations, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        public ActionResult Send()
        {
            return View((BookingRequestAdminVM)null);
        }

    }
}
