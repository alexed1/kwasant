using System;
using Data.Entities;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Data.Interfaces
{
    public interface IPerson
    {
        [Key]
        int PersonId { get; set; }       

        String FirstName { get; set; }
        String LastName { get; set; }

        int? EmailAddressID { get; set; }

        EmailAddressDO EmailAddress { get; set; }
        
    }
}