using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Data.Constants;
using Data.Entities;
using Data.Interfaces;
using Data.Repositories;
using Data.Entities.Enumerations;

namespace KwasantCore.Services
{
    public class Negotiations
    {
        public string getNegotiation(IUnitOfWork uow, int bookingRequestId)
        {
            string negotiationLbl = "";

            var negotiationdo = uow.NegotiationsRepository.GetAll().Where(br => br.RequestId == bookingRequestId && br.State != NegotiationState.Resolved);
            if (negotiationdo.Count() > 0)
                negotiationLbl = "Edit Negotiation";
            else
                negotiationLbl = "Create Negotiation";

            return negotiationLbl;
        }

        

    }
}
