﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Data.Entities;
using Data.Interfaces;
using System.Net.Mail;
namespace KwasantCore.Services
{
    public class EmailAddress : IEmailAddress
    {
       public EmailAddressDO ConvertFromMailAddress(IUnitOfWork uow, MailAddress address)
       {
           return uow.EmailAddressRepository.GetOrCreateEmailAddress(address.Address, address.DisplayName);
       }

       public List<ParsedEmailAddress> ExtractFromString(String textToSearch)
       {
           if (String.IsNullOrEmpty(textToSearch))
               return new List<ParsedEmailAddress>();
           //This is the email regex.
           //It searches for emails in the format of <Some Person>somePerson@someDomain.someExtension

           //We assume that names can only contain letters, numbers, and spaces. We also allow for a blank name, in the form of <>
           const string nameRegex = @"[ a-zA-Z0-9]*";

           //We assume for now, that emails can only contain letters and numbers. This can be updated in the future (parsing emails is actually incredibly difficult).
           //See http://tools.ietf.org/html/rfc2822#section-3.4.1 in the future if we ever update this.
           const string emailUserNameRegex = @"[a-zA-Z0-9]*";

           //Domains can only contain letters or numbers.
           const string domainRegex = @"[a-zA-Z0-9]+";

           //Top level domain must be at least two characters long. Only allows letters, numbers or dashes.
           const string tldRegex = @"[a-zA-Z0-9\-]{2,}";

           //The name part is optional; we can find emails like 'rjrudman@gmail.com', or '<Robert Rudman>rjrudman@gmail.com'.
           //The regex uses named groups; 'name' and 'email'.
           //Name will contain the name, without <>. Email will contain the full email address (without the name).

           //Typically, you won't need to modify the below code, only the four variables defined above.
           var fullRegexExpression = String.Format(@"(<(?<name>{0})>)?(?<email>{1}@{2}\.{3})", nameRegex, emailUserNameRegex, domainRegex, tldRegex);

           var regex = new Regex(fullRegexExpression);

           var result = new List<ParsedEmailAddress>();
           foreach (Match match in regex.Matches(textToSearch))
           {
               var parse = new ParsedEmailAddress
               {
                   Name = match.Groups["name"].Value,
                   Email = match.Groups["email"].Value
               };
               result.Add(parse);
           }
           return result;
       }

        public List<EmailAddressDO> GetEmailAddresses(IUnitOfWork uow, params string[] textToSearch)
        {
            var emailAddresses = textToSearch.SelectMany(ExtractFromString);
            
            var uniqueEmails = emailAddresses.GroupBy(ea => ea.Email.ToLower()).Select(g =>
            {
                var potentialFirst = g.FirstOrDefault(e => !String.IsNullOrEmpty(e.Name)) ?? g.First();
                return potentialFirst;
            });

            var addressList =
                FilterOutDomains(uniqueEmails, "sant.com")
                    .Select(parsedEmailAddress =>
                        uow.EmailAddressRepository.GetOrCreateEmailAddress(parsedEmailAddress.Email, parsedEmailAddress.Name)
                    );
            
            return addressList.ToList();
        }

        public IEnumerable<ParsedEmailAddress> FilterOutDomains(IEnumerable<ParsedEmailAddress> addressList, params string[] domains)
        {
            return addressList.Where(a => domains.All(domain => !a.Email.EndsWith(domain)));
        }

        public EmailAddressDO ConvertFromString(string emailString, IUnitOfWork uow)
        {
            String email = string.Empty;
            String name = string.Empty;
            emailString = emailString.Replace("\"", string.Empty);
            if (emailString.Contains("<"))
            {
                string[] parts = emailString.Split('<');
                name = parts[0];
                email = parts[1];
                email = email.Replace(">", string.Empty);
            }
            else
                email = emailString;

            EmailAddressDO convertAddresFromString = uow.EmailAddressRepository.GetOrCreateEmailAddress(email, name);
            return convertAddresFromString;
        }
    }
}
