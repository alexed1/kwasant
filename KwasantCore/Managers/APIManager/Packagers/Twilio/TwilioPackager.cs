using System;
using System.Configuration;
using Microsoft.WindowsAzure;
using Twilio;

namespace KwasantCore.Managers.APIManager.Packagers.Twilio
{
    public class TwilioPackager
    {
        private const string AccountSIDWebConfigName = "TWILIO_SID";
        private const string AuthTokenWebConfigName = "TWILIO_TOKEN";
        private const string FromNumberWebConfigName = "TwilioFromNumber";

        private readonly TwilioRestClient _twilio;
        private readonly String _twilioFromNumber;
        public TwilioPackager()
        {
           


            //this will be overridden by Azure settings with the same name, on RC, Staging, and Production
            string accountSID = CloudConfigurationManager.GetSetting(AccountSIDWebConfigName);
            string accountAuthKey = CloudConfigurationManager.GetSetting(AuthTokenWebConfigName);
            _twilioFromNumber = CloudConfigurationManager.GetSetting(FromNumberWebConfigName);

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
            SMSMessage result = _twilio.SendSmsMessage(_twilioFromNumber, number, message);
            if (result.RestException != null)
                throw new Exception(result.RestException.Message);
            return result;
        }
    }
}
