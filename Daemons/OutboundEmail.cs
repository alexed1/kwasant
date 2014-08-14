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
using Utilities.Logging;
using Microsoft.WindowsAzure;
using Data.Infrastructure;
using System.Collections.Generic;

namespace Daemons
{
    public class OutboundEmail : Daemon
    {

        private string logString;

        public OutboundEmail()
        {
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
                EmailRepository emailRepository = unitOfWork.EmailRepository;
                var emailToUpdate = emailRepository.GetQuery().FirstOrDefault(e => e.Id == emailID);
                if (emailToUpdate == null)
                {
                    Logger.GetLogger()
                        .Error("Email id " + emailID +
                               " recieved a callback saying it was rejected from Mandrill, but the email was not found in our database");
                    return;
                }

                Logger.GetLogger()
                    .Error(String.Format("Email was rejected with id '{0}'. Reason: {1}", emailID, reason));

                emailToUpdate.EmailStatus = EmailState.SendRejected;
                unitOfWork.SaveChanges();
            });

            RegisterEvent<int, string, string, int>(MandrillPackagerEventHandler.EmailCriticalError,
                (errorCode, name, message, emailID) =>
                {
                    IUnitOfWork unitOfWork = ObjectFactory.GetInstance<IUnitOfWork>();
                    EmailRepository emailRepository = unitOfWork.EmailRepository;
                    var emailToUpdate = emailRepository.GetQuery().FirstOrDefault(e => e.Id == emailID);
                    if (emailToUpdate == null)
                    {
                        Logger.GetLogger()
                            .Error("Email id " + emailID +
                                   " recieved a callback saying it recieved a critical error from Mandrill, but the email was not found in our database");
                        return;
                    }

                    Logger.GetLogger()
                        .Error(String.Format("Email failed. Error code: {0}. Name: {1}. Message: {2}. EmailID: {3}",
                            errorCode, name, message, emailID));

                    emailToUpdate.EmailStatus = EmailState.SendCriticalError;
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
                EmailRepository emailRepository = unitOfWork.EmailRepository;
                var emailToUpdate = emailRepository.GetQuery().FirstOrDefault(e => e.Id == emailID);
                if (emailToUpdate == null)
                {
                    Logger.GetLogger()
                        .Error("Email id " + emailID +
                               " recieved a callback saying it was rejected from Gmail, but the email was not found in our database");
                    return;
                }

                Logger.GetLogger()
                    .Error(String.Format("Email was rejected with id '{0}'. Reason: {1}", emailID, reason));

                emailToUpdate.EmailStatus = EmailState.SendRejected;
                unitOfWork.SaveChanges();
            });

            RegisterEvent<int, string, string, int>(GmailPackagerEventHandler.EmailCriticalError,
                (errorCode, name, message, emailID) =>
                {
                    IUnitOfWork unitOfWork = ObjectFactory.GetInstance<IUnitOfWork>();
                    EmailRepository emailRepository = unitOfWork.EmailRepository;
                    var emailToUpdate = emailRepository.GetQuery().FirstOrDefault(e => e.Id == emailID);
                    if (emailToUpdate == null)
                    {
                        Logger.GetLogger()
                            .Error("Email id " + emailID +
                                   " recieved a callback saying it recieved a critical error from Gmail, but the email was not found in our database");
                        return;
                    }

                    Logger.GetLogger()
                        .Error(String.Format("Email failed. Error code: {0}. Name: {1}. Message: {2}. EmailID: {3}",
                            errorCode, name, message, emailID));

                    emailToUpdate.EmailStatus = EmailState.SendCriticalError;
                    unitOfWork.SaveChanges();
                });
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
                EnvelopeRepository envelopeRepository = unitOfWork.EnvelopeRepository;
                EventRepository eventRepository = unitOfWork.EventRepository;
                CommunicationManager _comm = new CommunicationManager();
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
                            if (CloudConfigurationManager.GetSetting("ArchiveOutboundEmail") == "true")
                            {
                                EmailAddressDO outboundemailaddress = new EmailAddressDO(CloudConfigurationManager.GetSetting("ArchiveEmailAddress"));
                                envelope.Email.AddEmailRecipient(EmailParticipantType.Bcc, outboundemailaddress);
                            }

                            //Removing email address which are not test account in debug mode
                            #if DEBUG
                            {
                                if (RemoveRecipients(envelope.Email.Recipients.ToList(), envelope, subUow))
                                {
                                    Logger.GetLogger().Info("Removed one or more email recipients because they were not test accounts");
                                }
                            }
                            #endif
                            packager.Send(envelope);
                            numSent++;

                            var email = envelope.Email; // subUow.EmailRepository.GetQuery().First(e => e.Id == envelope.Email.Id);
                            email.EmailStatus = EmailState.Dispatched;
                            subUow.SaveChanges();

                            string customerId = subUow.UserRepository.GetAll().Where(e => e.EmailAddress.Address == email.Recipients.First(c => c.EmailParticipantType == EmailParticipantType.To).EmailAddress.Address).First().Id;
                            AlertManager.EmailSent(email.Id, customerId);
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
        private bool RemoveRecipients(List<RecipientDO> recipientList, EnvelopeDO envelope, IUnitOfWork uow)
        {
            bool isRecipientRemoved = false;
            foreach (RecipientDO recipient in recipientList)
            {
                UserDO user = uow.UserRepository.FindOne(e => e.EmailAddress.Address == recipient.EmailAddress.Address);
                if (user != null)
                {
                    if (!user.TestAccount)
                    {
                        envelope.Email.Recipients.RemoveAll(s => s.EmailAddress.Address == recipient.EmailAddress.Address);
                        isRecipientRemoved = true;
                    }
                }
            }
            return isRecipientRemoved;
        }
    }
}
