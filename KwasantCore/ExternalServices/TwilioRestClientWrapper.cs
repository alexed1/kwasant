using Twilio;

namespace KwasantCore.ExternalServices
{
    public class TwilioRestClientWrapper : ITwilioRestClient
    {
        private TwilioRestClient _internalClient;
        public SMSMessage SendSmsMessage(string from, string to, string body)
        {
            return _internalClient.SendSmsMessage(from, to, body);
        }

        public void Initialize(string accountSID, string accountAuthKey)
        {
            _internalClient = new TwilioRestClient(accountSID, accountAuthKey);
        }
    }
}
