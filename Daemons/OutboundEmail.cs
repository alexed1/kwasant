using System;
using System.Linq;
using Daemons.EventExposers;
using Data.Constants;
using Data.Entities;
using Data.Entities.Constants;
using Data.Interfaces;
using Data.Repositories;
using KwasantCore.Managers;
using KwasantCore.Managers.APIManager.Packagers;
using StructureMap;
using Utilities.Logging;
using Microsoft.WindowsAzure;
using Data.Infrastructure;

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

                emailToUpdate.EmailStatusID = EmailStatus.Sent;
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

                emailToUpdate.EmailStatusID = EmailStatus.SendRejected;
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

                    emailToUpdate.EmailStatusID = EmailStatus.SendCriticalError;
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

                emailToUpdate.EmailStatusID = EmailStatus.Sent;
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

                emailToUpdate.EmailStatusID = EmailStatus.SendRejected;
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

                    emailToUpdate.EmailStatusID = EmailStatus.SendCriticalError;
                    unitOfWork.SaveChanges();
                });
        }

        public override int WaitTimeBetweenExecution
        {
            get { return (int) TimeSpan.FromSeconds(10).TotalMilliseconds; }
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
                foreach (EnvelopeDO envelope in envelopeRepository.FindList(e => e.Email.EmailStatusID == EmailStatus.Queued))
                {
                    using (var subUow = ObjectFactory.GetInstance<IUnitOfWork>())
                    {
                        try
                        {
                            IEmailPackager packager = ObjectFactory.GetNamedInstance<IEmailPackager>(envelope.Handler);
                            if (CloudConfigurationManager.GetSetting("ArchiveOutboundEmail") == "true")
                            {
                                EmailAddressDO outboundemailaddress = new EmailAddressDO(CloudConfigurationManager.GetSetting("ArchiveEmailAddress"));
                                envelope.Email.AddEmailRecipient(EmailParticipantType.Bcc, outboundemailaddress);
                            }
                            packager.Send(envelope);
                            numSent++;

                            var email = subUow.EmailRepository.GetQuery().First(e => e.Id == envelope.Email.Id); // probably "= envelope.Email" will work now...
                            email.EmailStatusID = EmailStatus.Dispatched;
                            subUow.SaveChanges();
                            string customerId = subUow.UserRepository.GetAll().Where(e => e.EmailAddress.Address == email.Recipients.First(c => c.EmailParticipantTypeID == EmailParticipantType.To).EmailAddress.Address).First().Id;
                            AlertManager.EmailSent(email.Id, customerId);
                        }
                        catch (StructureMapConfigurationException ex)
                        {
                            Logger.GetLogger().ErrorFormat("Unknown email packager: {0}", envelope.Handler);
                            throw new UnknownEmailPackagerException(string.Format("Unknown email packager: {0}", envelope.Handler), ex);
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
    }
}
