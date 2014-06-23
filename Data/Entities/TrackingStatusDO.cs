using Data.Entities.Enumerations;
using Data.Interfaces;

namespace Data.Entities
{
    public class TrackingStatusDO : ICustomField
    {
        public int Id { get; set; }
        public string ForeignTableName { get; set; }

        public TrackingType Type { get; set; }
        public TrackingStatus Status { get; set; }


    }
}
