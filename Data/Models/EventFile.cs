using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Shnexy.Models
{
    //
    public class EventFile
    {

        public int Id { get; set; }
        public string Body { get; set; }

        //Attendees should be encoded into the ICS in accordance with the standard. If that were done properly, they would be in the Body, above, with the rest of the event. 
        //However, Attendee support is actually not yet implemented in Dday. For the moment, we will store Attendees as a separate property, here:
        //we're going to have to implement this pretty quickly, though. 
        public ICollection<EmailAddress> Attendees { get; set; } 
    }
}