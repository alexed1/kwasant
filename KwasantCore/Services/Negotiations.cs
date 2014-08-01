using System.Linq;
using Data.Constants;
using Data.Interfaces;


namespace KwasantCore.Services
{
    public class Negotiations
    {
        public string getNegotiation(IUnitOfWork uow, int bookingRequestId)
        {
            string negotiationLbl = "";

            var negotiationdo = uow.NegotiationsRepository.GetAll().Where(br => br.RequestId == bookingRequestId && br.NegotiationStateID != NegotiationState.Resolved);
            if (negotiationdo.Count() > 0)
                negotiationLbl = "Edit Negotiation";
            else
                negotiationLbl = "Create Negotiation";

            return negotiationLbl;
        }

        

    }
}
