using System;
using Data.Entities;
using Data.Infrastructure;
using Data.Interfaces;
using StructureMap;

namespace KwasantCore.Managers
{
    public class IncidentReporter
    {
        public void SubscribeToAlerts()
        {
            AlertManager.AlertEmailProcessingFailure += ProcessAlert_EmailProcessingFailure;
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
                incidentDO.PrimaryCategory = "Email";
                incidentDO.SecondaryCategory = "Intake Failure";
                incidentDO.CreateTime = Convert.ToDateTime(dateReceived);
                incidentDO.Priority = 5;
                incidentDO.Activity = "Created";
                incidentDO.Notes = errorMessage;
                _uow.IncidentRepository.Add(incidentDO);
                _uow.SaveChanges();
            }
        }


    }
}
