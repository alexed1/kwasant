using KwasantCore.Managers.APIManager.Packagers;
using KwasantCore.Managers.APIManager.Packagers.Twilio;
using KwasantCore.StructureMap;
using NUnit.Framework;
using StructureMap;
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

            ObjectFactory.Configure(a => a.For<ISMSPackager>().Use(new TwilioPackager()));
        }

        [Test]
        public void TestTwillioConfiguration()
        {
            //We just want to test that we can instantiate the packager
        }

        [Test]
        public void TestCanSendSMS()
        {
            const string testBody = "Test SMS";

            var twil = ObjectFactory.GetInstance<ISMSPackager>();
            var res = twil.SendSMS(ObjectFactory.GetInstance<IConfigRepository>().Get<string>("TwilioToNumber"), testBody);

            Assert.AreEqual(testBody, res.Body);
            Assert.Null(res.RestException);
        }
    }
}