using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using Configuration;
using Daemons;
using Data.Entities;
using Data.Interfaces;
using Data.Repositories;
using KwasantWeb;
using Microsoft.Ajax.Utilities;
using Microsoft.Owin;
using Owin;
using StructureMap;

[assembly: OwinStartup(typeof(KwasantWeb.Startup))]

namespace KwasantWeb
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureDaemons();
            ConfigureCommunicationConfigs();
        }


        //SeedDatabases
        //Ensure that critical configuration information is present in the database
        //Twilio SMS messages are based on CommunicationConfigurations
        //Database should have a CommunicationConfiguration that sends an SMS to 14158067915
        //Load Repository and query for CommunicationConfigurations. If null, create one set to 14158067915
        //If not null, make sure that at least one exists where the ToAddress is 14158067915
        public void ConfigureCommunicationConfigs()
        {
            IUnitOfWork uow = ObjectFactory.GetInstance<IUnitOfWork>();
            CommunicationConfigurationRepository communicationConfigurationRepo = new CommunicationConfigurationRepository(uow);
            List<CommunicationConfigurationDO> curConfigureCommunicationConfigs = communicationConfigurationRepo.GetAll().ToList()



            if (!(curConfigureCommunicationConfigs.Find(
                    config => config.ToAddress == ConfigurationManager.AppSettings["MainSMSAlertNumber"]) != null))
                // it is not true that there is at least one commConfig that has the Main alert number
                {
                    CommunicationConfigurationDO curCommConfig = new CommunicationConfigurationDO();
                        curCommConfig.ToAddress = ConfigurationManager.AppSettings["MainSMSAlertNumber"];
                        communicationConfigurationRepo.Add(curCommConfig);
                        communicationConfigurationRepo.UnitOfWork.SaveChanges();
                }
           
            }

        public void AddMainSMSAlertToDb(CommunicationConfigurationRepository communicationConfigurationRepo)
        {
           
        }


        private static void ConfigureDaemons()
        {
            DaemonSettings daemonConfig = ConfigurationManager.GetSection("daemonSettings") as DaemonSettings;
            if (daemonConfig != null)
            {
                if (daemonConfig.Enabled)
                {
                    foreach (DaemonConfig daemon in daemonConfig.Daemons)
                    {
                        if (daemon.Enabled)
                        {
                            Type type = Type.GetType(daemon.InitClass, true);
                            Daemon obj = Activator.CreateInstance(type) as Daemon;
                            if (obj == null)
                                throw new ArgumentException(
                                    string.Format(
                                        "An daemon must implement IDaemon. Type '{0}' does not implement the interface.",
                                        type.Name));
                            obj.Start();
                        }
                    }
                }
            }
        }
    }
}
