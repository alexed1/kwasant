using System;
using System.Linq;
using Data.Constants;
using Data.Entities;
using Data.Interfaces;
using KwasantCore.Exceptions;
using KwasantCore.Managers;
using KwasantICS.DDay.iCal;
using KwasantICS.DDay.iCal.DataTypes;
using Utilities;
//using Data.Constants;


namespace KwasantCore.Services
{
    public class Negotiations
    {
        public string getNegotiationTask(IUnitOfWork uow, int bookingRequestId)
        {
            string negotiationLbl = "";

            //var negotiationdo = uow.NegotiationsRepository.GetAll().Where(br => br.RequestId == bookingRequestId && br.State != NegotiationState.Resolved);
            var negotiationdo = uow.NegotiationsRepository.GetAll().Where(br => br.RequestId == bookingRequestId && br.NegotiationStateID != NegotiationState.Resolved);
            if (negotiationdo.Count() > 0)
                negotiationLbl = "Edit Negotiation";
            else
                negotiationLbl = "Create Negotiation";

            return negotiationLbl;
        }
              
    }
}
