using KwasantCore.Managers.APIManager.Packagers.Twilio;
using KwasantCore.StructureMap;
using NUnit.Framework;
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
        public void TestTwillioConfiguration()
        {
            //We just want to test that we can instantiate the packager
            var twil = new TwilioPackager();
        }

        [Test]
        public void TestCanSendSMS()
        {
            const string testBody = "Test SMS";

            var twil = new TwilioPackager();
            var res = twil.SendSMS(ConfigRepository.Get<string>("TwilioToNumber"), testBody);

            Assert.AreEqual(testBody, res.Body);
            Assert.Null(res.RestException);
        }
    }
}