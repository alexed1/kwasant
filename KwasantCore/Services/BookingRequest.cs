﻿using System;
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
        public int recordcount;
        public void Process(IUnitOfWork uow, BookingRequestDO bookingRequest)
        {
            bookingRequest.BookingRequestStateID = BookingRequestState.Unprocessed;
            UserDO curUser = uow.UserRepository.GetOrCreateUser(bookingRequest);
            
            bookingRequest.User = curUser;
            bookingRequest.Instructions = ProcessShortHand(uow, bookingRequest.HTMLText);

            foreach (var calendar in bookingRequest.User.Calendars)
                bookingRequest.Calendars.Add(calendar);
        }

        public List<object> GetAllByUserId(IBookingRequestRepository curBookingRequestRepository, int start, int length, string userid)
        {
            return curBookingRequestRepository.GetAll().Where(e => e.User.Id == userid).Skip(start).Take(length).Select(e =>
                            (object)new
                            {
                                id = e.Id,
                                subject = e.Subject,
                                dateReceived = e.DateReceived.ToString("M-d-yy hh:mm tt"),
                                linkedcalendarids = String.Join(",", e.User.Calendars.Select(c => c.Id))
                            }).ToList();
        }

        public int GetBookingRequestsCount(IBookingRequestRepository curBookingRequestRepository, string userid)
        {
            return curBookingRequestRepository.GetAll().Where(e => e.User.Id == userid).Count();
        }

        public string GetUserId(IBookingRequestRepository curBookingRequestRepository, int bookingRequestId)
        {
            return (from requests in curBookingRequestRepository.GetAll()
                    where requests.Id == bookingRequestId
                    select requests.User.Id).FirstOrDefault();
        }

        public object GetUnprocessed(IUnitOfWork uow)
        {
            return
                uow.BookingRequestRepository.GetAll()
                    .Where(e => e.BookingRequestStateID == BookingRequestState.Unprocessed)
                    .OrderByDescending(e => e.DateReceived)
                    .Select(
                        e =>
                            new
                            {
                                id = e.Id,
                                subject = e.Subject,
                                fromAddress = e.From.Address,
                                dateReceived = e.DateReceived.ToString("M-d-yy hh:mm tt"),
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


        protected List<BR_RelatedItems> GetRelatedEvents(IUnitOfWork uow, int bookingRequestId)
        {
          return uow.EventRepository.GetAll().Where(e => e.BookingRequestID == bookingRequestId).Select(e => new BR_RelatedItems
                {
                    id = e.Id,
                    Type = "Event",
                    Date = e.StartDate.ToString("M-d-yy hh:mm tt")
                }).ToList();

        }

        protected List<BR_RelatedItems> GetRelatedClarificationRequests(IUnitOfWork uow, int bookingRequestId)
        {
            return uow.ClarificationRequestRepository.GetAll().Where(e => e.BookingRequestId == bookingRequestId).Select(e => new BR_RelatedItems
            {
                id = e.Id,
                Type = "Clarification",
                Date = e.DateCreated.ToString("M-d-yy hh:mm tt")
            }).ToList();
           
        }


        public List<BR_RelatedItems> BuildRelatedEventsJSON(IUnitOfWork uow, int bookingRequestId, int start, int length)
        {
            List<BR_RelatedItems> bR_RelatedItems = new List<BR_RelatedItems>();
            var events = GetRelatedEvents(uow, bookingRequestId);
            var clarificationRequests = GetRelatedClarificationRequests(uow, bookingRequestId);

            if(events.Count()>0)
                bR_RelatedItems.AddRange(events);

            if (clarificationRequests.Count()>0)
                bR_RelatedItems.AddRange(clarificationRequests);

            recordcount = bR_RelatedItems.Count();
            return bR_RelatedItems.OrderByDescending(x => x.Date).Skip(start).Take(length).ToList();
        }


    }

    public struct BR_RelatedItems
    {
        public int id { get; set; }
        public string Date { get; set; }
        public string Type { get; set; }
    }

   
}
