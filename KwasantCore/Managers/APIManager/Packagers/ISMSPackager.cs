using System;
using Twilio;

namespace KwasantCore.Managers.APIManager.Packagers
{
    public interface ISMSPackager
    {
        SMSMessage SendSMS(String number, String message);
    }
}