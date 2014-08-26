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
        
        private PollingEngine _polling;


        [SetUp]
        public void SetUp()
        {
            StructureMapBootStrapper.ConfigureDependencies(StructureMapBootStrapper.DependencyType.TEST);
            _uow = ObjectFactory.GetInstance<IUnitOfWork>();

            _fixture = new FixtureData();
            _polling = new PollingEngine(_uow);
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
