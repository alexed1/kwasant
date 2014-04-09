using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Net.Mime;
using System.Text;
using Data.Constants;
using Data.DataAccessLayer.Interfaces;
using Data.DataAccessLayer.Repositories;
using Data.DDay.DDay.iCal;
using Data.DDay.DDay.iCal.ExtensionMethods;
using Data.DDay.DDay.iCal.Serialization.iCalendar.Serializers;
using Data.Models;
using DBTools.Managers;
using StructureMap;
using Attachment = Data.Models.Attachment;


namespace DBTools
{
    public static class EmailHelper
    {
        public static void AttachInvitationToEmail(Event ev, Email email)
        {
            iCalendar iCal = new iCalendar();
            iCal.AddChild(ev);

            iCalendarSerializer serializer = new iCalendarSerializer(iCal);
            var fileToAttach = serializer.Serialize(iCal);

            var attachment = CreateNewAttachment(
                new System.Net.Mail.Attachment(
                    new MemoryStream(Encoding.UTF8.GetBytes(fileToAttach)),
                    new ContentType {MediaType = "application/calendar", Name = "invite.ics"}
                ));

            email.Attachments.Add(attachment);
        }

        public static Email AddNewEmailToRepository(IEmailRepository emailRepository, MailMessage mailAddress)
        {
            var uow = emailRepository.UnitOfWork;
            var email = new Email
            {
                From = GetOrCreateEmailAddress(uow, mailAddress.From),
                BCC = mailAddress.Bcc.Select(a => GetOrCreateEmailAddress(uow, a)).ToList(),
                CC = mailAddress.CC.Select(a => GetOrCreateEmailAddress(uow, a)).ToList(),
                Subject = mailAddress.Subject,
                Text = mailAddress.Body,
                Attachments = mailAddress.Attachments.Select(CreateNewAttachment).ToList(),
                To = mailAddress.To.Select(a => GetOrCreateEmailAddress(uow, a)).ToList(),
                Invitation = null
            };
            email.To.ForEach(a => a.ToEmail = email);
            email.CC.ForEach(a => a.BCCEmail = email);
            email.BCC.ForEach(a => a.CCEmail = email);

            emailRepository.Add(email);
            return email;
        }

        private static EmailAddress GetOrCreateEmailAddress(IUnitOfWork uow, MailAddress address)
        {
            var emailAddressRepo = new EmailAddressRepository(uow);
            var existingEmailAddress = emailAddressRepo.GetQuery().Where(ea => ea.Address == address.Address);
            if (existingEmailAddress.Any())
                return existingEmailAddress.First();

            return new EmailAddress { Address = address.Address, Name = address.DisplayName };
        }

        private static Attachment CreateNewAttachment(System.Net.Mail.Attachment attachment)
        {
            var fileName = Path.GetFullPath(Path.GetRandomFileName());
            var fileStream = File.Open(fileName, FileMode.OpenOrCreate);
            attachment.ContentStream.CopyTo(fileStream);
            fileStream.Close();

            return new Attachment
            {
                FileLocation = fileName,
                Name = attachment.Name,
                Type = attachment.ContentType.MediaType
            };
        }

        public static void SendEmail(Email email)
        {
            new EmailManager().Send(email);
            var uow = ObjectFactory.GetInstance<IUnitOfWork>();
            var er = new EmailRepository(uow);

            var originalEmail = er.GetByKey(email.EmailID);
            email.Status = EmailStatusConstants.GetStatusRow(EmailStatusConstants.SENT);
            er.Update(email, originalEmail);
            uow.SaveChanges();
        }
    }
}
