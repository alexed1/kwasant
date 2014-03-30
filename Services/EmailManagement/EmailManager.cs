using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using Microsoft.Ajax.Utilities;
using Shnexy.Models;
using Shnexy.Services.APIManagement.Packagers.Mandrill;

namespace Shnexy.Services.EmailManagement
{
    public class EmailManager
    {
        #region Members

        private MandrillPackager MandrillAPI;

        #endregion

        #region Constructor

        /// <summary>
        /// Initialize EmailManager
        /// </summary>
        public EmailManager()
        {
            MandrillAPI = new MandrillPackager();
        }

        #endregion

        #region Method

        /// <summary>
        /// This implementation of Send uses the Mandrill API
        /// </summary>
        public void SendTemplate(string templateName, Email message, Dictionary<string, string> mergeFields)
        {
            var results = MandrillAPI.PostMessageSendTemplate(templateName, message, mergeFields);
        }

        //this processes a regular email
        public void Send(Email message)
        {

            var results = MandrillAPI.PostMessageSend(message);
            Debug.WriteLine(results);
        }

        public void Ping()
        {
            var results = MandrillAPI.PostPing();
            Debug.WriteLine(results);
        }

        //converts a standard email into a Email so we can easily auto-serialize into Mandrill JSON
        //public Email Convert(Email anEmail)
        //{
        //    Email curEmail = new Email();
        //    curEmail.FromEmail = curEmail.Sender.EmailAddressBody;
        //    curEmail.To[0] = curEmail.To.First(); //THIS ONLY WORKS FOR A SINGLE RECIPIENT!
        //    curEmail.Subject = curEmail.Subject;
        //    return curEmail;

        //}

        #endregion

        #region TestMethod

        ///TODO : It is test method, we will remove it later
        /// <summary>
        /// simulates the creation of a sendable email message and then sends it.
        /// </summary>
        public void Test()
        {
            string templateName = "order-complete-v1";

            Email message = new Email();
            message.To = new List<EmailAddress>();
            EmailAddress address = new EmailAddress();
            address.Email = "alexlucre1@gmail.com";
            address.Name = "Test Sender";
            message.To.Add(address);
            message.Subject = "test message";
            message.FromEmail = "sender@edelstein.org";
            message.FromName = "LeaseItKeepIt";
            message.Html = "foo";
            message.Text = "bar";

            Dictionary<string, string> mergeFields = new Dictionary<string, string>();
            mergeFields["First_Name"] = "Chad";
            mergeFields["OrderItem_Names"] = "80 inch Sony Television, model XBR059";
            mergeFields["Lease_Amount"] = "$453.43";
            mergeFields["Next_Payment_Date"] = "February 22, 2014";

           // SendTemplate(templateName, message, mergeFields);
            Send(message);
        }
        #endregion

    }
}








