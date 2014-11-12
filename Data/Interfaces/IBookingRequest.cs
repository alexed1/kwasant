using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Data.Entities;
using Data.Repositories;

namespace Data.Interfaces
{
    public interface IBookingRequestDO : IEmail
    {
        [Required]
        UserDO User { get; set; }

      
    }

    public interface IBookingRequest 
    {
        void Process(IUnitOfWork uow, BookingRequestDO bookingRequest);

        List<object> GetAllByUserId(IBookingRequestDORepository curBookingRequestRepository, int start,
            int length, string userid);

        int GetBookingRequestsCount(IBookingRequestDORepository curBookingRequestRepository, string userid);
        string GetUserId(IBookingRequestDORepository curBookingRequestRepository, int bookingRequestId);
        object GetUnprocessed(IUnitOfWork uow);
        IEnumerable<object> GetRelatedItems(IUnitOfWork uow, int bookingRequestId);
        void Timeout(IUnitOfWork uow, BookingRequestDO bookingRequestDO);
        void ExtractEmailAddresses(IUnitOfWork uow, EventDO eventDO);
        object GetCheckOutBookingRequest(IUnitOfWork uow, string curBooker);
        string getCountDaysAgo(DateTimeOffset dateReceived);
        object GetAllBookingRequests(IUnitOfWork uow);
        UserDO GetPreferredBooker(BookingRequestDO bookingRequestDO);
        String GetConversationThread(BookingRequestDO bookingRequestDO);
    }

}