using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Data.Entities;
using FluentValidation;
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
                .GreaterThan(eventDO => eventDO.StartDate)
                .WithMessage("End date must after Start date");


            RuleFor(eventDO => eventDO.Priority)
           .NotNull()
           .GreaterThan(0)
           .WithMessage("Priority must be greater than 0.");

           RuleFor(eventDO => eventDO.Sequence)
          .NotNull()
          .GreaterThan(0)
          .WithMessage("Sequence must be greater than 0.");

           RuleFor(eventDO => eventDO.Description).Length(3, 200).WithMessage("Event Description must be between 3 and 200 characters"); ;
           
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

            if (list != null && list.Count < 1)
            {
                return false;
            }

            return true;
        }
    }
}
