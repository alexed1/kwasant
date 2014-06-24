using System;
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
                .WithMessage("You need to provide a valid Email Address. ");
        }
    }

    public class EmailAddressStringValidator : AbstractValidator<EmailAddressStringValidator.EmailAddressWrapper>
    {
        public EmailAddressStringValidator()
        {
            RuleFor(obj => obj.Address)
                .EmailAddress()
                .WithMessage("Email Address objects require a legitimate email address in their address field. ");
        }

        public class EmailAddressWrapper
        {
            public String Address { get; set; }

            public EmailAddressWrapper(String address)
            {
                Address = address;
            }
        }

    }

    public static class StringExtension
    {
        //validate that this string is a properly formed email address
        public static Boolean IsEmailAddress(this string value)
        {
            if (String.IsNullOrEmpty(value))
                value = String.Empty;

            var curEmailAddressStringValidator = new EmailAddressStringValidator();
            curEmailAddressStringValidator.ValidateAndThrow(new EmailAddressStringValidator.EmailAddressWrapper(value));
            return true;
        }
    }
}
