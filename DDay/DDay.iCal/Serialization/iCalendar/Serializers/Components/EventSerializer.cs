using System;
using Data.DDay.DDay.iCal.Interfaces.Serialization;
using Data.Models;

namespace Data.DDay.DDay.iCal.Serialization.iCalendar.Serializers.Components
{
    public class EventSerializer :
        UniqueComponentSerializer
    {
        #region Constructor

        public EventSerializer() : base()
        {
        }

        public EventSerializer(ISerializationContext ctx)
            : base(ctx)
        {
        }

        #endregion

        #region Overrides

        public override Type TargetType
        {
            get { return typeof(Event); }
        }

        public override string SerializeToString(object obj)
        {
            if (obj is Event)
            {
                Event evt = ((Event)obj).Copy<Event>();

                // NOTE: DURATION and DTEND cannot co-exist on an event.
                // Some systems do not support DURATION, so we serialize
                // all events using DTEND instead.
                if (evt.Properties.ContainsKey("DURATION") && evt.Properties.ContainsKey("DTEND"))
                    evt.Properties.Remove("DURATION");

                return base.SerializeToString(evt);
            }
            return base.SerializeToString(obj);
        }

        #endregion
    }
}
