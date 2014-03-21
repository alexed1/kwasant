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
            return new Event
            {
                Id = 1,
                CustomerId = 1,
                DTStart = (iCalDateTime)DateTime.Parse("20040117"),
                DTEnd = (iCalDateTime)DateTime.Parse("20040110"),
                Location = "San Francisco",
                Description = "First Ever Event",
                WorkflowState = "Undispatched"

            };
        }



    }
}
