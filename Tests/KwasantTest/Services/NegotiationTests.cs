using Data.Entities;
using Data.Interfaces;
using KwasantCore.Services;
using KwasantTest.Fixtures;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NUnit.Framework;
using Daemons;
using KwasantTest.Utilities;
using S22.Imap;
using KwasantTest.Daemons;
using System.Collections.Generic;
using System.Net.Mail;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Mail;
using System.Threading;
using Daemons;
using Data.Entities;
using Data.Infrastructure;
using Data.Interfaces;
using Data.States;
using FluentValidation.Validators;
using KwasantCore.Services;
using KwasantCore.StructureMap;
using KwasantICS.DDay.iCal;
using KwasantTest.Daemons;
using KwasantTest.Fixtures;
using KwasantTest.Utilities;
using Moq;
using NUnit.Framework;
using S22.Imap;
using StructureMap;
using Utilities;

namespace KwasantTest.Services
{
    [TestFixture]
    public class NegotiationTests
    {
        private IUnitOfWork _uow;
        private string _testUserEmail;
        private string _testUserEmailPassword;
        private string _archivePollEmail;
        private string _archivePollPassword;
        private Email _emailService;
        private string _startPrefix;
        private string _endPrefix;     
        private FixtureData _fixture;
        private Negotiation _negotiation;

        private PollingEngine _polling;


        [SetUp]
        public void SetUp()
        {
            StructureMapBootStrapper.ConfigureDependencies(StructureMapBootStrapper.DependencyType.TEST);
            _uow = ObjectFactory.GetInstance<IUnitOfWork>();

            _negotiation = new Negotiation();
            _fixture = new FixtureData();
            _polling = new PollingEngine(_uow);
        }

   
        //This method takes a NegotiationId as input that must resolve to a Negotiation with status "Unresolved".
        //Build a list of Users based on the Attendee lists of the associated Events.
        //For each user, if there is not already a ClarificationRequestDO that has this UserId and this NeogtiationId, call ClarificationRequest#Create, get back a ClarificationRequestDO, and associate it with the Negotiation (You will need to add a property of type NegotiationDO to ClarificationRequestDO).
        //call ClarificationRequest#GenerateResponseURL
        //call ClarificationRequest#Send
        //This will generate an email with a link that takes the recipient to a response view. 
        [Test]
        [Category("IntegrationTests")]
        public void ITest_CanDispatchNegotiationEmail()
        {
            //Create a BookingRequest and two test Attendees
            //Create a Negotiation with a Question and an Answer
            //Save the Negotiation as if running ProcessSubmittedNegotiation
            //Verify that Clarification Request was received by Attendee
            //Verify that executing link in clarification request produces appropriate view

            BookingRequestDO testBR = _fixture.TestBookingRequest1();
            UserDO testUser1 = _fixture.TestUser1();
            AttendeeDO testAttendee1 = _fixture.TestAttendee1();
           


            string targetAddress = testUser1.EmailAddress.Address; // MUST BE integration@kwawant
            string targetPassword = "thorium65";
            ImapClient client = new ImapClient("imap.gmail.com", 993, targetAddress, targetPassword, AuthMethod.Login, true);
            InboundEmail inboundDaemon = new InboundEmail();
            inboundDaemon.username = targetAddress;
            inboundDaemon.password = targetPassword;

           // AttendeeDO testAttendee2 = _fixture.TestAttendee2();
            NegotiationDO testNegotiation = _fixture.TestNegotiation1();
            _uow.NegotiationsRepository.Add(testNegotiation);
            _uow.SaveChanges();


            //EXECUTE

            //Start Timer
            using (_polling.NewTimer(_polling.totalOperationTimeout, "Workflow"))
            {
                _negotiation.Process(testNegotiation);

                //make sure that the emails go out. this may not be necessary
                _polling.FlushOutboundEmailQueues();
            }

            //VERIFY

            //Verify that attendee has received an appropriate ClarificationRequest.
            //If attendee has received the CR, the first EmailDO should have certain characteristics
            ClarificationRequestDO foundCR = PollForClarificationRequest(testAttendee1, client, inboundDaemon);
            
            //check timeouts
            _polling.CheckTimeouts();

        }



        //Injected Query
        //examines the unread messages in an email account for one that appears to be a clarification request
        public static IEnumerable<EmailDO> InjectedQuery_FindClarificationRequest(EmailDO targetCriteria, List<EmailDO> unreadMessages)
        {

            List<EmailDO> matchingEmails =  unreadMessages.FindAll(um => um.Subject == targetCriteria.Subject && um.To.Equals(targetCriteria.To));
            return matchingEmails;
        }
        
        //Inject a query into the email polling engine that looks for a clarification request with characteristics matching the specified Attendee
        public ClarificationRequestDO PollForClarificationRequest(AttendeeDO testAttendee, ImapClient client, InboundEmail inboundDaemon)
        {
            EmailDO targetCriteria = new EmailDO();
            targetCriteria.Subject = "We need a little more information from you"; //this is the current subject for clarification requests.
            PollingEngine.InjectedEmailQuery injectedQuery = InjectedQuery_FindClarificationRequest;
            List<EmailDO> queryResults = _polling.PollForEmail(injectedQuery, targetCriteria, "external", client, inboundDaemon);
            return (ClarificationRequestDO)queryResults.First();

        }
        
    }
}
