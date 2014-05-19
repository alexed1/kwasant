using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Data.Entities;
using KwasantCore.Managers.APIManager.Transmitters.Restful;
using Newtonsoft.Json;
using UtilitiesLib;
using JsonSerializer = KwasantCore.Managers.APIManager.Serializers.Json.JsonSerializer;

namespace KwasantCore.Managers.APIManager.Packagers.Mandrill
{ 
    //uses the Mandrill API at https://mandrillapp.com/settings/index
    public static class MandrillPackager
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
            if (handler != null) handler(errorCode, name, message, emailID); ;
        }

        #region Members

        private static string baseURL;
        private static JsonSerializer jsonSerializer;
        private static string MandrillKey;

        #endregion

        #region Constructor

        /// <summary>
        /// Initialize mandrill packager
        /// </summary>
        static MandrillPackager()
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
        public static void PostMessageSendTemplate(String templateName, EmailDO message, Dictionary<string, string> mergeFields)
        {
            RestfulCall curCall = new RestfulCall(baseURL, "/messages/send-template.json", Method.POST);
            MandrillTemplatePackage curTemplatePackage = new MandrillTemplatePackage(MandrillKey, message);

            curTemplatePackage.TemplateName = templateName;

            //ADD CUSTOM MERGE FIELDS
            //Currently we support just a single recipient. Mandrill's merge tag solution, though, requires a syntax that assume multiple recipients.
            //What we're doing here is just copying the email address from the EmailAddress object into a similar field called 'rcpt'
            //This will break the moment we use 'cc' or 'bcc' or put more than one addressee into the message.
            MandrillMergeRecipient curRecipient = new MandrillMergeRecipient();
            //curRecipient.Rcpt = message.To. FIX THIS

            //map the template-specific chunks of custom data that will be dyanmically integrated into the template at send time. Put them into a list that can be easily serialized.
            foreach (KeyValuePair<string, string> pair in mergeFields)
            {
                MandrillDynamicContentChunk curChunk = new MandrillDynamicContentChunk();
                curChunk.Name = pair.Key;
                curChunk.Content = pair.Value;
                curRecipient.Vars.Add(curChunk);

            }
            //message.MergeVars.Add(curRecipient); NEED A DIFFERENT WAY TO ADD MERGE VARS

            AssembleAndSend(curTemplatePackage, curCall);

        }


        public static void InitialiseWebhook(String url)
        {
            new Thread(() =>
            {
                //Delete old ones
                List<int> activeHookIDs = GetActiveWebHooks();
                DeleteOldWebHooks(activeHookIDs);

                InstantiateNewWebhook(url);
            }).Start();
        }

        private static void DeleteOldWebHooks(List<int> activeHookIDs)
        {
            foreach (int activeHookID in activeHookIDs)
            {
                RestfulCall curCall = new RestfulCall(baseURL, "/webhooks/delete.json", Method.POST);
                MandrillDeleteWebhook listRequest = new MandrillDeleteWebhook()
                {
                    Key = MandrillKey,
                    ID = activeHookID
                };
                string serialisedEmail = jsonSerializer.Serialize(listRequest);
                curCall.AddBody(serialisedEmail, "application/json");

                curCall.Execute();
            }
        }

        private static List<int> GetActiveWebHooks()
        {
            RestfulCall curCall = new RestfulCall(baseURL, "/webhooks/list.json", Method.POST);
            MandrillListWebhooks listRequest = new MandrillListWebhooks
            {
                Key = MandrillKey
            };
            string serialisedEmail = jsonSerializer.Serialize(listRequest);
            curCall.AddBody(serialisedEmail, "application/json");

            RestfulResponse response = curCall.Execute();
            MissingMemberHandling oldSetting = jsonSerializer.Settings.MissingMemberHandling;
            jsonSerializer.Settings.MissingMemberHandling = MissingMemberHandling.Ignore;
            List<ActiveMandrillWebhook> activeHooks = jsonSerializer.Deserialize<List<ActiveMandrillWebhook>>(response.Content);

            jsonSerializer.Settings.MissingMemberHandling = oldSetting;
            return activeHooks.Select(ah => ah.ID).ToList();
        }

        private static void InstantiateNewWebhook(string url)
        {
            RestfulCall curCall = new RestfulCall(baseURL, "/webhooks/add.json", Method.POST);
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

            string serialisedEmail = jsonSerializer.Serialize(addRequest);
            curCall.AddBody(serialisedEmail, "application/json");

            //Transmit the call
            curCall.Execute();
        }

        //======================================================
        //simple send with no merge variables or templates
        public static void PostMessageSend(EmailDO message)
        {
            RestfulCall curCall = new RestfulCall(baseURL, "/messages/send.json", Method.POST);
            MandrillBasePackage curBasePackage = new MandrillBasePackage(MandrillKey, message);
            AssembleAndSend(curBasePackage, curCall);
        }


        //FINAL ASSEMBLY AND TRANSMISSION
        //finish configuring a complete 'package' that can be auto-serialized into the json that Mandrill understands.
        private static void AssembleAndSend(MandrillBasePackage curTemplatePackage, RestfulCall curCall)
        {
            //serialize the email data and add it to the RestfulCall
            string serialisedEmail = jsonSerializer.Serialize(curTemplatePackage);
            curCall.AddBody(serialisedEmail, "application/json");

            //Transmit the call
            RestfulResponse response = curCall.Execute();

            HandleResponse(response.Content, curTemplatePackage.Email);

        }

        private static void HandleResponse(String responseStr, EmailDO email)
        {
            List<MandrillResponse> responses;
            try
            {
                responses = jsonSerializer.Deserialize<List<MandrillResponse>>(responseStr);
            }
            catch (JsonSerializationException)
            {
                OnEmailCriticalError(-1, "Return JSON could not be parsed.", responseStr, email.EmailID);
                throw;
            }
            foreach (MandrillResponse response in responses)
            {
                if (response.Status == MandrillResponse.MandrilSent)
                {
                    if (String.IsNullOrEmpty(response.RejectReason))
                    {
                        OnEmailSent(response._ID, email.EmailID);
                    }
                    else
                    {
                        OnEmailRejected(response._ID, response.RejectReason, email.EmailID);
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
                    OnEmailRejected(response._ID, response.RejectReason, email.EmailID);
                }
                else if (response.Status == MandrillResponse.MandrilInvalid)
                {
                    OnEmailCriticalError(response.Code, response.Name, response.Message, email.EmailID);
                    throw new Exception("Error sending email: " + response.Message);
                }
                else if (response.Status == MandrillResponse.MandrilError)
                {
                    OnEmailCriticalError(response.Code, response.Name, response.Message, email.EmailID);
                    throw new Exception("Error sending email: " + response.Message);
                }
                else
                {
                    OnEmailCriticalError(-1, "Unknown state", "State: " + response.Status, email.EmailID);
                    throw new Exception("Unknown email state: " + response.Status);
                }
            }
        }

        public static void HandleWebhookResponse(String responseStr)
        {
            List<MandrillWebhookResponse> responses;
            try
            {
                responses = jsonSerializer.Deserialize<List<MandrillWebhookResponse>>(responseStr);
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
            RestfulCall curCall = new RestfulCall(baseURL, "/users/ping.json", Method.POST);
            string pingstring = @"{ ""key"": """ + MandrillKey + @"""}";
            curCall.AddBody(pingstring, "application/json");
            RestfulResponse response = curCall.Execute();

            return response.Content;
        }
    }

    //=============================================================================================================================================
    //MANDRILL-SPECIFIC ENTITIES
    //These only really exist because it makes it really easy to auto serialize and deserialize from the specific JSON that Mandrill has defined.
    //They should not be used directly, but only by MandrillPackager

    public class MandrillListWebhooks
    {
        public String Key;
    }

    public class MandrillDeleteWebhook
    {
        public String Key;
        public int ID;
    }

    public class ActiveMandrillWebhook
    {
        public int ID;
        public String URL;
        public DateTime Created_At;
        public DateTime Last_Sent_At;
        public int Batches_Sent;
        public int Events_Sent;
        public String Description;
        public String AuthKey;
        public List<String> Events;
    }

    public class MandrillWebhookResponse
    {
        public class MandrillWebhookMessage
        {
            public int TS;
            public String Subject;
            public String Email;
            public List<String> Tags;
            public List<String> Opens;
            public List<String> Clicks;
            public String State;
            public List<String> SMTP_Events;
            public String Subaccount;
            public List<String> Resends;
            public String Reject;
            public String _ID;
            public String Sender;
            public String Template;

        }
        public string Event;
        public string _ID;
        public MandrillWebhookMessage Msg;
        public int TS;
    }

    public class MandrillResponse
    {
        public const String MandrilSent = "sent";
        public const String MandrilQueued = "queued";
        public const String MandrilScheduled = "scheduled";
        public const String MandrilRejected = "rejected";
        public const String MandrilInvalid = "invalid";
        public const String MandrilError = "error";

        public String Email;
        public String Status;
        public String RejectReason;
        public String _ID;
        public int Code;
        public String Name;
        public String Message;
    }

    public class MandrillWebhookAddRequest
    {
        public string Key;
        public string URL;
        public string Description;
        public List<String> Events;
    }


    public class MandrillBasePackage
    {
        #region Members

        public class MandrilEmail
        {
            public class MandrilEmailAddress
            {
                public string Email;
                public string Name;
                public string Type;
            }

            public class MandrilHeader
            {
                public string ReplyTo;
            }

            public class MandrilAttachment
            {
                public String Type;
                public String Name;
                public String Content;
            }

            public string HTML;
            public string Subject;
            public string FromEmail;
            public string FromName;
            public List<MandrilEmailAddress> To;
            public MandrilHeader Headers;
            public bool Important;
            public List<MandrilAttachment> Attachments;
            public List<MandrilAttachment> Images;
            public List<String> Tags;
        }

        public string Key;
        [JsonIgnore]
        public EmailDO Email;

        public MandrilEmail Message;
        public bool Async = false;

        #endregion

        #region Constructor
        public MandrillBasePackage(string curKey, EmailDO message)
        {
            Key = curKey;
            Email = message;

            Message = new MandrilEmail
            {
                HTML = message.HTMLText,
                Subject = message.Subject,
                FromEmail = message.From.Address,
                FromName = message.From.Name,
                To = message.To.Select(t => new MandrilEmail.MandrilEmailAddress { Email = t.Address, Name = t.Name, Type = "to" }).ToList(),
                Headers = null,
                Important = false,
                Tags = new List<string> { Email.EmailID.ToString() },
                Attachments = message.Attachments.Select(a =>
                {
                    byte[] file = a.Bytes;
                    string base64Version = Convert.ToBase64String(file, 0, file.Length);

                    return new MandrilEmail.MandrilAttachment
                    {
                        Content = base64Version,
                        Name = a.OriginalName,
                        Type = a.Type
                    };
                }).ToList(),
            };
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
        public MandrillTemplatePackage(string curKey, EmailDO email)
            : base(curKey, email)
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