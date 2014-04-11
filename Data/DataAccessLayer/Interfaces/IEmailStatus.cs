using System;
using System.ComponentModel.DataAnnotations;

namespace Data.DataAccessLayer.Interfaces
{
    public interface IEmailStatus
    {
        [Key]
        int EmailStatusID { get; set; }

        String Value { get; set; }
    }
}