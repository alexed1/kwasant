using System;
using System.ComponentModel.DataAnnotations;

namespace Data.Entities
{

    //Kact stands for a Kwasant Action. It's a loggable event that we'll record for our Business Intelligence activities
    //We'll switch to a better name when we think of one, but Event, Action, Activity are all taken in one way or another...
    public class KactDO
    {
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
        public int BookerId { get; set; }
        public int AdminId { get; set; }
        public string Data { get; set; }

        public string Status { get; set; }
        public DateTime CreateDate { get; set; }
    }
}
