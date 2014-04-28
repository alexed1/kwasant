using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Data.Entities;

namespace KwasantCore.Managers.CommunicationManager
{
    public class CommunicationManager
    {



        //Register for interesting events

        public void SubscribeToAlerts()
        {
            AlertManager.alertCustomerCreated += NewCustomerWorkflow;
        }


        //this is called when a new customer is created, because the communication manager has subscribed to the alertCustomerCreated alert.
        public void NewCustomerWorkflow(BooqitAlertData eventData)
        {
           
            Debug.WriteLine("NewCustomer has been created.");
        }

        public void ProcessBRNotifications(IList<BookingRequestDO> bookingRequests)
        {
            
        }
    }
}