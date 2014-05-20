using System;
using System.ComponentModel.DataAnnotations;
using Data.Entities;

namespace Data.Interfaces
{
    public interface IEvent
    {
        [Key]
        int Id { get; set; }

        string Summary { get; set; }
        string Location { get; set; }
        DateTime StartDate { get; set; }
        DateTime EndDate { get; set; }
        UserDO CreatedBy { get; set; }
    }
}