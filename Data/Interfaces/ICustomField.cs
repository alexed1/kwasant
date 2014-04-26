namespace Data.Interfaces
{
    public interface ICustomField<TValueType>
    {
        int ForeignTableID { get; set; }
        string ForeignTableName { get; set; }
        TValueType Value { get; set; }
    }
}
