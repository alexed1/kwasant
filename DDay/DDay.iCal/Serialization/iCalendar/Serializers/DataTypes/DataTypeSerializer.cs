using System;
using Data.DDay.DDay.iCal.Interfaces.DataTypes;
using Data.DDay.DDay.iCal.Interfaces.General;
using Data.DDay.DDay.iCal.Interfaces.Serialization;

namespace Data.DDay.DDay.iCal.Serialization.iCalendar.Serializers.DataTypes
{
    public abstract class DataTypeSerializer :
        SerializerBase
    {
        #region Constructors

        public DataTypeSerializer()
        {
        }

        public DataTypeSerializer(ISerializationContext ctx) : base(ctx)
        {
        }

        #endregion        

        #region Protected Methods

        virtual protected ICalendarDataType CreateAndAssociate()
        {
            // Create an instance of the object
            ICalendarDataType dt = Activator.CreateInstance(TargetType) as ICalendarDataType;
            if (dt != null)
            {
                ICalendarObject associatedObject = SerializationContext.Peek() as ICalendarObject;
                if (associatedObject != null)
                    dt.AssociatedObject = associatedObject;
                
                return dt;
            }
            return null;
        }

        #endregion
    }
}
