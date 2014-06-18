using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Data.Entities;
using FluentValidation;
using FluentValidation.Results;
using FluentValidation.Validators;
using Data.Validators;

namespace Data.Validations
{
    public class MeetingInformationValidator : AbstractValidator<MeetingInformationDO>
    {
       public MeetingInformationValidator()
       {
           RuleFor(x => x.message).SetValidator(new StringMinLength(30))
               .WithMessage("Meeting information must have at least 30 characters");
       }
    }
}
