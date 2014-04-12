namespace Data.DDay.DDay.iCal.Interfaces.DataTypes
{
    public interface IFreeBusyEntry :
        IPeriod
    {
        FreeBusyStatus Status { get; set; }
    }
}
