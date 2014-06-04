using System;
using System.ComponentModel.DataAnnotations;

namespace Data.Entities
{

    //Kact stands for a Kwasant Action. It's a loggable event that we'll record for our Business Intelligence activities
    //We'll switch to a better name when we think of one, but Event, Action, Activity are all taken in one way or another...
    public class Kact
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public string PrimaryCategory { get; set; }
        public string SecondaryCategory { get; set; }
        public int TaskId  { get; set; }
        public int CustomerId { get; set; }
        public int BookerId { get; set; }
        public int AdminId { get; set; }
        public int Data { get; set; }

        public string Status { get; set; }
        public DateTime CreateDate { get; set; }
    }
}
