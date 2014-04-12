using System;
using System.ComponentModel.DataAnnotations;

namespace Data.DataAccessLayer.Interfaces
{
    interface IEmailAddress
    {
        String Name { get; set; }
        String Address { get; set; }
    }
}
