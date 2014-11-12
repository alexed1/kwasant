using System;
using System.Text;
using Data.Entities;
using Data.Infrastructure;
using Data.Interfaces;
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
        }

        private void AlertManagerOnAlertResponseReceived(int bookingRequestId, string userID, string customerID)
        {
            using (var _uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                IncidentDO incidentDO = new IncidentDO();
                incidentDO.PrimaryCategory = "Booking Request";
                incidentDO.SecondaryCategory = "Response Recieved";
                incidentDO.CreateTime = DateTime.Now;
                incidentDO.CustomerId = customerID;
                incidentDO.BookerId = userID;
                incidentDO.ObjectId = bookingRequestId;
                incidentDO.Activity = "Response Recieved";
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
        private void ProcessErrorSyncingCalendar(IRemoteCalendarAuthDataDO authData, IRemoteCalendarLinkDO calendarLink = null)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                IncidentDO incidentDO = new IncidentDO();
                incidentDO.PrimaryCategory = "SyncFailure";
                incidentDO.SecondaryCategory = "Calendar";
                incidentDO.CreateTime = DateTime.Now;
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

    }
}
