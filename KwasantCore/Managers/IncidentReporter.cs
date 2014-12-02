using System;
using System.Text;
using Data.Entities;
using Data.Infrastructure;
using Data.Interfaces;
using KwasantCore.Exceptions;
using KwasantCore.Services;
using StructureMap;

namespace KwasantCore.Managers
{
    public class IncidentReporter
    {
        public void SubscribeToAlerts()
        {
            AlertManager.AlertEmailProcessingFailure += ProcessAlert_EmailProcessingFailure;
            AlertManager.AlertBookingRequestProcessingTimeout += ProcessTimeout;
            AlertManager.AlertError_EmailSendFailure += ProcessEmailSendFailure;
            AlertManager.AlertErrorSyncingCalendar += ProcessErrorSyncingCalendar;
            AlertManager.AlertResponseReceived += AlertManagerOnAlertResponseReceived;
            AlertManager.AlertAttendeeUnresponsivenessThresholdReached += ProcessAttendeeUnresponsivenessThresholdReached;
            AlertManager.AlertBookingRequestCheckedOut += ProcessBRCheckedOut;
            AlertManager.AlertBookingRequestMarkedProcessed += ProcessBRMarkedProcessed; 
        }

        private void ProcessAttendeeUnresponsivenessThresholdReached(int expectedResponseId)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var expectedResponseDO = uow.ExpectedResponseRepository.GetByKey(expectedResponseId);
                if (expectedResponseDO == null)
                    throw new EntityNotFoundException<ExpectedResponseDO>(expectedResponseId);
                IncidentDO incidentDO = new IncidentDO();
                incidentDO.PrimaryCategory = "Negotiation";
                incidentDO.SecondaryCategory = "ClarificationRequest";
                incidentDO.CustomerId = expectedResponseDO.UserID;
                incidentDO.ObjectId = expectedResponseId;
                incidentDO.Activity = "UnresponsiveAttendee";
                uow.IncidentRepository.Add(incidentDO);
                uow.SaveChanges();
            }
        }

        private void AlertManagerOnAlertResponseReceived(int bookingRequestId, string userID, string customerID)
        {
            using (var _uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                IncidentDO incidentDO = new IncidentDO();
                incidentDO.PrimaryCategory = "Booking Request";
                incidentDO.SecondaryCategory = "Response Recieved";
                incidentDO.CustomerId = customerID;
                incidentDO.BookerId = userID;
                incidentDO.ObjectId = bookingRequestId;
                incidentDO.Activity = "Response Recieved";
                incidentDO.Notes = "No additional notes";
                _uow.IncidentRepository.Add(incidentDO);
                _uow.SaveChanges();
            }
        }

        public void ProcessAlert_EmailProcessingFailure(string dateReceived, string errorMessage)
        {
            using (var _uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                IncidentDO incidentDO = new IncidentDO();
                incidentDO.PrimaryCategory = "EmailFailure";
                incidentDO.SecondaryCategory = "Email";
                incidentDO.Priority = 5;
                incidentDO.Activity = "IntakeFailure";
                incidentDO.Notes = errorMessage;
                incidentDO.ObjectId = 0;
                _uow.IncidentRepository.Add(incidentDO);
                _uow.SaveChanges();
            }
        }

        public void ProcessTimeout(int bookingRequestId, string bookerId)
        {

            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                BookingRequestDO bookingRequestDO = uow.BookingRequestRepository.GetByKey(bookingRequestId);
                IncidentDO incidentDO = new IncidentDO();
                incidentDO.PrimaryCategory = "Timeout";
                incidentDO.SecondaryCategory = "BookingRequest";
                incidentDO.Activity = "";
                incidentDO.ObjectId = bookingRequestDO.Id;
                incidentDO.CustomerId = bookingRequestDO.CustomerID;
                incidentDO.BookerId = bookingRequestDO.BookerID;
                uow.IncidentRepository.Add(incidentDO);
                uow.SaveChanges();
            }
        }


        private void ProcessEmailSendFailure(int emailId, string message)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                IncidentDO incidentDO = new IncidentDO();
                incidentDO.PrimaryCategory = "EmailFailure";
                incidentDO.SecondaryCategory = "Email";
                incidentDO.Activity = "SendFailure";
                incidentDO.ObjectId = emailId;
                incidentDO.Notes = message;
                uow.IncidentRepository.Add(incidentDO);
                uow.SaveChanges();
            }
            Email _email = ObjectFactory.GetInstance<Email>();
            _email.SendAlertEmail("Alert! Kwasant Error Reported: EmailSendFailure",
                                  string.Format(
                                      "EmailID: {0}\r\n" +
                                      "Message: {1}",
                                      emailId, message));
        }
        private void ProcessErrorSyncingCalendar(IRemoteCalendarAuthDataDO authData, IRemoteCalendarLinkDO calendarLink = null)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                IncidentDO incidentDO = new IncidentDO();
                incidentDO.PrimaryCategory = "SyncFailure";
                incidentDO.SecondaryCategory = "Calendar";
                incidentDO.Activity = "SyncFailure";
                incidentDO.ObjectId = authData.Id;
                incidentDO.CustomerId = authData.UserID;
                if (calendarLink != null)
                {
                    incidentDO.Notes = string.Format("Link #{0}: {1}", calendarLink.Id, calendarLink.LastSynchronizationResult);
                }
                uow.IncidentRepository.Add(incidentDO);
                uow.SaveChanges();
            }

            var emailBodyBuilder = new StringBuilder();
            emailBodyBuilder.AppendFormat("CalendarSync failure for calendar auth data #{0} ({1}):\r\n", authData.Id,
                                          authData.Provider.Name);
            emailBodyBuilder.AppendFormat("Customer id: {0}\r\n", authData.UserID);
            if (calendarLink != null)
            {
                emailBodyBuilder.AppendFormat("Calendar link id: {0}\r\n", calendarLink.Id);
                emailBodyBuilder.AppendFormat("Local calendar id: {0}\r\n", calendarLink.LocalCalendarID);
                emailBodyBuilder.AppendFormat("Remote calendar url: {0}\r\n", calendarLink.RemoteCalendarHref);
                emailBodyBuilder.AppendFormat("{0}\r\n", calendarLink.LastSynchronizationResult);
            }

            Email email = ObjectFactory.GetInstance<Email>();
            email.SendAlertEmail("CalendarSync failure", emailBodyBuilder.ToString());
        }

        public void ProcessBRCheckedOut(int bookingRequestId, string bookerId)
        {
            BookingRequest _br = new BookingRequest();
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var bookingRequestDO = uow.BookingRequestRepository.GetByKey(bookingRequestId);
                if (bookingRequestDO == null)
                    throw new ArgumentException(string.Format("Cannot find a Booking Request by given id:{0}", bookingRequestId), "bookingRequestId");
                string status = bookingRequestDO.BookingRequestStateTemplate.Name;
                IncidentDO curAction = new IncidentDO()
                {
                    PrimaryCategory = "BookingRequest",
                    SecondaryCategory = "Throughput",
                    Activity = "Checkout",
                    CustomerId = bookingRequestDO.Customer.Id,
                    ObjectId = bookingRequestDO.Id,
                    BookerId = bookerId,
                    Notes = string.Format("ObjectId ID {0} Booker BookerId: {1} ", bookingRequestDO.Id, bookerId),
                };

                int getMinutinQueue = _br.GetTimeInQueue(uow, bookingRequestDO.Id.ToString());

                curAction.Data = string.Format("Time To Process: {0}", getMinutinQueue);

                uow.IncidentRepository.Add(curAction);
                uow.SaveChanges();
            }
        }


        public void ProcessBRMarkedProcessed(int bookingRequestId, string bookerId)
        {
            BookingRequest _br = new BookingRequest();
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var bookingRequestDO = uow.BookingRequestRepository.GetByKey(bookingRequestId);
                if (bookingRequestDO == null)
                    throw new ArgumentException(string.Format("Cannot find a Booking Request by given id:{0}", bookingRequestId), "bookingRequestId");
                string status = bookingRequestDO.BookingRequestStateTemplate.Name;
                IncidentDO curAction = new IncidentDO()
                {
                    PrimaryCategory = "BookingRequest",
                    SecondaryCategory = "Throughput",
                    Activity = "MarkedProcessed",
                    CustomerId = bookingRequestDO.Customer.Id,
                    ObjectId = bookingRequestDO.Id,
                    BookerId = bookerId,
                    Notes = string.Format("ObjectId ID {0} Booker BookerId: {1} ", bookingRequestDO.Id, bookerId),
                };

                int getMinutinQueue = _br.GetTimeInQueue(uow, bookingRequestDO.Id.ToString());

                curAction.Data = string.Format("Time To Process: {0}", getMinutinQueue);

                uow.IncidentRepository.Add(curAction);
                uow.SaveChanges();
            }
        }

    }


}
