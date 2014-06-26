﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;

namespace Data.Validators
{
    public class OutboundHtmlEmailValidator : OutboundEmailValidatorBase
    {
        public OutboundHtmlEmailValidator()
        {
            RuleFor(e => e.PlainText).Length(10, int.MaxValue).WithMessage("Email must have plain text of length at least 10 characters.");
            RuleFor(e => e.HTMLText).Length(10, int.MaxValue).WithMessage("Email must have HTML text of length at least 10 characters."); ;
        }
    }
}