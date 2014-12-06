using System;
using System.Collections.Generic;
using StructureMap;
using Utilities;

namespace KwasantCore.Services
{
    public class Envelope
    {
        private static readonly IConfigRepository ConfigRepository = ObjectFactory.GetInstance<IConfigRepository>();
        public static readonly Dictionary<String, String> TemplateDescriptionMapping = new Dictionary<string, string>
        {
            { ConfigRepository.Get("welcome_to_kwasant_template"), "Welcome to Kwasant" },
            { ConfigRepository.Get("CR_template_for_creator"), "Negotiation request" },
            { ConfigRepository.Get("CR_template_for_precustomer"), "Negotiation request" },
            { ConfigRepository.Get("ForgotPassword_template"), "Forgot Password" },
            { ConfigRepository.Get("User_Settings_Notification"), "User Settings Notification" },
            { ConfigRepository.Get("user_credentials"), "User Credentials" },
            { ConfigRepository.Get("InvitationInitial_template"), "Event Invitation" },
            { ConfigRepository.Get("InvitationUpdate_template"), "Event Invitation Update" },
            { ConfigRepository.Get("SimpleEmail_template"), "Simple Email" },
        };
    }
}
