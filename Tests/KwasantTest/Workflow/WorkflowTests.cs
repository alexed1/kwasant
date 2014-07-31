using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Mail;
using System.Threading;
using Daemons;
using Data.Entities;
using Data.Entities.Constants;
using Data.Entities.Enumerations;
using Data.Interfaces;
using KwasantCore.Services;
using KwasantCore.StructureMap;
using KwasantICS.DDay.iCal;
using KwasantTest.Daemons;
using NUnit.Framework;
using S22.Imap;
using StructureMap;
using Utilities;

namespace KwasantTest.Workflow
{
    [TestFixture]
    public class WorkflowTests
    {
        private IUnitOfWork _uow;
        private string _testUserEmail;
        private string _testUserEmailPassword;
        private string _archivePollEmail;
        private string _archivePollPassword;

        [SetUp]
        public void Setup()
        {
            StructureMapBootStrapper.ConfigureDependencies(StructureMapBootStrapper.DependencyType.TEST);
            _uow = ObjectFactory.GetInstance<IUnitOfWork>();

            _testUserEmail = ConfigRepository.Get("OutboundUserName");
            _testUserEmailPassword = ConfigRepository.Get("OutboundUserPassword");

            _archivePollEmail = ConfigRepository.Get("ArchivePollEmailAddress");
            _archivePollPassword = ConfigRepository.Get("ArchivePollEmailPassword");
        }



        [Test, Ignore]
        [Category("Workflow")]
        public void Workflow_CanReceiveInvitationOnEmailInTime()
        {
            //SETUP
            var emailToRequestTimeout = TimeSpan.FromSeconds(60);
            var requestToEmailTimeout = TimeSpan.FromSeconds(60);
            var totalOperationTimeout = TimeSpan.FromSeconds(120);

            var subject = string.Format("Event {0}", Guid.NewGuid());
            var now = DateTimeOffset.Now;
            // iCal truncates time up to seconds so we need to truncate as well to be able to compare time
            var start = new DateTimeOffset(now.Ticks / TimeSpan.TicksPerSecond * TimeSpan.TicksPerSecond, now.Offset).AddDays(1);
            var end = start.AddHours(1);
            const string startPrefix = "Start:";
            const string endPrefix = "End:";
            var body = string.Format("Event details:\r\n{0}{1}\r\n{2}{3}", startPrefix, start, endPrefix, end);

            var emailDO = new EmailDO()
            {
                From = Email.GenerateEmailAddress(_uow, new MailAddress(_testUserEmail)),
                Recipients = new List<RecipientDO>()
                {
                    new RecipientDO()
                    {
                        EmailAddress = Email.GenerateEmailAddress(_uow, new MailAddress("kwasantintegration@gmail.com")),
                        EmailParticipantTypeID = EmailParticipantType.To
                    }
                },
                Subject = subject,
                PlainText = body,
                HTMLText = body
            };

            _uow.EmailRepository.Add(emailDO);

            //adding user for alerts at outboundemail.cs  //If we don't add user, AlertManager at outboundemail generates error and test fails.
            AddNewTestCustomer(emailDO.From);

            var emailService = new Email(_uow, emailDO);

            Stopwatch totalOperationDuration = new Stopwatch();
            Stopwatch emailToRequestDuration = new Stopwatch();
            Stopwatch requestToEmailDuration = new Stopwatch();

            InboundEmail inboundDaemon = new InboundEmail();
            OutboundEmail outboundDaemon = new OutboundEmail();

            ImapClient client = new ImapClient("imap.gmail.com", 993, _testUserEmail.Split('@')[0], _testUserEmailPassword, AuthMethod.Login, true);

            //EXECUTE
            emailService.Send();

            _uow.SaveChanges();

            DaemonTests.RunDaemonOnce(outboundDaemon);

            totalOperationDuration.Start();
            emailToRequestDuration.Start();

            BookingRequestDO request;
            do
            {
                DaemonTests.RunDaemonOnce(inboundDaemon);
                request =
                    _uow.BookingRequestRepository.FindOne(
                        br => br.From.Address == _testUserEmail && br.Subject == subject);
            } while (request == null && emailToRequestDuration.Elapsed < emailToRequestTimeout);
            emailToRequestDuration.Stop();

            if (request != null)
            {
                var lines = request.HTMLText.Split(new[] { "\r\n" }, StringSplitOptions.None);
                var startString = lines[1].Remove(0, startPrefix.Length);
                var endString = lines[2].Remove(0, endPrefix.Length);
                var e = new Event();
                var edo = e.Create(_uow, request.Id, startString, endString);
                edo.CreatedByID = "1";
                edo.Description = "test event description";
                _uow.EventRepository.Add(edo);
                e.Process(_uow, edo);
                _uow.SaveChanges();

                requestToEmailDuration.Start();

                DaemonTests.RunDaemonOnce(outboundDaemon);

                MailMessage inviteMessage = null;
                uint inviteMessageId = 0;
                do
                {
                    Thread.Sleep(TimeSpan.FromSeconds(1));
                    var uids = client.Search(SearchCondition.Unseen()).ToList();
                    foreach (var uid in uids)
                    {
                        var curMessage = client.GetMessage(uid);
                        var icsView = curMessage.AlternateViews
                            .FirstOrDefault(v => string.Equals(v.ContentType.MediaType, "application/ics"));
                        if (icsView != null)
                        {
                            var cal = iCalendar.LoadFromStream(icsView.ContentStream).FirstOrDefault();
                            if (cal != null && cal.Events.Count > 0 &&
                                cal.Events[0].Start.Value == start &&
                                cal.Events[0].End.Value == end)
                            {
                                inviteMessageId = uid;
                                inviteMessage = curMessage;
                                break;
                            }
                        }
                    }
                } while (inviteMessage == null && requestToEmailDuration.Elapsed < requestToEmailTimeout);
                requestToEmailDuration.Stop();

                var requestMessages = client.Search(SearchCondition.Subject(subject)).ToList();
                client.DeleteMessages(requestMessages);

                if (inviteMessage != null)
                {
                    client.DeleteMessage(inviteMessageId);
                }
            }
            totalOperationDuration.Stop();

            //VERIFY
            //check timeouts
            Assert.Less(emailToRequestDuration.Elapsed, emailToRequestTimeout, "Email to BookingRequest conversion timed out.");
            Assert.Less(requestToEmailDuration.Elapsed, requestToEmailTimeout, "BookingRequest to Invitation conversion timed out.");
            Assert.Less(totalOperationDuration.Elapsed, totalOperationTimeout, "Workflow timed out.");
        }


