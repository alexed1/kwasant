using System;
using Data.Interfaces;

namespace Data.Entities
{
    public class TrackingStatusDO : ICustomField<String>
    {
        public int ForeignTableID { get; set; }
        public string ForeignTableName { get; set; }
        public string Value { get; set; }
    }
}
