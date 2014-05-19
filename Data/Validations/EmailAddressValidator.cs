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
  public class EmailAddressValidator: AbstractValidator<EmailAddressDO>
    {
      public EmailAddressValidator()
      {
          RuleFor(obj => obj.Address).EmailAddress().WithMessage("Email Address objects require a legitimate email address in their address field. ");
      }
    }
}
