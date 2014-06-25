using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Data.Constants;
using Data.Entities;
using Data.Interfaces;
using Data.Repositories;

namespace KwasantCore.Services
{
    public class BookingRequest
    {
        public void ProcessBookingRequest(IUnitOfWork uow, BookingRequestDO bookingRequest)
        {
            bookingRequest.BookingRequestStatusID = BookingRequestStatus.Unprocessed;
            UserDO curUser = uow.UserRepository.GetOrCreateUser(bookingRequest);
            
            bookingRequest.User = curUser;
            bookingRequest.Instructions = ProcessShortHand(uow, bookingRequest.HTMLText);
        }

        public List<BookingRequestDO> GetBookingRequests(IBookingRequestRepository curBookingRequestRepository, int id)
        {
            return curBookingRequestRepository.GetAll().Where(e => e.User.Id == (from requests in curBookingRequestRepository.GetAll()
                                                                                 where requests.Id == id
                                                                                 select requests.User.Id).FirstOrDefault()).Where(e => e.BookingRequestStatusID == BookingRequestStatus.Unprocessed).ToList();
        }

        public object GetUnprocessed(IBookingRequestRepository curBookingRequestRepository)
        {
            return
                curBookingRequestRepository.GetAll()
                    .Where(e => e.BookingRequestStatusID == BookingRequestStatus.Unprocessed)
                    .OrderByDescending(e => e.DateReceived)
                    .Select(
                        e =>
                            new
                            {
                                id = e.Id,
                                subject = e.Subject,
                                fromAddress = e.From.Address,
                                dateReceived = e.DateReceived.ToString("yy-MM-dd"),
                                body =
                                    e.HTMLText.Trim().Length > 400
                                        ? e.HTMLText.Trim().Substring(0, 400)
                                        : e.HTMLText.Trim()
                            })
                    .ToList();
        }
        
        private List<InstructionDO> ProcessShortHand(IUnitOfWork uow, string emailBody)
        {
            List<int?> instructionIDs = ProcessTravelTime(emailBody).Select(travelTime => (int?) travelTime).ToList();
            instructionIDs.Add(ProcessAllDay(emailBody));
            instructionIDs = instructionIDs.Where(i => i.HasValue).Distinct().ToList();
            InstructionRepository instructionRepo = uow.InstructionRepository;
            return instructionRepo.GetQuery().Where(i => instructionIDs.Contains(i.Id)).ToList();
        }

        private IEnumerable<int> ProcessTravelTime(string emailBody)
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
                if (!String.IsNullOrEmpty(emailBody))
                {
                    if (reg.IsMatch(emailBody))
                    {
                        instructions.Add(travelTimeMapping[allowedDuration]);
                    }
                }
            }
            return instructions;
        }


        private int? ProcessAllDay(string emailBody)
        {
            const string regex = "(ccADE)";

            Regex reg = new Regex(regex, RegexOptions.IgnoreCase);
            if (!String.IsNullOrEmpty(emailBody))
            {
                if (reg.IsMatch(emailBody))
                {
                    return InstructionConstants.EventDuration.MarkAsAllDayEvent;
                }
            }
            return null;
        }
    }
}
