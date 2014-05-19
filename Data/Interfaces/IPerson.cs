using System;
using System.Web;
using System.ComponentModel;
using Data.Entities;

namespace Data.Interfaces
{
    public interface IPerson
    {
        int Id { get; set; }
        EmailAddressDO EmailAddress { get; set; }
    }
}

