using System;
using System.Linq;
using Daemons.EventExposers;
using Data.Entities;
using Data.Interfaces;
using Data.Repositories;
using Data.States;
using KwasantCore.Managers;
using KwasantCore.Managers.APIManager.Packagers;
using StructureMap;
using Utilities;
using Utilities.Logging;
using Microsoft.WindowsAzure;
using Data.Infrastructure;
using System.Collections.Generic;
using KwasantCore.Services;

namespace Daemons
{
    public class OutboundEmail : Daemon
    {

        private string logString;

        public OutboundEmail()
        {

            #region RegisterEvent Calls
            RegisterEvent<string, int>(MandrillPackagerEventHandler.EmailSent, (id, emailID) =>
            {
                IUnitOfWork unitOfWork = ObjectFactory.GetInstance<IUnitOfWork>();
                EmailRepository emailRepository = unitOfWork.EmailRepository;
                var emailToUpdate = emailRepository.GetQuery().FirstOrDefault(e => e.Id == emailID);
                if (emailToUpdate == null)
                {
                    Logger.GetLogger()
                        .Error("Email id " + emailID +
                               " recieved a callback saying it was sent from Mandrill, but the email was not found in our database");
                    return;
                }

                emailToUpdate.EmailStatus = EmailState.Sent;
                unitOfWork.SaveChanges();
            });

            RegisterEvent<string, string, int>(MandrillPackagerEventHandler.EmailRejected, (id, reason, emailID) =>
            {
                IUnitOfWork unitOfWork = ObjectFactory.GetInstance<IUnitOfWork>();
                string logMessage = String.Format("Email was rejected with id '{0}'. Reason: {1}", emailID, reason);
                var emailToUpdate = ProcessMandrillError(logMessage, emailID);
                emailToUpdate.EmailStatus = EmailState.SendRejected;
                unitOfWork.SaveChanges();
            });

            RegisterEvent<int, string, string, int>(MandrillPackagerEventHandler.EmailCriticalError,
                (errorCode, name, message, emailID) =>
                {
                    IUnitOfWork unitOfWork = ObjectFactory.GetInstance<IUnitOfWork>();
                  

                    string logMessage = String.Format("Email failed. Error code: {0}. Name: {1}. Message: {2}. EmailID: {3}", errorCode, name, message, emailID);
                    var emailToUpdate = ProcessMandrillError(logMessage, emailID);

                    emailToUpdate.EmailStatus = EmailState.SendCriticalError;
                    AlertManager.Error_EmailSendFailure();
                    Email _email = ObjectFactory.GetInstance<Email>();
                    _email.SendAlertEmail();

                    unitOfWork.SaveChanges();
                });

            RegisterEvent<int>(GmailPackagerEventHandler.EmailSent, emailID =>
            {
                IUnitOfWork unitOfWork = ObjectFactory.GetInstance<IUnitOfWork>();
                EmailRepository emailRepository = unitOfWork.EmailRepository;
                var emailToUpdate = emailRepository.GetQuery().FirstOrDefault(e => e.Id == emailID);
                if (emailToUpdate == null)
                {
                    Logger.GetLogger()
                        .Error("Email id " + emailID +
                               " recieved a callback saying it was sent from Gmail, but the email was not found in our database");
                    return;
                }

                emailToUpdate.EmailStatus = EmailState.Sent;
                unitOfWork.SaveChanges();
            });

            RegisterEvent<string, int>(GmailPackagerEventHandler.EmailRejected, (reason, emailID) =>
            {
                IUnitOfWork unitOfWork = ObjectFactory.GetInstance<IUnitOfWork>();
               

                string logMessage = String.Format("Email was rejected with id '{0}'. Reason: {1}", emailID, reason);
                var emailToUpdate = ProcessMandrillError(logMessage, emailID);
                emailToUpdate.EmailStatus = EmailState.SendRejected;
                unitOfWork.SaveChanges();
            });

            RegisterEvent<int, string, string, int>(GmailPackagerEventHandler.EmailCriticalError,
                (errorCode, name, message, emailID) =>
                {
                    IUnitOfWork unitOfWork = ObjectFactory.GetInstance<IUnitOfWork>();
                    
                    string logMessage = String.Format("Email failed. Error code: {0}. Name: {1}. Message: {2}. EmailID: {3}", errorCode, name, message, emailID);
                    var emailToUpdate = ProcessMandrillError(logMessage, emailID);

                    emailToUpdate.EmailStatus = EmailState.SendCriticalError;
                    AlertManager.Error_EmailSendFailure();
                    Email _email = ObjectFactory.GetInstance<Email>();
                    _email.SendAlertEmail();
                    unitOfWork.SaveChanges();
                });
            #endregion
        }

