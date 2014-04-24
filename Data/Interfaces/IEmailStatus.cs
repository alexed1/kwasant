using System;
using System.ComponentModel.DataAnnotations;

namespace Data.Interfaces
{
    public interface IEmailStatus
    {
        [Key]
        int EmailStatusID { get; set; }

        String Value { get; set; }
    }
}