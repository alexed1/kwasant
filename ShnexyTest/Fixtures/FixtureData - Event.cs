using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Data.DDay.DDay.iCal.DataTypes;
using Data.Models;
using Shnexy.DDay.iCal;

namespace ShnexyTest.Fixtures
{
    partial class FixtureData
    {

        public  Event TestEvent()
        {
            return new Event(_uow)
            {
                Id = 1,
                CustomerId = 1,
                //DTStart = (iCalDateTime)DateTime.Parse("20040117"),
                DTStart = new iCalDateTime("20140517"),
                DTEnd = new iCalDateTime("20140610"),
                Location = "San Francisco",
                Description = "First Ever Event",
                Summary = "Here's a Summary",
                WorkflowState = "Undispatched",
                Attendees = new List<EmailAddress> {
                    TestEmail1(),
                    TestEmail2()
                 }

              //   DateTimeSerializer serializer = new DateTimeSerializer();
            //CopyFrom(serializer.Deserialize(new StringReader(value)) as ICopyable);

            };
        }



    }
}
