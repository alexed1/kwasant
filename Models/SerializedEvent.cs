using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

//the DDay Event class is rich but has not been designed to be persistable. The underlying logic is that you create an ICS and you're done. 
//So we'll use string ICS files as the storage mechanism, in a sort of No SQL way
//An ICS is a container for all sorts of things, but we're going to constrain its use in this class to only hold a single Event
using Shnexy.DDay.iCal;
using Shnexy.DDay.iCal.Serialization.iCalendar;

namespace Shnexy.Models
{
    public class SerializedEvent : ISerializedEvent
    {

        private int Id { get; set; }
        private string _body { get; set; }

        public SerializedEvent(Event curEvent)
        {

            //_body = Serialize(curEvent);
            iCalendar iCal = new iCalendar();      
            iCal.AddChild(curEvent);
            iCalendarSerializer serializer = new iCalendarSerializer(iCal);
            serializer.Serialize(@"filename.ics");
        }

        //maps the body into an Event object
        public Event Deserialize()
        {
            Event curEvent = new Event();
            //implement
            return curEvent;

        }

        //maps an Event object into this.
        public void Serialize(Event curEvent)
        {
            //this.Body = curEvent.foo;
            //implement 



        }

    }


}