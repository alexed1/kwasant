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
    public class MandrillPackager
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
        public MandrillPackager()
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
        public string PostMessageSendTemplate(String templateName, MandrillEmail message, Dictionary<string, string> mergeFields)
        {
            RestfulCall curCall = new RestfulCall(baseURL, "/messages/send-template.json", Method.POST);
            MandrillPackage curPackage = new MandrillPackage(MandrillKey);

            curPackage.TemplateName = templateName;

            //ADD CUSTOM MERGE FIELDS
            //Currently we support just a single recipient. Mandrill's merge tag solution, though, requires a syntax that assume multiple recipients.
            //What we're doing here is just copying the email address from the EmailAddress object into a similar field called 'rcpt'
            //This will break the moment we use 'cc' or 'bcc' or put more than one addressee into the message.
            var curRecipient = new MandrillEmailTemplateMergeRecipient();
            curRecipient.Rcpt = message.To[0].Email;

            //map the template-specific chunks of custom data that will be dyanmically integrated into the template at send time. Put them into a list that can be easily serialized.
            foreach (var pair in mergeFields)
            {
                MandrillEmailTemplateContentChunk curChunk = new MandrillEmailTemplateContentChunk();
                curChunk.Name = pair.Key;
                curChunk.Content = pair.Value;
                curRecipient.Vars.Add(curChunk);

            }
            message.MergeVars.Add(curRecipient);

            return AssembleAndSend(curPackage, curCall, message);

        }

        //simple send with no merge variables or templates
        public string PostMessageSend(MandrillEmail message)
        {
            RestfulCall curCall = new RestfulCall(baseURL, "/messages/send.json", Method.POST);
            MandrillPackage curPackage = new MandrillPackage(MandrillKey);
            return AssembleAndSend(curPackage, curCall, message);
        }


        //FINAL ASSEMBLY AND TRANSMISSION
        //finish configuring a complete 'package' that can be auto-serialized into the json that Mandrill understands.
        public string AssembleAndSend(MandrillPackage curPackage, RestfulCall curCall, MandrillEmail message)
        {
           
            curPackage.Message = message;

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
public class MandrillPackage
{
    #region Members

    public string Key;
    public string TemplateName;
    public List<MandrillEmailTemplateContentChunk> TemplateContent;
    public MandrillEmail Message;

    #endregion

    #region Constructor
    public MandrillPackage(string curKey)
    {
        Key = curKey;
    }

    #endregion
}


[Serializable]
public class MandrillEmailTemplateContentChunk
{
    public string Name;
    public string Content;
}

/// <summary>
/// In the Mandrill JSON, dynamic merge data must be provided on a per-recipient basis. Each recipient can have a List of dynamic chunks.
/// </summary>
[Serializable]
public class MandrillEmailTemplateMergeRecipient
{
    #region Members

    public string Rcpt;
    public List<MandrillEmailTemplateContentChunk> Vars;

    #endregion

    #region Constructor
    public MandrillEmailTemplateMergeRecipient()
    {
        Vars = new List<MandrillEmailTemplateContentChunk> { };
    }

    #endregion
}









