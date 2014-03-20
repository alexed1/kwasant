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

        public static Event TestEvents()
        {
            return new Event
            {
                Id = 1,
                DTStart = (iCalDateTime)DateTime.Parse("20040117"),
                DTEnd = (iCalDateTime)DateTime.Parse("20040110"),
                Location = "San Francisco",
                Description = "First Ever Event",
                WorkflowState = "Undispatched"

            };
        }



    }
}
