using System;
using System.Collections.Generic;

namespace KwasantCore.Managers.APIManagers.Packagers.Mandrill.APIStructures
{
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
}