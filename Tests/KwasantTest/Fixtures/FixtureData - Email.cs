using Data.Entities;
using Data.States;

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

        public EmailDO TestEmail3()
        {

            EmailDO curEmailDO = new EmailDO();
            curEmailDO.Id = 3;
            curEmailDO.From = TestEmailAddress3();
            curEmailDO.AddEmailRecipient(EmailParticipantType.To, TestEmailAddress3());
            curEmailDO.Subject = "Main Subject";
            curEmailDO.HTMLText = "This Email is intended to be used with KwasantIntegration account ";
            return curEmailDO;

        }

    }
}

