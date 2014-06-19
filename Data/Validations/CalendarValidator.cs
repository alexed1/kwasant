using Data.Entities;
using FluentValidation;

namespace Data.Validators
{
  public class CalendarValidator: AbstractValidator<CalendarDO>
    {
      public CalendarValidator()
      {
          RuleFor(currCalendarDO => currCalendarDO.Name).NotNull().NotEmpty().WithMessage("Calendar name is required.").Length(1, 300);
      }
    }
}
