using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.Owin;
using Shnexy.Models;
using Shnexy.Services.APIManagement.Serializers.Json;
using Shnexy.Services.APIManagement.Transmitters.Restful;
using Shnexy.Utilities;

namespace Shnexy.Services.APIManagement.Packagers.Mandrill

{
    //uses the Mandrill API at https://mandrillapp.com/settings/index
    public class MandrillPackage_SendTemplater
    {
        #region Members

        string baseURL;
        private JsonSerializer jsonSerializer;
        string MandrillKey;

        #endregion

        #region Constructor

        /// <summary>
        /// Initialize mandrill packager
        /// </summary>
        public MandrillPackage_SendTemplater()
        {
            baseURL = "https://mandrillapp.com/api/1.0/";
            jsonSerializer = new JsonSerializer();
            MandrillKey = "kTVEu8YY1OcZGnUUNnO9Hg"; //this is currently tied to the alex@leaseitkeepit.com mandrill account. details https://cmretail.atlassian.net/wiki/display/LIKI/Email+Integration+Details
        }

        #endregion






        #region Method

        /// <summary>
        ///Mandrill's API essentially inserts an array of email addresses inside a message which is put with a key inside what we'll call a "package"
        ///See https://mandrillapp.com/api/docs/messages.JSON.html#method=send
        /// </summary>
        public string PostMessageSendTemplate(String templateName, Email message, Dictionary<string, string> mergeFields)
        {
            RestfulCall curCall = new RestfulCall(baseURL, "/messages/send-template.json", Method.POST);
            MandrillPackage_SendTemplate curPackage = new MandrillPackage_SendTemplate(MandrillKey);

            curPackage.TemplateName = templateName;

            //ADD CUSTOM MERGE FIELDS
            //Currently we support just a single recipient. Mandrill's merge tag solution, though, requires a syntax that assume multiple recipients.
            //What we're doing here is just copying the email address from the EmailAddress object into a similar field called 'rcpt'
            //This will break the moment we use 'cc' or 'bcc' or put more than one addressee into the message.
            var curRecipient = new EmailTemplateMergeRecipient();
            curRecipient.Rcpt = message.To[0].Email;

            //map the template-specific chunks of custom data that will be dyanmically integrated into the template at send time. Put them into a list that can be easily serialized.
            foreach (var pair in mergeFields)
            {
                EmailTemplateContentChunk curChunk = new EmailTemplateContentChunk();
                curChunk.Name = pair.Key;
                curChunk.Content = pair.Value;
                curRecipient.Vars.Add(curChunk);

            }
            //message.MergeVars.Add(curRecipient); NEED A DIFFERENT WAY TO ADD MERGE VARS

            return AssembleAndSend(curPackage, curCall, message);

        }

        //simple send with no merge variables or templates
        public string PostMessageSend(Email message)
        {
            RestfulCall curCall = new RestfulCall(baseURL, "/messages/send.json", Method.POST);
            MandrillPackage_SendNoTemplate curPackage = new MandrillPackage_SendNoTemplate(MandrillKey);
            return AssembleAndSend_NoTemplate(curPackage, curCall, message);
        }


        //FINAL ASSEMBLY AND TRANSMISSION
        //finish configuring a complete 'package' that can be auto-serialized into the json that Mandrill understands.
        public string AssembleAndSend(MandrillPackage_SendTemplate curPackage, RestfulCall curCall, Email message)
        {

            curPackage.Message = new Email();
            curPackage.Key = MandrillKey;
                //message;

            //serialize the email data and add it to the RestfulCall
            curCall.AddBody(jsonSerializer.Serialize(curPackage), "application/json");

            //Transmit the call
            var response = curCall.Execute();

            return response.Content;

        }

        //DRY THIS UP
        public string AssembleAndSend_NoTemplate(MandrillPackage_SendNoTemplate curPackage, RestfulCall curCall, Email message)
        {

            curPackage.Message = message;
            curPackage.Key = MandrillKey;
       

            //serialize the email data and add it to the RestfulCall
            curCall.AddBody(jsonSerializer.Serialize(curPackage), "application/json");

            //Transmit the call
            var response = curCall.Execute();

            return response.Content;

        }

        #endregion

    }
}


//MANDRILL-SPECIFIC ENTITIES
//These only really exist because it makes it really easy to auto serialize and deserialize from the specific JSON that Mandrill has defined.

/// <summary>
/// This package combines an email message with mandrill-specific template chunks and a Mandrill key
/// </summary>
public class MandrillPackage_SendTemplate
{
    #region Members

    public string Key;
    public string TemplateName;
    public List<EmailTemplateContentChunk> TemplateContent;
    public Email Message;

    #endregion

    #region Constructor
    public MandrillPackage_SendTemplate(string curKey)
    {
        Key = curKey;
    }

    #endregion
}

public class MandrillPackage_SendNoTemplate
{
    #region Members

    public string Key;
    public Email Message;

    #endregion

    #region Constructor
    public MandrillPackage_SendNoTemplate(string curKey)
    {
        Key = curKey;
    }

    #endregion
}



[Serializable]
public class EmailTemplateContentChunk
{
    public string Name;
    public string Content;
}

/// <summary>
/// In the Mandrill JSON, dynamic merge data must be provided on a per-recipient basis. Each recipient can have a List of dynamic chunks.
/// </summary>
[Serializable]
public class EmailTemplateMergeRecipient
{
    #region Members

    public string Rcpt;
    public List<EmailTemplateContentChunk> Vars;

    #endregion

    #region Constructor
    public EmailTemplateMergeRecipient()
    {
        Vars = new List<EmailTemplateContentChunk> { };
    }

    #endregion
}









