using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text.RegularExpressions;
using Data.Constants;
using Data.DataAccessLayer.Interfaces;
using Data.DataAccessLayer.Repositories;
using Data.Models;

namespace Data.Tools.Managers
{
    public static class BookingRequestManager
    {
        public static void ConvertEmail(IUnitOfWork uow, MailMessage currMessage)
        {
            var existingCustomer = GetOrCreateCustomer(uow, currMessage);

            var bookingRequestRepo = new BookingRequestRepository(uow);
            var bookingRequest = EmailHelper.ConvertMailMessageToEmail(bookingRequestRepo, currMessage);
            bookingRequest.Customer = existingCustomer;
            bookingRequest.Instructions = ProcessShortHand(uow, currMessage.Body);
            uow.SaveChanges();
        }

        private static CustomerDO GetOrCreateCustomer(IUnitOfWork uow, MailMessage currMessage)
        {
            var fromEmailAddress = currMessage.From.Address;
            var customerRepo = new CustomerRepository(uow);
            var existingCustomer = customerRepo.GetQuery().FirstOrDefault(c => c.EmailAddress == fromEmailAddress);
            if (existingCustomer == null)
            {
                existingCustomer = new CustomerDO();
                existingCustomer.EmailAddress = fromEmailAddress;
                existingCustomer.FirstName = currMessage.From.DisplayName;
                customerRepo.Add(existingCustomer);
            }
            return existingCustomer;
        }

        private static List<InstructionDO> ProcessShortHand(IUnitOfWork uow, string emailBody)
        {
            var instructionIDs = ProcessTravelTime(emailBody).Select(travelTime => (int?) travelTime).ToList();
            instructionIDs.Add(ProcessAllDay(emailBody));
            instructionIDs = instructionIDs.Where(i => i.HasValue).Distinct().ToList();
            var instructionRepo = new InstructionRepository(uow);
            return instructionRepo.GetQuery().Where(i => instructionIDs.Contains(i.InstructionID)).ToList();
        }

        private static IEnumerable<int> ProcessTravelTime(string emailBody)
        {
            const string regex = "{0}CC|CC{0}";

            var travelTimeMapping = new Dictionary<int, int>
            {
                {30, InstructionConstants.TravelTime.Add30MinutesTravelTime},
                {60, InstructionConstants.TravelTime.Add60MinutesTravelTime},
                {90, InstructionConstants.TravelTime.Add90MinutesTravelTime},
                {120, InstructionConstants.TravelTime.Add120MinutesTravelTime}
            };

            var instructions = new List<int>();

            //Matches cc[number] or [number]cc. Not case sensitive
            foreach (var allowedDuration in travelTimeMapping.Keys)
            {
                var reg = new Regex(String.Format(regex, allowedDuration), RegexOptions.IgnoreCase);
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

            var reg = new Regex(regex, RegexOptions.IgnoreCase);
            if (reg.IsMatch(emailBody))
            {
                return InstructionConstants.EventDuration.MarkAsAllDayEvent;
            }
            return null;
        }
    }
}
