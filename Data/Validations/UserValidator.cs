﻿using Data.Entities;
using FluentValidation;

namespace Data.Validators
{
  public class UserValidator: AbstractValidator<UserDO>
    {
      public UserValidator()
      {
          RuleFor(curUserDO => curUserDO.EmailAddress.Address).NotNull().WithMessage("Users must be associated with a valid EmailAddress object containing a valid email address.");
          RuleFor(curUserDO => curUserDO.UserName).NotNull().WithMessage("Users must have a UserName. We don't use this much but Identity requires it. Use the email address.");

      }
    }
}
