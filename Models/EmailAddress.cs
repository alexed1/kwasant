using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Web;

namespace Shnexy.Models
{
    public interface IEmailAddress
    {
        int Id { get; set; }
        string Name { get; set; }
        string Email { get; set; }
    }

    public class EmailAddress : IEmailAddress
    {

        public int Id {get; set; }
        public string Name {get; set;}
        public string Email { get; set; }


        public EmailAddress(MailAddress importedAddress)
        {
            Name = importedAddress.DisplayName;
            Email = importedAddress.Address;

        }

        public EmailAddress()
        {
            
        }
      
 
    }
}