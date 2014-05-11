using System;
using System.Configuration;
using Twilio;

namespace KwasantCore.Managers.APIManager.Packagers.Twilio
{
    public class TwilioPackager
    {
        //private const string AccountSIDWebConfigName = "TWILIO_SID";
        //private const string AuthTokenWebConfigName = "TWILIO_TOKEN";
        private const string FromNumberWebConfigName = "TwilioFromNumber";

        private readonly TwilioRestClient _twilio;
        private readonly String _twilioFromNumber;
        public TwilioPackager()
        {
           

            // Try to access Azure seetings before falling back on the test account in the config
           // if (!(Services.Settings.TryGetValue("TWILIO.SID", out accountSID))
            //    accountSID = ConfigurationManager.AppSettings[AccountSIDWebConfigName];

            //this will be overridden by Azure settings with the same name, on RC, Staging, and Production
            string accountSID = ConfigurationManager.AppSettings["TWILIO_SID"];
            string accountAuthKey = ConfigurationManager.AppSettings["TWILIO_TOKEN"];
            _twilioFromNumber = ConfigurationManager.AppSettings[FromNumberWebConfigName];

            if (String.IsNullOrEmpty(accountSID))
                throw new ArgumentNullException(AccountSIDWebConfigName, "Value must be set in web.config");

            if (String.IsNullOrEmpty(accountAuthKey))
                throw new ArgumentNullException(AuthTokenWebConfigName, "Value must be set in web.config");

            if (String.IsNullOrEmpty(_twilioFromNumber))
                throw new ArgumentNullException(FromNumberWebConfigName, "Value must be set in web.config");

            _twilio = new TwilioRestClient(accountSID, accountAuthKey);
        }

        public SMSMessage SendSMS(String number, String message)
        {
            return _twilio.SendSmsMessage(_twilioFromNumber, number, message);
        }
    }
}
