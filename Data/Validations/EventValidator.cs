using Data.Entities;
using FluentValidation;
using FluentValidation.Results;

namespace Data.Validators
{
    public class EventValidator : AbstractValidator<EventDO>
    {
        public EventValidator()
        {
            RuleFor(eventDO => eventDO.StartDate)
                .NotEmpty()
                .WithMessage("Start Date is Required");

            RuleFor(eventDO => eventDO.EndDate)
                .NotEmpty().WithMessage("End date is required")
                .GreaterThanOrEqualTo(eventDO => eventDO.StartDate)
                .WithMessage("End date must after Start date");

            //RuleFor(eventDO => eventDO.Description).NotNull().SetValidator(new StringMinLength(3));
            //initial event creation has no description, so a general purpose rule here won't suit. need to either split into two different event validators or do something else


            RuleFor(eventDO => eventDO.Attendees).SetValidator(new ListMustContainAtLeastOneItem<AttendeeDO>())
                .WithMessage("Event must have at least one attendee");

            RuleFor(eventDO => eventDO.CreatedBy)
                .NotNull()
                .WithMessage("CreatedBy is Required");

        }

        //=================================================================
        //Utilities 
        //TO DO: Genericize this
        public void ValidateEvent(EventDO curEventDO)
        {
            ValidationResult results = Validate(curEventDO);
            if (results.IsValid)
                return;

            throw new ValidationException(results.Errors);

        }
    }
}
