using System;
using System.IO;
using Data.DDay.DDay.iCal.DataTypes;
using Data.DDay.DDay.iCal.Interfaces.General;
using Data.DDay.DDay.iCal.Serialization.iCalendar.Serializers.DataTypes;

namespace Data.DDay.DDay.iCal.Serialization.iCalendar.Serializers.Other
{
    public class IntegerSerializer :
        EncodableDataTypeSerializer
    {
        public override Type TargetType
        {
            get { return typeof(int); }
        }

        public override string SerializeToString(object integer)
        {
            try
            {
                int i = Convert.ToInt32(integer);

                ICalendarObject obj = SerializationContext.Peek() as ICalendarObject;
                if (obj != null)
                {
                    // Encode the value as needed.
                    EncodableDataType dt = new EncodableDataType();
                    dt.AssociatedObject = obj;
                    return Encode(dt, i.ToString());
                }
                return i.ToString();
            }
            catch
            {
                return null;
            }
        }

        public override object Deserialize(TextReader tr)
        {
            string value = tr.ReadToEnd();

            try
            {
                ICalendarObject obj = SerializationContext.Peek() as ICalendarObject;
                if (obj != null)
                {
                    // Decode the value, if necessary!
                    EncodableDataType dt = new EncodableDataType();
                    dt.AssociatedObject = obj;
                    value = Decode(dt, value);
                }

                int i;
                if (Int32.TryParse(value, out i))
                    return i;
            }
            catch {}

            return value;
        }
    }
}
