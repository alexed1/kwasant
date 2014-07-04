using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Data.Repositories;
using Data.Infrastructure;
using StructureMap;
using Data.Interfaces;
using Data.Repositories;
using Data.Entities;
namespace Data.Infrastructure
{
   public class IncidentReporter
    {
        public void SubscribeToAlerts()
        {
            AlertManager.AlertEmailProcessingFailure += ProcessAlert_EmailProcessingFailure;
        }

        public void ProcessAlert_EmailProcessingFailure(string from, string dateReceived)
        {
            using (var _uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                IncidentRepository incidentRepo = _uow.IncidentRepository;
                IncidentDO incidentDO = new IncidentDO();
                incidentDO.PrimaryCategory = from;
                incidentDO.SecondaryCategory = "intake";
                incidentDO.CreateTime = Convert.ToDateTime(dateReceived);
                incidentDO.Priority = 5;
                incidentDO.Activity = "Created";
                incidentRepo.Add(incidentDO);
                _uow.SaveChanges();
            }
        }

    }
}
