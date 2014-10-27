using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Data.Entities
{

    //Kact stands for a Kwasant Action. It's a loggable event that we'll record for our Business Intelligence activities
    //We'll switch to a better name when we think of one, but Event, Action, Activity are all taken in one way or another...
    public class FactDO
    {
        public FactDO()
        {
            var t = 0;//For debug breakpoint purposes
        }
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public string PrimaryCategory { get; set; }
        public string SecondaryCategory { get; set; }
        public string Activity { get; set; }
        //the ID of the object that is the subject of this Kact
        public int ObjectId { get; set; }
        //intended to be used to group related Kacts
        public int TaskId  { get; set; }

        //ASP Identity GUID. Ugly.
        public string CustomerId { get; set; }
        public string BookerId { get; set; }
        public int AdminId { get; set; }
        public string Data { get; set; }

        [ForeignKey("CreatedBy")]
        public string CreatedByID { get; set; }
        public virtual UserDO CreatedBy { get; set; }

        public string Status { get; set; }
        public DateTimeOffset CreateDate { get; set; }
    }
}
