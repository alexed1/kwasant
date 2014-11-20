using Data.Entities;
using Data.States;
using StructureMap;
using Data.Interfaces;namespace KwasantTest.Fixtures
{
    public partial class FixtureData
    {
        public NegotiationDO TestNegotiation1()
        {
            var curBookingRequestDO = TestBookingRequest1();
            var curNegotiationDO = new NegotiationDO
            {
                Id = 1,
                BookingRequest = curBookingRequestDO,
                BookingRequestID = curBookingRequestDO.Id,
                NegotiationState = NegotiationState.InProcess,
                Name = "Negotiation 1"
            };
            var question = TestQuestion1();
            question.Negotiation = curNegotiationDO;
            curNegotiationDO.Questions.Add(question);
            return curNegotiationDO;
        }

        public NegotiationDO TestNegotiation2()
        {
            var curBookingRequestDO = TestBookingRequest2();
            var curNegotiationDO = new NegotiationDO
            {
                Id = 1,
                BookingRequest = curBookingRequestDO,
                BookingRequestID = curBookingRequestDO.Id,
                NegotiationState = NegotiationState.InProcess,
                Name = "Negotiation 2"
            };
            var question = TestQuestion1();
            question.Negotiation = curNegotiationDO;
            curNegotiationDO.Questions.Add(question);
            return curNegotiationDO;

        }

        

       
    }
}
