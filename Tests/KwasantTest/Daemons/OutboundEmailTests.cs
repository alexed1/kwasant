using Daemons;
using Data.Entities;
using Data.Entities.Enumerations;
using Data.Interfaces;
using Data.Repositories;
using KwasantCore.Managers.APIManager.Packagers.Mandrill;
using KwasantCore.StructureMap;
using Moq;
using NUnit.Framework;
using StructureMap;

namespace KwasantTest.Daemons
{
    [TestFixture]
    public class OutboundEmailTests
    {
        [SetUp]
        public void Setup()
        {
            StructureMapBootStrapper.ConfigureDependencies("test");
        }

        [Test]
        public void TestInboundEmail()
        {
            var uow = ObjectFactory.GetInstance<IUnitOfWork>();
            var emailRepo = new EmailRepository(uow);
            var outboundEmail = new EmailDO {Status = EmailStatus.QUEUED};
            emailRepo.Add(outboundEmail);

            var oe = new OutboundEmail();

            var mandril = new Mock<MandrillPackager>();
            ObjectFactory.Container.Inject(mandril.Object);

            DaemonTests.RunDaemonOnce(oe);
            mandril.Verify(p => p.PostMessageSend(It.IsAny<EmailDO>()));
        }
    }
}
