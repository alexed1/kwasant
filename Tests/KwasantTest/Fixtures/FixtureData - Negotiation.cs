using Data.Constants;
using Data.Entities;

namespace KwasantTest.Fixtures
{
    public partial class FixtureData
    {
        public NegotiationDO TestNegotiation()
        {
            var curNegotiationDO = new NegotiationDO
            {
                Id = 1,
                RequestId = TestBookingRequest1().Id,
                NegotiationStateID = NegotiationState.InProcess,
                Name = "Negotiation 1"
            };
            return curNegotiationDO;
        }
    }
}
