using System;
using System.Collections.Generic;
using System.Diagnostics;
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


        //this is called when a new customer is created, because the communication manager has subscribed to the alertCustomerCreated alert.
        public void NewCustomerWorkflow(BooqitAlertData eventData)
        {
           
            Debug.WriteLine("NewCustomer has been created.");
        }


    }
}