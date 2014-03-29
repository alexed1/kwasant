using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Linq;
using System.Net.Mail;
using System.Web;
using System.Windows.Forms;
using Shnexy.DataAccessLayer.Repositories;

namespace Shnexy.Models
{

    ///// <summary>
    /////This class is structured to facilitate auto-serialization of data that gets sent using the Mandrill API. It is similar to our main Email class
    ///// We should probably get rid of it, and write manual serialization code
    ///// Another open issue is: should the EmailAddress be done as an array (like here) or as a Collection. The array is probably more cross-platform
    ///// </summary>
    //[Serializable]
    //public class Email
    //{

    //    public List<EmailTemplateMergeRecipient> MergeVars;
    //    public Email()
    //    {
    //        MergeVars = new List<EmailTemplateMergeRecipient> { };
    //    }
    //}

    public class Email : IEmail
    {
        [Key]
        public int Id { get; set; }

        public string Html { get; set; }
        public string Text { get; set; }

        public string Subject { get; set; }
        public string FromEmail { get; set; }
        public string FromName { get; set; }
        

        public List<EmailAddress> To { get; set; }

        public List<EmailAddress> CC { get; set; }

        public List<EmailAddress> Bcc { get; set; }

        public List<Attachment> Attachments { get; set; } 

        public string Status { get; set; } //TODO replace with typesafe enum

        private EmailAddress curAddress ;
        private IEmailRepository _emailRepo;

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

        public void Configure(EmailAddress destEmailAddress, int eventId, string filename)
        {
            To.Add(destEmailAddress);
            //TODO tag the email with the eventId
            
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