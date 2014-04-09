using System;
using System.Collections.Generic;
using Data.Models;
using DBTools.Managers.APIManager.Serializers.Json;
using DBTools.Managers.APIManager.Transmitters.Restful;
using UtilitiesLib;

namespace DBTools.Managers.APIManager.Packagers.Mandrill
{ //uses the Mandrill API at https://mandrillapp.com/settings/index
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
            MandrillKey = "Nr9OJgXzpEgaibv4fIuudQ"; //this is currently tied to the alex@edelstein.org mandrill account. https://maginot.atlassian.net/wiki/display/SH/Email+Systems
        }

        #endregion






        #region Method

        /// <summary>
        ///Mandrill's API essentially inserts an array of email addresses inside a message which is put with a key inside what we'll call a "package"
        ///See https://mandrillapp.com/api/docs/messages.JSON.html#method=send
        /// </summary>
        public string PostMessageSendTemplate(String templateName, Email message, Dictionary<string, string> mergeFields)
        {
            var curCall = new RestfulCall(baseURL, "/messages/send-template.json", Method.POST);
            var curTemplatePackage = new MandrillTemplatePackage(MandrillKey);

            curTemplatePackage.TemplateName = templateName;

            //ADD CUSTOM MERGE FIELDS
            //Currently we support just a single recipient. Mandrill's merge tag solution, though, requires a syntax that assume multiple recipients.
            //What we're doing here is just copying the email address from the EmailAddress object into a similar field called 'rcpt'
            //This will break the moment we use 'cc' or 'bcc' or put more than one addressee into the message.
            var curRecipient = new MandrillMergeRecipient();
            //curRecipient.Rcpt = message.To. FIX THIS

            //map the template-specific chunks of custom data that will be dyanmically integrated into the template at send time. Put them into a list that can be easily serialized.
            foreach (var pair in mergeFields)
            {
                var curChunk = new MandrillDynamicContentChunk();
                curChunk.Name = pair.Key;
                curChunk.Content = pair.Value;
                curRecipient.Vars.Add(curChunk);

            }
            //message.MergeVars.Add(curRecipient); NEED A DIFFERENT WAY TO ADD MERGE VARS

            return AssembleAndSend(curTemplatePackage, curCall, message);

        }


        //======================================================
        //simple send with no merge variables or templates
        public string PostMessageSend(Email message)
        {
            var curCall = new RestfulCall(baseURL, "/messages/send.json", Method.POST);
            var curBasePackage = new MandrillBasePackage(MandrillKey);
            return AssembleAndSend(curBasePackage, curCall, message);
        }


        //FINAL ASSEMBLY AND TRANSMISSION
        //finish configuring a complete 'package' that can be auto-serialized into the json that Mandrill understands.
        public string AssembleAndSend(MandrillBasePackage curTemplatePackage, RestfulCall curCall, Email message)
        {

            curTemplatePackage.Message = message;
            curTemplatePackage.Key = MandrillKey;

            //serialize the email data and add it to the RestfulCall
            curCall.AddBody(jsonSerializer.Serialize(curTemplatePackage), "application/json");

            //Transmit the call
            var response = curCall.Execute();

            return response.Content;

        }

  
        #endregion


        //for testing Mandrill if things are broken
        public string PostPing()
        {
            RestfulCall curCall = new RestfulCall(baseURL, "/users/ping.json", Method.POST);
            string pingstring = @"{ ""key"": """ + MandrillKey + @"""}";
            curCall.AddBody(pingstring, "application/json");
            var response = curCall.Execute();

            return response.Content;
        }
    }

//=============================================================================================================================================
//MANDRILL-SPECIFIC ENTITIES
//These only really exist because it makes it really easy to auto serialize and deserialize from the specific JSON that Mandrill has defined.
//They should not be used directly, but only by MandrillPackager


    public class MandrillBasePackage
    {
        #region Members

        public string Key;
        public Email Message;

        #endregion

        #region Constructor
        public MandrillBasePackage(string curKey)
        {
            Key = curKey;
        }

        #endregion
    }

    /// <summary>
    /// This package combines an email message with mandrill-specific template chunks and a Mandrill key
    /// </summary>
    public class MandrillTemplatePackage : MandrillBasePackage
    {
        #region Members

   
        public string TemplateName;
        public List<MandrillDynamicContentChunk> TemplateContent;


        #endregion

        #region Constructor
        public MandrillTemplatePackage(string curKey) : base(curKey)
        {
 
        }

        #endregion
    }



    [Serializable]
    public class MandrillDynamicContentChunk
    {
        public string Name;
        public string Content;
    }

    /// <summary>
    /// In the Mandrill JSON, dynamic merge data must be provided on a per-recipient basis. Each recipient can have a List of dynamic chunks.
    /// </summary>
    [Serializable]
    public class MandrillMergeRecipient
    {
        #region Members

        public string Rcpt;
        public List<MandrillDynamicContentChunk> Vars;

        #endregion

        #region Constructor
        public MandrillMergeRecipient()
        {
            Vars = new List<MandrillDynamicContentChunk> { };
        }

        #endregion
    }
}