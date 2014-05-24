using System;
using System.Collections.Generic;

using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Data.Entities;
using FluentValidation;
using FluentValidation.Results;
using FluentValidation.Validators;


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

            RuleFor(eventDO => eventDO.Description).Length(3, 200)
                .WithMessage("Event Description must be between 3 and 200 characters");

            //we currently don't really use sequence and priority, so don't test for them here.
            //RuleFor(eventDO => eventDO.Sequence).NotEqual(0)
            //    .WithMessage("Event sequence must not equal to 0.");
            //RuleFor(eventDO => eventDO.Priority).NotEqual(0)
            //    .WithMessage("Event priority must not equal to 0.");

            RuleFor(eventDO => eventDO.Attendees).SetValidator(new ListMustContainAtLeastOneItem<AttendeeDO>())
                .WithMessage("Event must have at least one attendee");

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


    //=================================================================
    //Custom Validators
    public class ListMustContainAtLeastOneItem<T> : PropertyValidator
    {
        public ListMustContainAtLeastOneItem()
            : base("Property {PropertyName} must contain at least 1 item!")
        {

        }

        protected override bool IsValid(PropertyValidatorContext context)
        {
            var list = context.PropertyValue as IList<T>;

            if (list == null || list.Count < 1)
            {
                return false;
            }

            return true;
        }
    }

 
}
