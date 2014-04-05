using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Net.Mail;
using Data.DataAccessLayer.Interfaces;
using Data.DataAccessLayer.Repositories;
using Data.Services.APIManager.Packagers.Mandrill;
using Data.Services.EmailManager;
using Newtonsoft.Json;
using StructureMap;

namespace Data.Models
{

    ///// <summary>
    /////This class is structured to facilitate auto-serialization of data that gets sent using the Mandrill API. It is similar to our main Email class
    ///// We should probably get rid of it, and write manual serialization code
    ///// Another open issue is: should the EmailAddress be done as an array (like here) or as a Collection. The array is probably more cross-platform
    ///// </summary>
    //[Serializable]
    //public class Email
    //{

    //    public List<MandrillMergeRecipient> MergeVars;
    //    public Email()
    //    {
    //        MergeVars = new List<MandrillMergeRecipient> { };
    //    }
    //}

    public class Email : IEmail
    {
        [Key]
        [JsonIgnore]
        public int Id { get; set; }

        public string Html { get; set; }
        public string Text { get; set; }

        public string Subject { get; set; }
        public string FromEmail { get; set; }
        public string FromName { get; set; }
        

        public List<EmailAddress> To { get; set; }

        public ICollection<EmailAddress> CC { get; set; }

        public ICollection<EmailAddress> Bcc { get; set; }

        public ICollection<Attachment> Attachments { get; set; } 

        public string Status { get; set; } //TODO replace with typesafe enum

        private EmailAddress curAddress ;
        private IEmailRepository _emailRepo;
        private ICustomerRepository _customerRepo;
        private IUnitOfWork _uow;
        private MandrillPackager MandrillAPI;
        private EmailManager _emailManager;

        public Email()
        {
            
        }
        public Email(IEmailRepository emailRepo)
        {
            _emailRepo = emailRepo;
            To = new List<EmailAddress>();
            CC = new List<EmailAddress>();
            Bcc = new List<EmailAddress>();
            Attachments = new List<Attachment>();
            _uow = ObjectFactory.GetInstance<IUnitOfWork>();
            _customerRepo = new CustomerRepository(_uow);
            MandrillAPI = new MandrillPackager();
            _emailManager = new EmailManager();
        }

        public Email(MailMessage curMessage, IEmailRepository emailRepo)
        {
            Text = curMessage.Body;
            Subject = curMessage.Subject;
            FromEmail = MapAddress(curMessage.From).Email;

            To = new List<EmailAddress>();
            CC = new List<EmailAddress>();
            Bcc = new List<EmailAddress>();
        
            foreach (var address in curMessage.To)
            {
                
                To.Add(MapAddress(address));
                     //ugly This maps the MSDN address to our normalized EmailAddress
            }
            foreach (var address in curMessage.CC)
            {
                CC.Add(MapAddress(address));
                 //ugly This maps the MSDN address to our normalized EmailAddress
            }
            foreach (var address in curMessage.Bcc)
            {
                
                Bcc.Add(MapAddress(address)); //ugly This maps the MSDN address to our normalized EmailAddress
            }
            Status = "Unprocessed";
            _emailRepo = emailRepo;
        }

        public EmailAddress MapAddress(MailAddress importedAddress)
        {
              return new EmailAddress(importedAddress);
        }

        public void Save()
        {
            _emailRepo.Add(this);
            _emailRepo.UnitOfWork.SaveChanges(); 
        }

        //Configure the outbound email for a specified Event
        public void Configure(Event curEvent)
        {
            //Get Customer using CustomerId. retrieve the email target address
            Customer curCustomer = new Customer(_customerRepo);
            curCustomer = curCustomer.GetByKey(curEvent.CustomerId);

            FromEmail = "lucreorganizer@gmail.com";
            FromName = "Booqit Organizer";
            Text = "This is a Booqit Event Request. For more information, see https://foo.com";
            Html = "This is a Booqit Event Request. For more information, see https://foo.com";
            Subject = "Invitation via Booqit: " + curEvent.Summary + "@ " + curEvent.DTStart;

            foreach (var attendee in curEvent.Attendees)
            {
                To.Add(attendee);
            }

            Attachment icsFile = new Attachment();
            icsFile.Name = "invite.ics";
            icsFile.Type = "application/ics";
            var file = File.ReadAllBytes(curEvent.ICSFilename);
            var base64Version = Convert.ToBase64String(file, 0, file.Length);
            icsFile.Content = base64Version;
            Attachments.Add(icsFile);
        }

        public void Send()
        {

            _emailManager.Send(this);
            
           
        
        }



        public IEnumerable<Email> GetAll()
        {
            return _emailRepo.GetAll();
            
        }

        public Email GetByKey(int Id)
        {
            return _emailRepo.GetByKey(Id);

        }

    }
}