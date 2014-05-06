using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Data.Constants;
using Data.Entities;
using Data.Entities.Enumerations;
using Data.Interfaces;
using Data.Repositories;

namespace KwasantCore.Services
{
    public static class BookingRequest
    {
        public static void ProcessBookingRequest(IUnitOfWork uow, BookingRequestDO bookingRequest)
        {
            CustomerDO curCustomer = GetOrCreateCustomer(uow, bookingRequest);
            
            bookingRequest.Customer = curCustomer;
            bookingRequest.Instructions = ProcessShortHand(uow, bookingRequest.Text);
            bookingRequest.Status = EmailStatus.UNPROCESSED;
            uow.SaveChanges();
        }

        private static CustomerDO GetOrCreateCustomer(IUnitOfWork uow, BookingRequestDO currMessage)
        {
            string fromEmailAddress = currMessage.From.Address;
            CustomerRepository customerRepo = new CustomerRepository(uow);
            CustomerDO curCustomer = customerRepo.GetQuery().FirstOrDefault(c => c.EmailAddress == fromEmailAddress);
            if (curCustomer == null)
            {
                curCustomer = new CustomerDO();
                curCustomer.EmailAddress = fromEmailAddress;
                curCustomer.FirstName = currMessage.From.Name;
                customerRepo.Add(curCustomer);
            }
            return curCustomer;
        }

        private static List<InstructionDO> ProcessShortHand(IUnitOfWork uow, string emailBody)
        {
            List<int?> instructionIDs = ProcessTravelTime(emailBody).Select(travelTime => (int?) travelTime).ToList();
            instructionIDs.Add(ProcessAllDay(emailBody));
            instructionIDs = instructionIDs.Where(i => i.HasValue).Distinct().ToList();
            InstructionRepository instructionRepo = new InstructionRepository(uow);
            return instructionRepo.GetQuery().Where(i => instructionIDs.Contains(i.InstructionID)).ToList();
        }

        private static IEnumerable<int> ProcessTravelTime(string emailBody)
        {
            const string regex = "{0}CC|CC{0}";

            Dictionary<int, int> travelTimeMapping = new Dictionary<int, int>
            {
                {30, InstructionConstants.TravelTime.Add30MinutesTravelTime},
                {60, InstructionConstants.TravelTime.Add60MinutesTravelTime},
                {90, InstructionConstants.TravelTime.Add90MinutesTravelTime},
                {120, InstructionConstants.TravelTime.Add120MinutesTravelTime}
            };

            List<int> instructions = new List<int>();

            //Matches cc[number] or [number]cc. Not case sensitive
            foreach (int allowedDuration in travelTimeMapping.Keys)
            {
                Regex reg = new Regex(String.Format(regex, allowedDuration), RegexOptions.IgnoreCase);
                if (reg.IsMatch(emailBody))
                {
                    instructions.Add(travelTimeMapping[allowedDuration]);
                }
            }
            return instructions;
        }


        private static int? ProcessAllDay(string emailBody)
        {
            const string regex = "(ccADE)";

            Regex reg = new Regex(regex, RegexOptions.IgnoreCase);
            if (reg.IsMatch(emailBody))
            {
                return InstructionConstants.EventDuration.MarkAsAllDayEvent;
            }
            return null;
        }
    }
}
