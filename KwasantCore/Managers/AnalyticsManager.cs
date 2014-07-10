using System;
using Data.Entities;
using Data.Infrastructure;
using Data.Interfaces;
using Microsoft.WindowsAzure;
using StructureMap;
using Utilities.Logging;

namespace KwasantCore.Managers
{
    public class AnalyticsManager
    {
        public AnalyticsManager()
        {
            SubscribeToAlerts();
        }


        public void SubscribeToAlerts()
        {
              AlertManager.AlertBookingRequestCreated += ProcessBookingRequestCreated;
        }

        public void ProcessBookingRequestCreated(BookingRequestDO curBR)
        {
            FactDO curAction = new FactDO()
            {
                Name = "BookingRequest Created",
                PrimaryCategory = "Email",
                SecondaryCategory = "BookingRequest",
                Activity = "Created",
                CustomerId = curBR.User.Id,
                CreateDate = DateTime.Now,
                ObjectId = curBR.Id
            };
            curAction.Data = curAction.Name + ": ID= " + curAction.ObjectId;
            if (CloudConfigurationManager.GetSetting("LogLevel") == "Verbose")
                Logger.GetLogger().Info(curAction.Data);
            IUnitOfWork uow = ObjectFactory.GetInstance<IUnitOfWork>();
            uow.FactRepository.Add(curAction);
                uow.SaveChanges();

        }
    }
}
 