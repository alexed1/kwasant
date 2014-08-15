using Data.Entities;
using Data.Interfaces;
using KwasantCore.Services;
using KwasantTest.Fixtures;
using NUnit.Framework;
using KwasantTest.Utilities;
using S22.Imap;
using System.Collections.Generic;
using System.Linq;
using KwasantCore.StructureMap;
using StructureMap;

namespace KwasantTest.Integration.NegotiationITests
{
    [TestFixture]
    public class NegotiationTests
    {
        private IUnitOfWork _uow;
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

            UserDO testUser3 = _fixture.TestUser3();
           


            string targetAddress = testUser3.EmailAddress.Address; // MUST BE integration@kwawant
            string targetPassword = "thorium65";
            ImapClient client = new ImapClient("imap.gmail.com", 993, targetAddress, targetPassword, AuthMethod.Login, true);

            AttendeeDO testAttendee3 = _fixture.TestAttendee3();
            NegotiationDO testNegotiation = _fixture.TestNegotiation1();
            testNegotiation.BookingRequest.User = testUser3;
            testNegotiation.Attendees.Add(testAttendee3);
            _uow.NegotiationsRepository.Add(testNegotiation);
            _uow.SaveChanges();


            //EXECUTE

            //Start Timer
            using (_polling.NewTimer(_polling.totalOperationTimeout, "Workflow"))
            {
                _negotiation.Process(testNegotiation);
                _uow.SaveChanges();

                //make sure that the emails go out. this may not be necessary
                _polling.FlushOutboundEmailQueues();
            }

            //VERIFY

            //Verify that attendee has received an appropriate ClarificationRequest.
            //If attendee has received the CR, the first EmailDO should have certain characteristics
            EmailDO foundCR = PollForClarificationRequest(testAttendee3, client);
            Assert.NotNull(foundCR, "No ClarificationRequest retrieved.");
            //check timeouts
            _polling.CheckTimeouts();

        }



        //Injected Query
        //examines the unread messages in an email account for one that appears to be a clarification request
        public static IEnumerable<EmailDO> InjectedQuery_FindClarificationRequest(EmailDO targetCriteria, List<EmailDO> unreadMessages)
        {

            List<EmailDO> matchingEmails =  unreadMessages.FindAll(um => um.Subject == targetCriteria.Subject);
            return matchingEmails;
        }
        
        //Inject a query into the email polling engine that looks for a clarification request with characteristics matching the specified Attendee
        public EmailDO PollForClarificationRequest(AttendeeDO testAttendee, ImapClient client)
        {
            EmailDO targetCriteria = new EmailDO();
            targetCriteria.Subject = "We need a little more information from you"; //this is the current subject for clarification requests.
            PollingEngine.InjectedEmailQuery injectedQuery = InjectedQuery_FindClarificationRequest;
            List<EmailDO> queryResults = _polling.PollForEmail(injectedQuery, targetCriteria, "external", client);
            return queryResults.FirstOrDefault();

        }
        
    }
}
