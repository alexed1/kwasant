using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Data.Constants;
using Data.Entities;
using Data.Interfaces;
using KwasantCore.Services;
using KwasantCore.StructureMap;
using KwasantTest.Fixtures;
using NUnit.Framework;
using StructureMap;
using Utilities;
using BookingRequestState = Data.Constants.BookingRequestState;
using ClarificationRequestState = Data.Constants.ClarificationRequestState;

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

        //NOTE: THESE TESTS NEED TO BE REEVALUATED NOW THAT CR'S HAVE CHANGED SO MUCH.
      
        [Test,Ignore]
        [Category("ClarificationRequest")]
        public void CanSend()
        {
            // SETUP
            var curBookingRequestDO = _fixture.TestBookingRequest1();
            const string responseUrl = "kwasant.com/crr?test";
            var curNegotiationDO = _fixture.TestNegotiation1();

            // EXECUTE
            var curClarificationRequestDO = _cr.Create(_uow, curBookingRequestDO, curNegotiationDO);
            _cr.Send(curClarificationRequestDO);
            _uow.SaveChanges();

            // VERIFY
            var curCrEnvelopeDO =
                _uow.EnvelopeRepository.GetQuery().FirstOrDefault(e => e.Email == curClarificationRequestDO);
            Assert.NotNull(curCrEnvelopeDO, "Envelope was not created.");
            Assert.AreEqual(curCrEnvelopeDO.TemplateName, "clarification_request_v1", "Invalid template name.");
            Assert.AreEqual(curCrEnvelopeDO.MergeData["RESP_URL"], responseUrl, "Invalid response URL.");
        }

        [Test, Ignore]
        [Category("ClarificationRequest")]
        public void CanGenerateResponseUrl()
        {
            // SETUP
            var curBookingRequestDO = _fixture.TestBookingRequest1();
            const string responseUrlPath = "kwasant.com/crr";
            const string responseUrlFormat = responseUrlPath + "?{0}";
            var curNegotiationDO = _fixture.TestNegotiation1();

            // EXECUTE
            var curClarificationRequestDO = _cr.Create(_uow, curBookingRequestDO, curNegotiationDO);
            var responseUrl = _cr.GenerateResponseURL(curClarificationRequestDO, responseUrlFormat);
            var responseUrlParts = responseUrl.Split('?');
            var encryptedParams = WebUtility.UrlDecode(responseUrlParts[1]);
            var decryptedParams = Encryption.DecryptParams(encryptedParams);

            // VERIFY
            Assert.AreEqual(responseUrlParts[0], responseUrlPath);
            Assert.AreEqual(decryptedParams["id"], curClarificationRequestDO.Id);
        }

        [Test, Ignore]
        [Category("ClarificationRequest")]
        public void CanProcessResponse()
        {
            // SETUP
            var curBookingRequestDO = _fixture.TestBookingRequest1();
            const string responseUrl = "kwasant.com/crr?test";
            var curNegotiationDO = _fixture.TestNegotiation1();

            // EXECUTE
            var curClarificationRequestDO = _cr.Create(_uow, curBookingRequestDO, curNegotiationDO);
          
            _cr.Send(curClarificationRequestDO);
            _uow.SaveChanges();

            var submittedClarificationRequestDO = curClarificationRequestDO;
           
            _cr.ProcessResponse(submittedClarificationRequestDO);
            
            var respondedCr = _uow.ClarificationRequestRepository.GetByKey(curClarificationRequestDO.Id);

            // VERIFY
          
            Assert.AreEqual(respondedCr.ClarificationRequestStateID, ClarificationRequestState.Resolved);
            
        }
    }
}