        public override int WaitTimeBetweenExecution
        {
            get { return (int)TimeSpan.FromSeconds(10).TotalMilliseconds; }
        }

        protected override void Run()
        {
            while (ProcessNextEventNoWait())
            {
            }

            using (IUnitOfWork unitOfWork = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var configRepository = ObjectFactory.GetInstance<IConfigRepository>();
                EnvelopeRepository envelopeRepository = unitOfWork.EnvelopeRepository;
                EventRepository eventRepository = unitOfWork.EventRepository;
                var numSent = 0;
                foreach (EnvelopeDO curEnvelopeDO in envelopeRepository.FindList(e => e.Email.EmailStatus == EmailState.Queued))
                {
                    using (var subUow = ObjectFactory.GetInstance<IUnitOfWork>())
                    {
                        try
                        {
                            // we have to query EnvelopeDO one more time to have it loaded in subUow
                            var envelope = subUow.EnvelopeRepository.GetByKey(curEnvelopeDO.Id);
                            IEmailPackager packager = ObjectFactory.GetNamedInstance<IEmailPackager>(envelope.Handler);
                            if (configRepository.Get<bool>("ArchiveOutboundEmail"))
                            {
                                EmailAddressDO outboundemailaddress = subUow.EmailAddressRepository.GetOrCreateEmailAddress(configRepository.Get("ArchiveEmailAddress"), "Outbound Archive");
                                envelope.Email.AddEmailRecipient(EmailParticipantType.Bcc, outboundemailaddress);
                            }

                            ////Removing email address which are not test account in debug mode
                            //#if DEBUG
                            //{
                            //    if (RemoveRecipients(envelope.Email, subUow))
                            //    {
                            //        Logger.GetLogger().Info("Removed one or more email recipients because they were not test accounts");
                            //    }
                            //}
                            //#endif
                            packager.Send(envelope);
                            numSent++;

                            var email = envelope.Email; // subUow.EmailRepository.GetQuery().First(e => e.Id == envelope.Email.Id);
                            email.EmailStatus = EmailState.Dispatched;
                            subUow.SaveChanges();

                            foreach (var recipient in email.To)
                            {
                                var curUser = subUow.UserRepository.GetQuery()
                                    .FirstOrDefault(u => u.EmailAddressID == recipient.Id);
                                if (curUser != null)
                                {
                                    AlertManager.EmailSent(email.Id, curUser.Id);
                                }
                            }
                        }
                        catch (StructureMapConfigurationException ex)
                        {
                            Logger.GetLogger().ErrorFormat("Unknown email packager: {0}", curEnvelopeDO.Handler);
                            throw new UnknownEmailPackagerException(string.Format("Unknown email packager: {0}", curEnvelopeDO.Handler), ex);
                        }
                    }
                }

                if (numSent == 0)
                {
                    logString = "nothing sent";
                }
                else
                {
                    logString = "Emails sent:" + numSent;
                }

                Logger.GetLogger().Info(logString);
            }
        }
        private bool RemoveRecipients(EmailDO emailDO, IUnitOfWork uow)
        {
            bool isRecipientRemoved = false;
            var recipientList = emailDO.Recipients.ToList();
            
            foreach (RecipientDO recipient in recipientList)
            {
                UserDO user = uow.UserRepository.FindOne(e => e.EmailAddress.Address == recipient.EmailAddress.Address);
                if (user != null && !user.TestAccount)
                {
                    uow.RecipientRepository.Remove(recipient);
                    isRecipientRemoved = true;
                }
            }
            return isRecipientRemoved;
        }


        public EmailDO ProcessMandrillError(string logMessage, int emailID)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var emailRepository = uow.EmailRepository.GetQuery().FirstOrDefault(e => e.Id == emailID);
                if (emailRepository == null)
                {
                    Logger.GetLogger().Error(logMessage);
                }
                return emailRepository;
            }
        }
    }
}
