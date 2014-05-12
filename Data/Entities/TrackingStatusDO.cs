using System;
using Data.Entities.Enumerations;
using Data.Interfaces;

namespace Data.Entities
{
    public class TrackingStatusDO : ICustomField
    {
        public int ForeignTableID { get; set; }
        public string ForeignTableName { get; set; }
        public TrackingStatus Status { get; set; }
    }
}
