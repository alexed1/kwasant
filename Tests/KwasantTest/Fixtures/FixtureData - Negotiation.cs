using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Data.Constants;
using Data.Entities;
using Data.Entities.Enumerations;

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
                State = NegotiationState.InProcess,
                Name = "Negotiation 1"
            };
            return curNegotiationDO;
        }
    }
}
