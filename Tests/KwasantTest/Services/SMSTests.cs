using KwasantCore.ExternalServices;
using KwasantCore.Managers.APIManager.Packagers;
using KwasantCore.Managers.APIManagers.Packagers;
using KwasantCore.Managers.APIManagers.Packagers.Twilio;
using KwasantCore.StructureMap;
using Moq;
using NUnit.Framework;
using StructureMap;
using Twilio;
using Utilities;

namespace KwasantTest.Services
{
    [TestFixture]
    public class SMSTests
    {
        [SetUp]
        public void Setup()
        {
            StructureMapBootStrapper.ConfigureDependencies(StructureMapBootStrapper.DependencyType.TEST);
        }

        [Test]
        public void TestCanSendSMS()
        {
            const string testBody = "Test SMS";

            var oj = new Mock<ITwilioRestClient>();
            oj.Setup(a => a.SendSmsMessage(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns<string, string, string>((from, to, body) => { return new SMSMessage { Body = body }; });

            ObjectFactory.Configure(a => a.For<ITwilioRestClient>().Use(oj.Object));

            var twil = ObjectFactory.GetInstance<ISMSPackager>();
            var res = twil.SendSMS(ObjectFactory.GetInstance<IConfigRepository>().Get<string>("TwilioToNumber"), testBody);

            Assert.AreEqual(testBody, res.Body);
            Assert.Null(res.RestException);
        }
    }
}