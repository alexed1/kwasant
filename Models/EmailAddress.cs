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
        string EmailAddressBody { get; set; }
    }

    public class EmailAddress : IEmailAddress
    {

        public int Id {get; set; }
        public string DisplayName {get; set;}
        public string EmailAddressBody { get; set; }


        public EmailAddress(MailAddress importedAddress)
        {
            DisplayName = importedAddress.DisplayName;
            EmailAddressBody = importedAddress.Address;

        }

        public EmailAddress()
        {
            
        }
      
 
    }
}