using System.Linq;
using Data.Entities;
using FluentValidation;

namespace Data.Validators
{
    public abstract class OutboundEmailValidatorBase : AbstractValidator<EmailDO>
    {
        private readonly EmailAddressValidator _emailAddressValidator;

        protected OutboundEmailValidatorBase()
        {
            _emailAddressValidator = new EmailAddressValidator();

            RuleFor(e => e.Subject).NotEmpty().Length(5, int.MaxValue).WithMessage("Email subject must be at least 5 characters long.");
            RuleFor(e => e.To).Must(to => to.Any()).WithMessage("Email must have at least one TO recipient.");
            RuleFor(e => e.DateCreated).NotEmpty().WithMessage("Email must have a non-null DateCreated."); ;
            RuleFor(e => e.From).NotEmpty().SetValidator(_emailAddressValidator).WithMessage("Email From validation failed.");
        }
    }
}