        [Test, Ignore]
        [Category("Workflow")]
        public void Workflow_CanAddBcctoOutbound()
        {
            var requestToEmailTimeout = TimeSpan.FromSeconds(60);
            Stopwatch requestToEmailDuration = new Stopwatch();

            var subject = string.Format("Bcc Test {0}", Guid.NewGuid());
            var now = DateTimeOffset.Now;
            // iCal truncates time up to seconds so we need to truncate as well to be able to compare time
            var start = new DateTimeOffset(now.Ticks / TimeSpan.TicksPerSecond * TimeSpan.TicksPerSecond, now.Offset).AddDays(1);
            var end = start.AddHours(1);
            const string startPrefix = "Start:";
            const string endPrefix = "End:";
            var body = string.Format("Bcc Test details:\r\n{0}{1}\r\n{2}{3}", startPrefix, start, endPrefix, end);

            var emailDO = new EmailDO()
            {
                From = Email.GenerateEmailAddress(_uow, new MailAddress(_testUserEmail)),
                Recipients = new List<RecipientDO>()
                {
                    new RecipientDO()
                    {
                        EmailAddress = Email.GenerateEmailAddress(_uow, new MailAddress("kwasantintegration@gmail.com")),
                        EmailParticipantTypeID = EmailParticipantType.To
                    }
                },
                Subject = subject,
                PlainText = body,
                HTMLText = body,
                EmailStatusID = EmailStatus.Queued
            };

            _uow.EmailRepository.Add(emailDO);

            var envelope = new EnvelopeDO()
            {
                Email = emailDO,
                Handler = EnvelopeDO.GmailHander
            };
            _uow.EnvelopeRepository.Add(envelope);

            //adding user for alerts at outboundemail.cs  //If we don't add user, AlertManager at outboundemail generates error and test fails.
            AddNewTestCustomer(emailDO.From);

            OutboundEmail outboundDaemon = new OutboundEmail();
            DaemonTests.RunDaemonOnce(outboundDaemon);
            ImapClient client = new ImapClient("imap.gmail.com", 993, _archivePollEmail, _archivePollPassword, AuthMethod.Login, true);
            _uow.SaveChanges();
            
            requestToEmailDuration.Start();

            int mailcount = 0;
            do
            {
                Thread.Sleep(TimeSpan.FromSeconds(1));
            var requestMessages = client.Search(SearchCondition.Subject(subject)).ToList();
                if (requestMessages.Count() > 0)
                {
            client.DeleteMessages(requestMessages);
                    mailcount = requestMessages.Count();
                    break;
                }
            } while (requestToEmailDuration.Elapsed < requestToEmailTimeout);
            requestToEmailDuration.Stop();

            Assert.AreEqual(1, mailcount);
        }

        private void AddNewTestCustomer(EmailAddressDO emailAddress)
        {
            var role = new Role();
            role.Add(_uow, new KwasantTest.Fixtures.FixtureData().TestRole());
            var u = new UserDO();
            var user = new User();
            UserDO currUserDO = new UserDO();
            currUserDO.EmailAddress = emailAddress;
            currUserDO.Calendars = new List<CalendarDO>() { 
                new CalendarDO()    {
                        Id=1,
                        Name="test"
                }
            };
            CalendarDO t = new CalendarDO();
            t.Name = "test";
            t.Id = 1;
            _uow.UserRepository.Add(currUserDO);
        }
    }
}
