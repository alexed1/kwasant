using System;
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
            AlertManager.AlertError_EmailSendFailure += ProcessEmailSendFailure;
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

        private void ProcessEmailSendFailure(int emailId, string message)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                IncidentDO incidentDO = new IncidentDO();
                incidentDO.PrimaryCategory = "EmailFailure";
                incidentDO.SecondaryCategory = "Email";
                incidentDO.CreateTime = DateTime.Now;
                ;
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

    }
}
