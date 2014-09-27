using Data.Entities;
using Data.Interfaces;
using KwasantTest.Fixtures;
using NUnit.Framework;
using System.Collections.Generic;
using KwasantCore.StructureMap;
using StructureMap;

namespace KwasantTest.Integration.NegotiationITests
{
    [TestFixture]
    public class NegotiationTests
    {
        private IUnitOfWork _uow;
        private FixtureData _fixture;
        

        [SetUp]
        public void SetUp()
        {
            StructureMapBootStrapper.ConfigureDependencies(StructureMapBootStrapper.DependencyType.TEST);
            _uow = ObjectFactory.GetInstance<IUnitOfWork>();

            _fixture = new FixtureData();
        }

        //Injected Query
        //examines the unread messages in an email account for one that appears to be a clarification request
        public static IEnumerable<EmailDO> InjectedQuery_FindClarificationRequest(EmailDO targetCriteria, List<EmailDO> unreadMessages)
        {

            List<EmailDO> matchingEmails =  unreadMessages.FindAll(um => um.Subject == targetCriteria.Subject);
            return matchingEmails;
        }
        
        
    }
}
