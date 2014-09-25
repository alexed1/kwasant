using System;
using System.Collections.Generic;

namespace KwasantCore.Managers.APIManagers.Packagers.Mandrill.APIStructures
{
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
}