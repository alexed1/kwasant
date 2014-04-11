namespace Data.DDay.DDay.iCal.Interfaces.Components
{
    public interface IJournal :
        IRecurringComponent
    {
        JournalStatus Status { get; set; }
    }
}
