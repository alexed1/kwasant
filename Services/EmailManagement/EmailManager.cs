using System;
using System.Collections.Generic;
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
        public void SendTemplate(string templateName, MandrillEmail message, Dictionary<string, string> mergeFields)
        {
            var results = MandrillAPI.PostMessageSendTemplate(templateName, message, mergeFields);
        }

        //this processes a regular email
        public void Send(Email message, Attachment curAttachment = null)
        {
            MandrillEmail curEmail = Convert(message);
            //if (curAttachment != null)
                //FIX THIScurEmail.Attachment = curAttachment;
            var results = MandrillAPI.PostMessageSend(curEmail);
        }

        //converts a standard email into a MandrillEmail so we can easily auto-serialize into Mandrill JSON
        public MandrillEmail Convert(Email curEmail)
        {
            MandrillEmail curMandrillEmail = new MandrillEmail();
            curMandrillEmail.FromEmail = curEmail.Sender.Email;
            curMandrillEmail.To[0] = curEmail.To_Addresses.First(); //THIS ONLY WORKS FOR A SINGLE RECIPIENT!
            curMandrillEmail.Subject = curEmail.Subject;
            return curMandrillEmail;

        }

        #endregion

        #region TestMethod

        ///TODO : It is test method, we will remover it later
        /// <summary>
        /// simulates the creation of a sendable email message and then sends it.
        /// </summary>
        public void Test()
        {
            string templateName = "order-complete-v1";

            MandrillEmail message = new MandrillEmail();
            message.To = new EmailAddress[1];
            EmailAddress address = new EmailAddress();
            address.Email = "poonam@cash1loans.com";
            address.Name = "poonam";
            message.To[0] = address;
            message.Subject = "test message";
            message.FromEmail = "sender@leaseitkeepit.com";
            message.FromName = "LeaseItKeepIt";

            Dictionary<string, string> mergeFields = new Dictionary<string, string>();
            mergeFields["First_Name"] = "Chad";
            mergeFields["OrderItem_Names"] = "80 inch Sony Television, model XBR059";
            mergeFields["Lease_Amount"] = "$453.43";
            mergeFields["Next_Payment_Date"] = "February 22, 2014";

            SendTemplate(templateName, message, mergeFields);
        }
        #endregion

    }
}




/// <summary>
///This class is structured to facilitate auto-serialization of data that gets sent using the Mandrill API. It is similar to our main Email class
/// We should probably get rid of it, and write manual serialization code
/// Another open issue is: should the EmailAddress be done as an array (like here) or as a Collection. The array is probably more cross-platform
/// </summary>
[Serializable]
public class MandrillEmail
{
    public string Html;
    public string Text;
    public string Subject;
    public string FromEmail;
    public string FromName;
    public List<Attachment> Attachments;
    public EmailAddress[] To;
    public List<MandrillEmailTemplateMergeRecipient> MergeVars;
    public MandrillEmail()
    {
        MergeVars = new List<MandrillEmailTemplateMergeRecipient> { };
    }
}

public class Attachment
{
    public string Type;
    public string Name;
    public string Content; 
}


//we are now using the "main" EmailAddress class and not creating one here.


/// <summary>
/// This is a standard email address. Note that email addresses can have a friendly name.
/// This class is structured to enable auto-serialization of data that gets sent using the Mandrill API.
/// </summary>
//[Serializable]
//public class EmailAddress
//{
//    public string Email;
//    public string Name;

//}
