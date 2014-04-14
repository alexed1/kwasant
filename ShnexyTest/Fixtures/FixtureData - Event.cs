using Data.DDay.DDay.iCal.DataTypes;
using Data.Models;
using DDay.DDay.iCal.Components;

namespace ShnexyTest.Fixtures
{
    partial class FixtureData
    {

        public  DDayEvent TestEvent()
        {
            return new DDayEvent()
            {
                
                //DTStart = (iCalDateTime)DateTime.Parse("20040117"),
                DTStart = new iCalDateTime("20140517"),
                DTEnd = new iCalDateTime("20140610"),
                Location = "San Francisco",
                Description = "First Ever Event",
                Summary = "Here's a Summary",
                WorkflowState = "Undispatched",
                
              //   DateTimeSerializer serializer = new DateTimeSerializer();
            //CopyFrom(serializer.Deserialize(new StringReader(value)) as ICopyable);

            };
        }



    }
}
