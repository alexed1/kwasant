using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Data.Constants;
using Data.Entities;
using Data.Entities.Enumerations;
using Data.Interfaces;
using KwasantCore.Services;
using KwasantCore.StructureMap;
using KwasantTest.Fixtures;
using NUnit.Framework;
using StructureMap;
using Utilities;

namespace KwasantTest.Services
{
    [TestFixture]
    public class ClarificationRequestTests
    {
        private FixtureData _fixture;
        private IUnitOfWork _uow;
        private ClarificationRequest _cr;

        [SetUp]
        public void SetUp()
        {
            StructureMapBootStrapper.ConfigureDependencies(StructureMapBootStrapper.DependencyType.TEST);
            _uow = ObjectFactory.GetInstance<IUnitOfWork>();
            _fixture = new FixtureData();
            _cr = new ClarificationRequest();
        }

        [Test]
        [Category("ClarificationRequest")]
        public void CanCreateFromBookingRequest()
        {
            // SETUP
            var curBookingRequestDO = _fixture.TestBookingRequest1();

            // EXECUTE
            var curClarificationRequestDO = _cr.Create(_uow, curBookingRequestDO);

            // VERIFY
            Assert.AreEqual(curBookingRequestDO.Id, curClarificationRequestDO.BookingRequestId, "CR must have BR assigned to passed one.");
            Assert.AreEqual(curBookingRequestDO.User.EmailAddress, curClarificationRequestDO.To.SingleOrDefault(), "CR must have TO recipient assigned to BR#User#EmailAddress");
        }

        [Test]
        [Category("ClarificationRequest")]
        public void CanSend()
        {
            // SETUP
            var curBookingRequestDO = _fixture.TestBookingRequest1();
            const string responseUrl = "kwasant.com/crr?test";

            // EXECUTE
            var curClarificationRequestDO = _cr.Create(_uow, curBookingRequestDO);
            _cr.Send(_uow, curClarificationRequestDO, responseUrl);
            var curCrEnvelopeDO =
                _uow.EnvelopeRepository.GetQuery().FirstOrDefault(e => e.Email == curClarificationRequestDO);

            // VERIFY
            Assert.NotNull(curCrEnvelopeDO, "Envelope was not created.");
            Assert.AreEqual(curCrEnvelopeDO.TemplateName, "clarification_request_v1", "Invalid template name.");
            Assert.AreEqual(curCrEnvelopeDO.MergeData["RESP_URL"], responseUrl, "Invalid response URL.");
        }

        [Test]
        [Category("ClarificationRequest")]
        public void CanGenerateResponseUrl()
        {
            // SETUP
            var curBookingRequestDO = _fixture.TestBookingRequest1();
            const string responseUrlPath = "kwasant.com/crr";
            const string responseUrlFormat = responseUrlPath + "?{0}";

            // EXECUTE
            var curClarificationRequestDO = _cr.Create(_uow, curBookingRequestDO);
            var responseUrl = _cr.GenerateResponseURL(curClarificationRequestDO, responseUrlFormat);
            var responseUrlParts = responseUrl.Split('?');
            var encryptedParams = WebUtility.UrlDecode(responseUrlParts[1]);
            var decryptedParams = Encryption.DecryptParams(encryptedParams);

            // VERIFY
            Assert.AreEqual(responseUrlParts[0], responseUrlPath);
            Assert.AreEqual(decryptedParams["id"], curClarificationRequestDO.Id);
        }

        [Test]
        [Category("ClarificationRequest")]
        public void CanProcessResponse()
        {
            // SETUP
            var curBookingRequestDO = _fixture.TestBookingRequest1();
            const string responseUrl = "kwasant.com/crr?test";

            // EXECUTE
            var curClarificationRequestDO = _cr.Create(_uow, curBookingRequestDO);
            curClarificationRequestDO.Questions
                .Add(new QuestionDO()
                         {
                             Id = 1,
                             ClarificationRequest = curClarificationRequestDO,
                             ClarificationRequestId = curClarificationRequestDO.Id,
                             Status = QuestionStatus.Unanswered,
                             Text = "question"
                         });
            _cr.Send(_uow, curClarificationRequestDO, responseUrl);

            var submittedClarificationRequestDO = curClarificationRequestDO;
            submittedClarificationRequestDO.Questions[0].Status = QuestionStatus.Answered;
            submittedClarificationRequestDO.Questions[0].Response = "response";
            _cr.ProcessResponse(submittedClarificationRequestDO);
            
            var respondedCr = _uow.ClarificationRequestRepository.GetByKey(curClarificationRequestDO.Id);

            // VERIFY
            Assert.AreEqual(respondedCr.Questions[0].Status, QuestionStatus.Answered);
            Assert.AreEqual(respondedCr.Questions[0].Response, "response");
            Assert.AreEqual(respondedCr.ClarificationStatus, ClarificationStatus.Resolved);
            Assert.AreEqual(respondedCr.BookingRequest.BookingRequestStatusID, BookingRequestStatus.Pending);
        }
    }
}
