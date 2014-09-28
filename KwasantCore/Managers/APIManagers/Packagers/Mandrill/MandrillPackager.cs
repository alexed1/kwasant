using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Data.Entities;
using KwasantCore.ExternalServices.REST;
using KwasantCore.Managers.APIManager.Packagers;
using KwasantCore.Managers.APIManagers.Packagers.Mandrill.APIStructures;
using Newtonsoft.Json;
using StructureMap;
using Utilities;
using JsonSerializer = KwasantCore.Managers.APIManagers.Serializers.Json.JsonSerializer;

namespace KwasantCore.Managers.APIManagers.Packagers.Mandrill
{ 
    //uses the Mandrill API at https://mandrillapp.com/settings/index
    public class MandrillPackager : IEmailPackager
    {
        public delegate void EmailSuccessArgs(string mandrillID, int emailID);
        public static event EmailSuccessArgs EmailSent;

        private static void OnEmailSent(string mandrillID, int emailid)
        {
            EmailSuccessArgs handler = EmailSent;
            if (handler != null) handler(mandrillID, emailid);
        }

        public delegate void EmailRejectedArgs(string mandrillID, string rejectReason, int emailID);
        public static event EmailRejectedArgs EmailRejected;

        private static void OnEmailRejected(string mandrillID, string rejectReason, int emailid)
        {
            EmailRejectedArgs handler = EmailRejected;
            if (handler != null) handler(mandrillID, rejectReason, emailid);
        }

        public delegate void EmailCriticalErrorArgs(int errorCode, string name, string message, int emailID);
        public static event EmailCriticalErrorArgs EmailCriticalError;

        private static void OnEmailCriticalError(int errorCode, string name, string message, int emailID)
        {
            EmailCriticalErrorArgs handler = EmailCriticalError;
            if (handler != null) handler(errorCode, name, message, emailID);
        }

        #region Members

        private static readonly string BaseURL;
        private static readonly JsonSerializer JsonSerializer;
        private static readonly string MandrillKey;

        #endregion

        #region Constructor

        /// <summary>
        /// Initialize mandrill packager
        /// </summary>
        static MandrillPackager()
        {
            BaseURL = "https://mandrillapp.com/api/1.0/";
            JsonSerializer = new JsonSerializer();
            MandrillKey = "Nr9OJgXzpEgaibv4fIuudQ"; //this is currently tied to the alex@edelstein.org mandrill account. https://maginot.atlassian.net/wiki/display/SH/Email+Systems
        }

        #endregion

        #region Method

        /// <summary>
        ///Mandrill's API essentially inserts an array of email addresses inside a message which is put with a key inside what we'll call a "package"
        ///See https://mandrillapp.com/api/docs/messages.JSON.html#method=send
        /// </summary>
        public static void PostMessageSendTemplate(String templateName, EmailDO message, IDictionary<string, string> mergeFields)
        {
            var curCall = CreateRestfulCall(BaseURL, "/messages/send-template.json", Method.POST);
            MandrillTemplatePackage curTemplatePackage = new MandrillTemplatePackage(MandrillKey, message)
            {
                TemplateName = templateName
            };

            //map the template-specific chunks of custom data that will be dyanmically integrated into the template at send time. Put them into a list that can be easily serialized.
            if (mergeFields != null)
            {
                curTemplatePackage.Message.GlobalMergeVars =
                    mergeFields.Select(pair => new MandrillDynamicContentChunk
                    {
                        Name = pair.Key,
                        Content = pair.Value
                    })
                        .ToList();
            }

            AssembleAndSend(curTemplatePackage, curCall);
        }


        public static void InitialiseWebhook(String url)
        {
            new Thread(() =>
            {
                //Delete old ones
                IEnumerable<int> activeHookIDs = GetActiveWebHooks();
                DeleteOldWebHooks(activeHookIDs);

                InstantiateNewWebhook(url);
            }).Start();
        }

