namespace Data.DDay.DDay.iCal.Interfaces.DataTypes
{
    public interface IGeographicLocation :
        IEncodableDataType
    {
        double Latitude { get; set; }
        double Longitude { get; set; }
    }
}
