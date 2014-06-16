using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Data.Entities;
using FluentValidation;

namespace Data.Validators
{
  public class BookingRequestValidator: AbstractValidator<BookingRequestDO>
    {
      public BookingRequestValidator()
      {
          RuleFor(curBR => curBR.User).NotNull().WithMessage("BR's must be associated with a valid UserDO");
         
       }
    }
}
