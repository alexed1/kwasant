using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Data.Entities;
using Data.Infrastructure;
using Data.Interfaces;
using Data.Repositories;
using Data.States;
using StructureMap;

namespace KwasantCore.Services
{
    public class Booker
    {
        //VerifyOwnership
        public string IsBookerValid(IUnitOfWork uow, int bookingRequestId, string currBooker)
        {

            BookingRequestDO bookingRequestDO = uow.BookingRequestRepository.GetByKey(bookingRequestId);
            if (bookingRequestDO.UserID != currBooker)
            {
                if (bookingRequestDO.UserID != null)
                    return uow.UserRepository.GetByKey(bookingRequestDO.UserID).FirstName;
                else
                    return "valid";
            }
            return "valid";

        }

        public string ChangeOwner(IUnitOfWork uow, int bookingRequestId, string currBooker)
        {
            string result = "";
            try
            {
                BookingRequestDO bookingRequestDO;
                if (bookingRequestId != null)
                {
                    bookingRequestDO = uow.BookingRequestRepository.GetByKey(bookingRequestId);
                    bookingRequestDO.UserID = currBooker;
                    uow.SaveChanges();
                    AlertManager.BookingRequestOwnershipChange(bookingRequestDO.Id, currBooker);
                    result = "Booking request ownership changed successfully!";
                }
            }
            catch (Exception ex)
            {
                result = ex.Message;
            }
            return result;
        }
    }
}
