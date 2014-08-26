using Data.Entities;
using FluentValidation;

namespace Data.Validations
{
  public class BookingRequestValidator: AbstractValidator<BookingRequestDO>
    {
      public BookingRequestValidator()
      {
          RuleFor(curBR => curBR.User).NotNull().WithMessage("BR's must be associated with a valid UserDO");
         
       }
    }
}
