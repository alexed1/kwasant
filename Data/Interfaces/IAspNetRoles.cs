using System;
using System.ComponentModel.DataAnnotations;
using Data.Entities;

namespace Data.Interfaces
{
    public interface IAspNetRoles
    {
        String Id { get; set; }
        String Name { get; set; }        
    }
}
        
