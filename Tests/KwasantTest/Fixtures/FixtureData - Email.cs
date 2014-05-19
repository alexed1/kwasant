using Data.Entities;

namespace KwasantTest.Fixtures
{
    partial class FixtureData
    {




   
        public EmailDO TestEmail1()
        {

            EmailDO curEmailDO = new EmailDO();
            curEmailDO.EmailID = 1;
            curEmailDO.From = TestEmailAddress1();
            curEmailDO.To.Add(TestEmailAddress2());
            curEmailDO.Subject = "Main Subject";
            curEmailDO.HTMLText = "This is the Body Text";
            return curEmailDO;

        }

       

    }
}

