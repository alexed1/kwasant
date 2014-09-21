using System;
using System.Collections.Generic;
using System.Net.Mail;
using Data.Entities;

namespace Data.Interfaces
{
    public interface IEmailAddressDO
    {
        String Name { get; set; }
        String Address { get; set; }
    }

    public interface IEmailAddress
    {
        EmailAddressDO ConvertFromMailAddress(IUnitOfWork uow, MailAddress address);
        List<ParsedEmailAddress> ExtractFromString(String textToSearch);
        IEnumerable<EmailAddressDO> FilterOutDomains(IEnumerable<EmailAddressDO> addressList, string domain);
    }
}
