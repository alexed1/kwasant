using Data.Entities;
using Data.Entities.Enumerations;
using Data.Repositories;

namespace KwasantTest.Fixtures
{
    partial class FixtureData
    {




   
        public EmailDO TestEmail1()
        {

            EmailDO curEmailDO = new EmailDO();
            curEmailDO.Id = 1;
            curEmailDO.AddEmailParticipant(EmailParticipantType.FROM, TestEmailAddress1());
            curEmailDO.AddEmailParticipant(EmailParticipantType.TO, TestEmailAddress2());
            curEmailDO.Subject = "Main Subject";
            curEmailDO.Text = "This is the Body Text";
            return curEmailDO;

        }

       

    }
}

