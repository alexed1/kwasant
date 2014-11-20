using System;
using System.ComponentModel.DataAnnotations;
namespace Data.Entities
{
    public class IncidentDO : BaseDO
    {
        public IncidentDO()
        {
            Priority = 1;
        }

        [Key]
        public int Id { get; set; }
        public String PrimaryCategory { get; set; }
        public String SecondaryCategory { get; set; }
        public String Activity { get; set; }
        public int Priority { get; set; }
        public string Notes { get; set; }
        public int ObjectId { get; set; }
        public string CustomerId { get; set; }
        public string BookerId { get; set; } 

    }
}
