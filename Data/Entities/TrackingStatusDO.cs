using System.ComponentModel.DataAnnotations.Schema;
using Data.Entities.Constants;
using Data.Interfaces;

namespace Data.Entities
{
    public class TrackingStatusDO : ICustomField
    {
        public int Id { get; set; }
        public string ForeignTableName { get; set; }

        [ForeignKey("TrackingType")]
        public int TrackingTypeID { get; set; }
        public TrackingTypeRow TrackingType { get; set; }

        [ForeignKey("TrackingStatus")]
        public int TrackingStatusID { get; set; }
        public TrackingStatusRow TrackingStatus { get; set; }
    }
}
