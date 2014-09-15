using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Net.Mail;
using Daemons;
using Data.Entities;
using Data.Interfaces;
using Data.States;
using KwasantCore.Managers.APIManager.Packagers;
using KwasantCore.Services;
using KwasantCore.StructureMap;
using KwasantTest.Fixtures;
using Moq;
using NUnit.Framework;
using StructureMap;

namespace KwasantTest.Daemons
{
    [TestFixture]
    public class OutboundEmailTests
    {
        private FixtureData _fixtureData;
        private IUnitOfWork _uow;
        private OutboundEmail _outboundEmailDaemon;

        [SetUp]
        public void Setup()
        {
            StructureMapBootStrapper.ConfigureDependencies(StructureMapBootStrapper.DependencyType.TEST);
            _uow = ObjectFactory.GetInstance<IUnitOfWork>();
            _fixtureData = new FixtureData();
            _outboundEmailDaemon = new OutboundEmail();
        }

        [Test]
        [Category("OutboundEmail")]
        public void CanSendGmailEnvelope()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                Console.WriteLine(AppDomain.CurrentDomain.SetupInformation.ConfigurationFile);
                Debug.WriteLine(AppDomain.CurrentDomain.SetupInformation.ConfigurationFile);

                // SETUP
                var email = _fixtureData.TestEmail1();

                uow.EmailRepository.Add(email);

                // EXECUTE
                var emailService = new Email(_uow, email);
                var envelope = emailService.Send();

                uow.SaveChanges();

                //adding user for alerts at outboundemail.cs  //If we don't add user, AlertManager at outboundemail generates error and test fails.
                AddNewTestCustomer(email.From);

                var mockEmailer = new Mock<IEmailPackager>();
                mockEmailer.Setup(a => a.Send(envelope)).Verifiable();
                ObjectFactory.Configure(
                    a => a.For<IEmailPackager>().Use(mockEmailer.Object).Named(EnvelopeDO.GmailHander));
                DaemonTests.RunDaemonOnce(_outboundEmailDaemon);

                // VERIFY
                mockEmailer.Verify(a => a.Send(envelope), "OutboundEmail daemon didn't dispatch email via Gmail.");
            }
        }

        [Test]
        [Category("OutboundEmail")]
        public void CanSendMandrillEnvelope()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                // SETUP
                var email = _fixtureData.TestEmail1();

                // EXECUTE
                var emailService = new Email(_uow, email);
                var envelope = emailService.SendTemplate("template", email, null);
                uow.SaveChanges();

                //adding user for alerts at outboundemail.cs  //If we don't add user, AlertManager at outboundemail generates error and test fails.
                AddNewTestCustomer(email.From);
                
                var mockEmailer = new Mock<IEmailPackager>();
                mockEmailer.Setup(a => a.Send(envelope)).Verifiable();
                ObjectFactory.Configure(
                    a => a.For<IEmailPackager>().Use(mockEmailer.Object).Named(EnvelopeDO.MandrillHander));
                DaemonTests.RunDaemonOnce(_outboundEmailDaemon);

                // VERIFY
                mockEmailer.Verify(a => a.Send(envelope), "OutboundEmail daemon didn't dispatch email via Mandrill.");
            }
        }

        [Test]
        [Category("OutboundEmail")]
        public void FailsToSendInvalidEnvelope()
        {
            // SETUP
            var email = _fixtureData.TestEmail1();

            // EXECUTE
            var emailService = new Email(_uow, email);
            var envelope = emailService.SendTemplate("template", email, null);
            
            envelope.Handler = "INVALID EMAIL PACKAGER";
            _uow.SaveChanges();

            //adding user for alerts at outboundemail.cs  //If we don't add user, AlertManager at outboundemail generates error and test fails.
            AddNewTestCustomer(email.From);

            var mockMandrillEmailer = new Mock<IEmailPackager>();
            mockMandrillEmailer.Setup(a => a.Send(envelope)).Throws<ApplicationException>(); // shouldn't be invoked
            ObjectFactory.Configure(a => a.For<IEmailPackager>().Use(mockMandrillEmailer.Object).Named(EnvelopeDO.MandrillHander));
            var mockGmailEmailer = new Mock<IEmailPackager>();
            mockGmailEmailer.Setup(a => a.Send(envelope)).Throws<ApplicationException>(); // shouldn't be invoked
            ObjectFactory.Configure(a => a.For<IEmailPackager>().Use(mockGmailEmailer.Object).Named(EnvelopeDO.GmailHander));
            
            // VERIFY
            Assert.Throws<UnknownEmailPackagerException>(
                () => DaemonTests.RunDaemonOnce(_outboundEmailDaemon),
                "OutboundEmail daemon didn't throw an exception for invalid EnvelopeDO.");
        }

        private void AddNewTestCustomer(EmailAddressDO emailAddress)
        {
            emailAddress.Recipients = new List<RecipientDO>()
                {
                    new RecipientDO()
                    {
                        EmailAddress = Email.GenerateEmailAddress(_uow, new MailAddress("joetest2@edelstein.org")),
                        EmailParticipantType = EmailParticipantType.To
                    }
                };            
            var role = new Role();
            role.Add(_uow, _fixtureData.TestRole());
            var u = new UserDO();
            var user = new User();
            UserDO currUserDO = new UserDO();
            currUserDO.EmailAddress = emailAddress;
            _uow.UserRepository.Add(currUserDO);
        }
    }
}
