using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Liki.App.Services.AlertManager;

namespace Shnexy.Services
{
    public class CommunicationManager
    {



        //Register for interesting events

        public void SubscribeToAlerts()
        {
            AlertManager.alertCustomerCreated += new AlertManager.CustomerCreatedHandler(NewCustomerWorkflow);
        }

        public void NewCustomerWorkflow(BooqitAlertData eventData)
        {
           
            
        }


    }
}