using Data.Constants;
using Data.Entities;
using Data.Entities.Constants;

namespace KwasantTest.Fixtures
{
    partial class FixtureData
    {

        public EmailDO TestEmail1()
        {

            EmailDO curEmailDO = new EmailDO();
            curEmailDO.Id = 1;
            curEmailDO.From = TestEmailAddress1();
            curEmailDO.AddEmailRecipient(EmailParticipantType.To, TestEmailAddress2());
            curEmailDO.Subject = "Main Subject";
            curEmailDO.HTMLText = "This is the Body Text";
            return curEmailDO;

        }

       

    }
}

