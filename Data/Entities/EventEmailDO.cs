using System.ComponentModel.DataAnnotations;

namespace Data.Entities
{
    public class EventEmailDO
    {
        [Key]
        public int EventID { get; set; }
        [Key]
        public int EmailID { get; set; }
    }
}
