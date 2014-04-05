using System.Net.Mail;
using Data.DataAccessLayer.Repositories;
using Newtonsoft.Json;

namespace Data.Models
{
    public interface IEmailAddress
    {
        [JsonIgnore]
        int Id { get; set; }
        string Name { get; set; }
        string Email { get; set; }
    }

    public class EmailAddress : IEmailAddress
    {

        public int Id {get; set; }
        public string Name {get; set;}
        public string Email { get; set; }

        private IEmailAddressRepository _emailAddressRepo;

        //This is the constructor when building an EmailAddress from an imported IMAP address (the library we're using uses MailAddress)
        public EmailAddress(MailAddress importedAddress)
        {
            Name = importedAddress.DisplayName;
            Email = importedAddress.Address;

        }

        //This is the constructor when building an EmailAddress from our own Db
        public EmailAddress(IEmailAddressRepository emailAddressRepo)
        {
            _emailAddressRepo = emailAddressRepo;

        }

        public EmailAddress()
        {
            
        }
      
 
    }
}