namespace Data.DDay.DDay.iCal.Interfaces.DataTypes
{
    public interface IEncodableDataType :
        ICalendarDataType
    {
        string Encoding { get; set; }
    }
}
