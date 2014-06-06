using System;
using System.Diagnostics;
using System.Net;
using System.Net.Mail;
using System.Threading;
using Data.Entities;
using Data.Interfaces;
using KwasantCore.Services;
using KwasantCore.StructureMap;
using KwasantTest.Fixtures;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NUnit.Framework;
using StructureMap;

namespace KwasantTest.Workflow
{
    [TestFixture]
    public class WorkflowTests
    {
        public IUnitOfWork _uow;
        private FixtureData _fixture;
        private SmtpClient _smtpClient;
        private string _testUserEmail;

        [SetUp]
        public void Setup()
        {
            StructureMapBootStrapper.ConfigureDependencies(StructureMapBootStrapper.DependencyType.TEST);
            _uow = ObjectFactory.GetInstance<IUnitOfWork>();

            _fixture = new FixtureData();

            _testUserEmail = "lucreorganizer@gmail.com";
            _smtpClient = new SmtpClient("smtp.gmail.com", 587)
                              {
                                  EnableSsl = true,
                                  UseDefaultCredentials = false,
                                  Credentials = new NetworkCredential(_testUserEmail, "lucre1lucre1")
                              };
        }



        [Test]
        [Category("Workflow")]
        public void Workflow_CanReceiveInvitationOnEmail()
        {
            //SETUP
            var emailToRequestTimeout = TimeSpan.FromSeconds(10);

            var subject = string.Format("Event {0}", Guid.NewGuid());
            var start = DateTime.Now.AddDays(1);
            var end = start.AddHours(1);
            var body = string.Format("Event details:\r\nStart:{0}\r\nEnd:{1}", start, end);
            MailMessage message = new MailMessage(_testUserEmail, "kwa@sant.com", subject, body);

            //EXECUTE
            _smtpClient.Send(message);
            Stopwatch emailToRequestDuration = new Stopwatch();
            emailToRequestDuration.Start();
            BookingRequestDO request;
            do
            {
                Thread.Sleep(TimeSpan.FromSeconds(1));
                request =
                    _uow.BookingRequestRepository.FindOne(
                        br => br.From.Address == _testUserEmail && br.Subject == subject);
            } while (request == null && emailToRequestDuration.Elapsed < emailToRequestTimeout);
            emailToRequestDuration.Stop();

            if (request != null)
            {
                var lines = request.HTMLText.Split(new[] { "\r\n" }, StringSplitOptions.None);
                var startString = lines[1].Split(':')[1];
                var endString = lines[2].Split(':')[1];
                var e = new Event();
                _uow.EventRepository.Add(e.Create(request.Id, startString, endString));


            }

            //VERIFY
            //check that it was saved to the db
/*
            UserDO savedUserDO = _uow.UserRepository.GetQuery().FirstOrDefault(u => u.Id == curUserDO.Id);
            Assert.AreEqual(curUserDO.FirstName, savedUserDO.FirstName);
            Assert.AreEqual(curUserDO.EmailAddress, savedUserDO.EmailAddress);
*/
        }
    }
}
