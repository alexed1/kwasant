using System;
using System.Collections.Generic;
using System.Linq;
using Data.Entities;
using Data.Interfaces;
using Data.Repositories;
using Data.States;
using KwasantCore.Exceptions;
using KwasantCore.Managers;
using KwasantICS.DDay.iCal;
using KwasantICS.DDay.iCal.DataTypes;
using StructureMap;
using Utilities;
//using Data.Constants;


namespace KwasantCore.Services
{
    public class Negotiation
    {
        private ClarificationRequest _cr;

        public Negotiation()
        {
            _cr = new ClarificationRequest();
        }
        public string getNegotiationTask(IUnitOfWork uow, int bookingRequestId)
        {
            string negotiationLbl = "";

            //var negotiationdo = uow.NegotiationsRepository.GetAll().Where(br => br.RequestId == bookingRequestId && br.State != NegotiationState.Resolved);
            var negotiationdo = uow.NegotiationsRepository.GetAll().Where(br => br.BookingRequestID == bookingRequestId && br.NegotiationState != NegotiationState.Resolved);
            if (negotiationdo.Count() > 0)
                negotiationLbl = "Edit Negotiation";
            else
                negotiationLbl = "Create Negotiation";

            return negotiationLbl;
        }

        //generate CR's and dispatch them.
        public void Process(NegotiationDO curNegotiation)
        {
            List<ClarificationRequestDO> curCRList = GenerateClarificationRequests(curNegotiation);
            foreach (var cr in curCRList)
            {
                  
                  _cr.Send(cr);
            }
        }

        //This will generate CR emails  with a link that takes the recipient to a response view. 
        public List<ClarificationRequestDO> GenerateClarificationRequests(NegotiationDO curNegotiation)
        {
            List<ClarificationRequestDO> curCRList = new List<ClarificationRequestDO>();
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {

                //NegotiationId must resolve to a Negotiation with status "Unresolved".
                INegotiationsRepository negotiationsRepository = uow.NegotiationsRepository;
              //  NegotiationDO curNegotiation = negotiationsRepository.GetByKey(curNegotiation.Id);
                if (curNegotiation == null)
                    throw new ApplicationException("DispatchNegotiationEmail was passed an invalid NegotiationId");
                //this line should work as written once kw-287 is done
                  if (curNegotiation.NegotiationState != NegotiationState.InProcess)
                    throw new ApplicationException("Cannot currently dispatch negotiation email for a negotiation that does not have a state of 'InProcess' ");

                ClarificationRequest _cr = new ClarificationRequest();

                //Build a list of Users based on the Attendees associated with the Negotiation.
                //This should work once the fixes to NegotiationDO are done in kw-324
                List<UserDO> Recipients = new List<UserDO>(); //this is a placeholder. see the line below this.
                    //curNegotiation.Attendees;//FINISH: for each AttendeeDO associated with this Negotiation, find their corresponding UserDO

               //For each user, if there is not already a ClarificationRequestDO that has this UserId and this NeogtiationId... 
                foreach (var user in Recipients)
                {
                    ClarificationRequestDO existingCR =
                        uow.ClarificationRequestRepository.FindOne(
                            c => c.To.First().Address == user.EmailAddress.Address);
                    if (existingCR != null && existingCR.NegotiationId != curNegotiation.Id)
                    {
                        
                        //call ClarificationRequest#Create, get back a ClarificationRequestDO,
                        ClarificationRequestDO curCR = _cr.Create(uow,curNegotiation.BookingRequest, curNegotiation);
                        uow.ClarificationRequestRepository.Add(curCR);
                        curCRList.Add(curCR);
                     
                    }
                    else throw new ApplicationException("Tried to dispatch a Clarification Request when one was already outstanding. We will need to enable this but for the moment, it's forbidden");
                }
                uow.SaveChanges();
                return curCRList;

            }
        }

        
              
    }
}
