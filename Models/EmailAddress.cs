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
        string DisplayName { get; set; }
        string Address { get; set; }
    }

    public class EmailAddress : IEmailAddress
    {

        public int Id {get; set; }
        public string DisplayName {get; set;}
        public string Address { get; set; }


        public EmailAddress(MailAddress importedAddress)
        {
            DisplayName = importedAddress.DisplayName;
            Address = importedAddress.Address;

        }

      
 
    }
}