        private static void DeleteOldWebHooks(IEnumerable<int> activeHookIDs)
        {
            foreach (int activeHookID in activeHookIDs)
            {
                var curCall = CreateRestfulCall(BaseURL, "/webhooks/delete.json", Method.POST);
                MandrillDeleteWebhook listRequest = new MandrillDeleteWebhook
                {
                    Key = MandrillKey,
                    ID = activeHookID
                };
                string serialisedEmail = JsonSerializer.Serialize(listRequest);
                curCall.AddBody(serialisedEmail, "application/json");

                curCall.Execute();
            }
        }

        private static IEnumerable<int> GetActiveWebHooks()
        {
            var curCall = CreateRestfulCall(BaseURL, "/webhooks/list.json", Method.POST);
            MandrillListWebhooks listRequest = new MandrillListWebhooks
            {
                Key = MandrillKey
            };
            string serialisedEmail = JsonSerializer.Serialize(listRequest);
            curCall.AddBody(serialisedEmail, "application/json");

            var response = curCall.Execute();
            MissingMemberHandling oldSetting = JsonSerializer.Settings.MissingMemberHandling;
            JsonSerializer.Settings.MissingMemberHandling = MissingMemberHandling.Ignore;
            List<ActiveMandrillWebhook> activeHooks = JsonSerializer.Deserialize<List<ActiveMandrillWebhook>>(response.Content);

            JsonSerializer.Settings.MissingMemberHandling = oldSetting;
            return activeHooks.Select(ah => ah.ID).ToList();
        }

        private static void InstantiateNewWebhook(string url)
        {
            var curCall = CreateRestfulCall(BaseURL, "/webhooks/add.json", Method.POST);
            MandrillWebhookAddRequest addRequest = new MandrillWebhookAddRequest
            {
                Key = MandrillKey,
                Description = "Kwasant webhook",
                Events = new List<string>
                {
                    "send",
                    "hard_bounce",
                    "soft_bounce",
                    "reject"
                },
                URL = url
            };

            string serialisedEmail = JsonSerializer.Serialize(addRequest);
            curCall.AddBody(serialisedEmail, "application/json");

            //Transmit the call
            curCall.Execute();
        }

        //======================================================
        //simple send with no merge variables or templates
        public static void PostMessageSend(EmailDO message)
        {
            var curCall = CreateRestfulCall(BaseURL, "/messages/send.json", Method.POST);
            MandrillBasePackage curBasePackage = new MandrillBasePackage(MandrillKey, message);
            AssembleAndSend(curBasePackage, curCall);
        }


        //FINAL ASSEMBLY AND TRANSMISSION
        //finish configuring a complete 'package' that can be auto-serialized into the json that Mandrill understands.
        private static void AssembleAndSend(MandrillBasePackage curTemplatePackage, IRestfullCall curCall)
        {
            //serialize the email data and add it to the RestfulCall
            string serialisedEmail = JsonSerializer.Serialize(curTemplatePackage);
            curCall.AddBody(serialisedEmail, "application/json");

            //Transmit the call
            var response = curCall.Execute();

            HandleResponse(response.Content, curTemplatePackage.Email);
        }

