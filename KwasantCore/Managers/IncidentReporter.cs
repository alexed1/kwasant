using System;
using KwasantCore.Services;
using StructureMap;
using Data.Interfaces;
using Data.Entities;
namespace Data.Infrastructure
{
    public class IncidentReporter
    {
        public void SubscribeToAlerts()
        {
            AlertManager.AlertEmailProcessingFailure += ProcessAlert_EmailProcessingFailure;
            AlertManager.AlertBookingRequestProcessingTimeout += ProcessTimeout;
            AlertManager.AlertError_EmailSendFailure += ProcessEmailSendFailure;
            AlertManager.AlertErrorSyncingCalendar += ProcessErrorSyncingCalendar;
        }

        public void ProcessAlert_EmailProcessingFailure(string dateReceived, string errorMessage)
        {
            using (var _uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                IncidentDO incidentDO = new IncidentDO();
                incidentDO.PrimaryCategory = "EmailFailure";
                incidentDO.SecondaryCategory = "Email";
                incidentDO.CreateTime = Convert.ToDateTime(dateReceived);
                incidentDO.Priority = 5;
                incidentDO.Activity = "IntakeFailure";
                incidentDO.Notes = errorMessage;
                _uow.IncidentRepository.Add(incidentDO);
                _uow.SaveChanges();
            }
        }

        public void ProcessTimeout(int bookingRequestId, string bookerId )
        {
            
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                BookingRequestDO bookingRequestDO = uow.BookingRequestRepository.GetByKey(bookingRequestId);
                IncidentDO incidentDO = new IncidentDO();
                incidentDO.PrimaryCategory = "Timeout";
                incidentDO.SecondaryCategory = "BookingRequest";
                incidentDO.CreateTime = DateTime.Now;
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
                incidentDO.CreateTime = DateTime.Now; ;
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

        private void ProcessErrorSyncingCalendar(IBaseDO calendarLink)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                IncidentDO incidentDO = new IncidentDO();
                incidentDO.PrimaryCategory = "SyncFailure";
                incidentDO.SecondaryCategory = "Calendar";
                incidentDO.CreateTime = DateTime.Now;
                incidentDO.Activity = "SyncFailure";
                incidentDO.ObjectId = calendarLink.Id;
                incidentDO.CustomerId = calendarLink.LocalCalendar.OwnerID;
                incidentDO.Notes = calendarLink.LastSynchronizationResult;
                uow.IncidentRepository.Add(incidentDO);
                uow.SaveChanges();
            }

            Email email = ObjectFactory.GetInstance<Email>();
            email.SendAlertEmail("CalendarSync failure",
                                 string.Format(
                                     "CalendarSync failure for calendar link #{0} ({1}):\r\n" +
                                     "Customer id: {2},\r\n" +
                                     "Local calendar id: {3}\r\n," +
                                     "Remote calendar url: {4}",
                                     calendarLink.Id,
                                     calendarLink.LastSynchronizationResult,
                                     calendarLink.LocalCalendar.OwnerID,
                                     calendarLink.LocalCalendarID,
                                     calendarLink.RemoteCalendarHref));
        }

    }
}
