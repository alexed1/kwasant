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
