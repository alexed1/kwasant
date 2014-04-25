using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Data.Entities
{
    public class TrackingStatusDO
    {
        //[Key]
        //public int TrackingStatusID { get; set; }

        //[Key][Column(Order=0)]
        public int ForeignTableID { get; set; }
        //[Key][Column(Order=1)]
        public string ForeignTableName { get; set; }

        public string Value { get; set; }
    }
}
