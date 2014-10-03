using System.Collections.Generic;

namespace KwasantCore.Managers.APIManagers.Packagers.Mandrill.APIStructures
{
    public class MandrillWebhookAddRequest
    {
        public string Key;
        public string URL;
        public string Description;
        public List<string> Events;
    }
}