using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading;
using Daemons;
using Data.Entities;
using Data.Entities.Enumerations;
using Data.Interfaces;
using KwasantCore.Services;
using KwasantCore.StructureMap;
using KwasantICS.DDay.iCal;
using KwasantTest.Daemons;
using KwasantTest.Fixtures;
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
        /*
                private FixtureData _fixture;
                private SmtpClient _smtpClient;
        */
        private string _testUserEmail;
        private string _testUserEmailPassword;

        [SetUp]
        public void Setup()
        {
            StructureMapBootStrapper.ConfigureDependencies(StructureMapBootStrapper.DependencyType.TEST);
            _uow = ObjectFactory.GetInstance<IUnitOfWork>();

            _testUserEmail = ConfigRepository.Get("OutboundUserName");
            _testUserEmailPassword = ConfigRepository.Get("OutboundUserPassword");
        }



        [Test]
        [Category("Workflow")]
        public void Workflow_CanReceiveInvitationOnEmailInTime()
        {
            //SETUP
            var emailToRequestTimeout = TimeSpan.FromSeconds(30);
            var requestToEmailTimeout = TimeSpan.FromSeconds(30);
            var totalOperationTimeout = TimeSpan.FromSeconds(60);

            var subject = string.Format("Event {0}", Guid.NewGuid());
            var now = DateTime.Now;
            var start = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second).AddDays(1);
            var end = start.AddHours(1);
            const string startPrefix = "Start:";
            const string endPrefix = "End:";
            var body = string.Format("Event details:\r\n{0}{1}\r\n{2}{3}", startPrefix, start, endPrefix, end);
            var emailService = new Email(_uow, 
                new EmailDO()
                    {
                        From = Email.GenerateEmailAddress(_uow, new MailAddress(_testUserEmail)),
                        Recipients = new List<RecipientDO>()
                                         {
                                             new RecipientDO()
                                                 {
                                                     EmailAddress = Email.GenerateEmailAddress(_uow, new MailAddress("kwasantintakeclone@gmail.com")),
                                                     Type = EmailParticipantType.TO
                                                 }
                                         },
                        Subject = subject,
                        PlainText = body,
                        HTMLText = body
                    });

            Stopwatch totalOperationDuration = new Stopwatch();
            Stopwatch emailToRequestDuration = new Stopwatch();
            Stopwatch requestToEmailDuration = new Stopwatch();

            //EXECUTE
            emailService.Send();

            totalOperationDuration.Start();
            emailToRequestDuration.Start();

            BookingRequestDO request;
            do
            {
                InboundEmail inboundDaemon = new InboundEmail();
                DaemonTests.RunDaemonOnce(inboundDaemon);
                request =
                    _uow.BookingRequestRepository.FindOne(
                        br => br.From.Address == _testUserEmail && br.Subject == subject);
            } while (request == null && emailToRequestDuration.Elapsed < emailToRequestTimeout);
            emailToRequestDuration.Stop();

            if (request != null)
            {
                var lines = request.HTMLText.Split(new[] {"\r\n"}, StringSplitOptions.None);
                var startString = lines[1].Remove(0, startPrefix.Length);
                var endString = lines[2].Remove(0, endPrefix.Length);
                var e = new Event();
                var edo = e.Create(_uow, request.Id, startString, endString);
                edo.Description = "test event description";
                _uow.EventRepository.Add(edo);
                e.Update(_uow, edo);
                _uow.SaveChanges();

                requestToEmailDuration.Start();

                OutboundEmail outboundDaemon = new OutboundEmail();
                DaemonTests.RunDaemonOnce(outboundDaemon);

                MailMessage inviteMessage = null;
                do
                {
                    Thread.Sleep(TimeSpan.FromSeconds(1));
                    ImapClient client = new ImapClient("imap.gmail.com", 993, _testUserEmail.Split('@')[0], _testUserEmailPassword, AuthMethod.Login, true);
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
                                inviteMessage = curMessage;
                                break;
                            }
                        }
                    }
                } while (inviteMessage == null && requestToEmailDuration.Elapsed < requestToEmailTimeout);
                requestToEmailDuration.Stop();
            }
            totalOperationDuration.Stop();

            //VERIFY
            //check timeouts
            Assert.Less(emailToRequestDuration.Elapsed, emailToRequestTimeout, "Email to BookingRequest conversion timed out.");
            Assert.Less(requestToEmailDuration.Elapsed, requestToEmailTimeout, "BookingRequest to Invitation conversion timed out.");
            Assert.Less(totalOperationDuration.Elapsed, totalOperationTimeout, "Workflow timed out.");
        }
    }
}
