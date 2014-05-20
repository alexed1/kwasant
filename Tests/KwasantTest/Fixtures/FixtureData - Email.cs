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
            curEmailDO.EmailID = 1;
            curEmailDO.AddEmailParticipant(EmailParticipantType.FROM,new EmailEmailAddressRepository(_uow), TestEmailAddress1());
            curEmailDO.AddEmailParticipant(EmailParticipantType.TO, new EmailEmailAddressRepository(_uow), TestEmailAddress2());
            curEmailDO.Subject = "Main Subject";
            curEmailDO.Text = "This is the Body Text";
            return curEmailDO;

        }

       

    }
}