        private static void HandleResponse(String responseStr, EmailDO email)
        {
            List<MandrillResponse> responses;
            try
            {
                // Max Kostyrkin: got responseStr = '[]' and this was deserialized as null.
                responses = JsonSerializer.Deserialize<List<MandrillResponse>>(responseStr) ??
                    new List<MandrillResponse>();
            }
            catch (JsonSerializationException)
            {
                OnEmailCriticalError(-1, "Return JSON could not be parsed.", responseStr, email.Id);
                throw;
            }
            foreach (MandrillResponse response in responses)
            {
                if (response.Status == MandrillResponse.MandrilSent)
                {
                    if (String.IsNullOrEmpty(response.RejectReason))
                    {
                        OnEmailSent(response._ID, email.Id);
                    }
                    else
                    {
                        OnEmailRejected(response._ID, response.RejectReason, email.Id);
                    }
                }
                else if (response.Status == MandrillResponse.MandrilQueued)
                {
                    //This will be processed in our webhook
                }
                else if (response.Status == MandrillResponse.MandrilScheduled)
                {
                    //This will be processed in our webhook
                }
                else if (response.Status == MandrillResponse.MandrilRejected)
                {
                    OnEmailRejected(response._ID, response.RejectReason, email.Id);
                }
                else if (response.Status == MandrillResponse.MandrilInvalid)
                {
                    OnEmailCriticalError(response.Code, response.Name, response.Message, email.Id);
                    throw new Exception("Error sending email: " + response.Message);
                }
                else if (response.Status == MandrillResponse.MandrilError)
                {
                    OnEmailCriticalError(response.Code, response.Name, response.Message, email.Id);
                    throw new Exception("Error sending email: " + response.Message);
                }
                else
                {
                    OnEmailCriticalError(-1, "Unknown state", "State: " + response.Status, email.Id);
                    throw new Exception("Unknown email state: " + response.Status);
                }
            }
        }

        public static void HandleWebhookResponse(String responseStr)
        {
            List<MandrillWebhookResponse> responses;
            try
            {
                responses = JsonSerializer.Deserialize<List<MandrillWebhookResponse>>(responseStr);
            }
            catch (JsonSerializationException)
            {
                OnEmailCriticalError(-1, "Return JSON could not be parsed.", responseStr, -1);
                throw;
            }
            foreach (MandrillWebhookResponse response in responses)
            {
                string firstTag = response.Msg.Tags.FirstOrDefault();
                if (firstTag == null)
                {
                    OnEmailCriticalError(-1, "No email ID was stored in tags.", "An email webhook was recieved, but we couldn't identify the email.", -1);
                    return;
                }
                int emailID = int.Parse(firstTag);
                if (response.Msg.State == MandrillResponse.MandrilSent)
                {
                    if (String.IsNullOrEmpty(response.Msg.Reject))
                    {
                        OnEmailSent(response._ID, emailID);
                    }
                    else
                    {
                        OnEmailRejected(response._ID, response.Msg.Reject, emailID);
                    }
                }
                else if (response.Msg.State == MandrillResponse.MandrilRejected)
                {
                    OnEmailRejected(response._ID, response.Msg.Reject, emailID);
                }
                else
                {
                    OnEmailCriticalError(-1, "Unknown state", "State: " + response.Msg.State, emailID);
                }
            }
        }
        #endregion

        //for testing Mandrill if things are broken
        public static string PostPing()
        {
            var curCall = CreateRestfulCall(BaseURL, "/users/ping.json", Method.POST);
            string pingstring = @"{ ""key"": """ + MandrillKey + @"""}";
            curCall.AddBody(pingstring, "application/json");
            var response = curCall.Execute();

            return response.Content;
        }

        #region Implementation of IEmailPackager

        public void Send(EnvelopeDO envelope)
        {
            if (envelope == null)
                throw new ArgumentNullException("envelope");
            if (!string.Equals(envelope.Handler, EnvelopeDO.MandrillHander))
                throw new ArgumentException(@"This envelope should not be handled with Mandrill.", "envelope");
            if (envelope.Email == null)
                throw new ArgumentException(@"This envelope has no Email.", "envelope");
            if (envelope.Email.Recipients.Count == 0)
                throw new ArgumentException(@"This envelope has no recipients.", "envelope");
            PostMessageSendTemplate(envelope.TemplateName, envelope.Email, envelope.MergeData);
        }

        private static IRestfullCall CreateRestfulCall(String baseURL, String resource, Method method)
        {
            var restfulCall = ObjectFactory.GetInstance<IRestfullCall>();
            restfulCall.Initialize(baseURL, resource, method);
            return restfulCall;
        }

        #endregion
    }
}