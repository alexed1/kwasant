using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

using Data.Entities;
using FluentValidation;

namespace Data.Validators
{
    public class EmailAddressValidator : AbstractValidator<EmailAddressDO>
    {
        public EmailAddressValidator()
        {
            RuleFor(obj => obj.Address)
                .EmailAddress()
                .WithMessage("Email Address objects require a legitimate email address in their address field. ");
        }
    }

    public class EmailAddressStringValidator : AbstractValidator<string>
    {
        public EmailAddressStringValidator()
        {
            RuleFor(obj => obj)
                .EmailAddress()
                .WithMessage("Email Address objects require a legitimate email address in their address field. ");
        }
    }

    public static class StringExtension
    {
        //validate that this string is a properly formed email address
        public static Boolean isEmailAddress(this string value)
        {
            var curEmailAddressStringValidator = new EmailAddressStringValidator();
            curEmailAddressStringValidator.ValidateAndThrow(value);
            return true;
        }
    }
}
