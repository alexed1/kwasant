namespace Data.Interfaces
{
    public interface ICustomField
    {
        int ForeignTableID { get; set; }
        string ForeignTableName { get; set; }
    }
}
