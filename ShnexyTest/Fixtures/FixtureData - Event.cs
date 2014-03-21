using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shnexy.Models;
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
                DTStart = new iCalDateTime("20040117"),
                DTEnd = new iCalDateTime("20040110"),
                Location = "San Francisco",
                Description = "First Ever Event",
                WorkflowState = "Undispatched"

              //   DateTimeSerializer serializer = new DateTimeSerializer();
            //CopyFrom(serializer.Deserialize(new StringReader(value)) as ICopyable);

            };
        }



    }
}
