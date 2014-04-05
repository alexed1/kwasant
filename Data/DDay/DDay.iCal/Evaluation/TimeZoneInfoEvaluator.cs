﻿using System;
using System.Collections.Generic;
using Data.DDay.DDay.iCal.Interfaces.Components;
using Data.DDay.DDay.iCal.Interfaces.DataTypes;

namespace Data.DDay.DDay.iCal.Evaluation
{
    public class TimeZoneInfoEvaluator :
        RecurringEvaluator
    {
        #region Protected Properties

        protected ITimeZoneInfo TimeZoneInfo
        {
            get { return Recurrable as ITimeZoneInfo; }
            set { Recurrable = value; }
        }

        #endregion

        #region Constructors

        public TimeZoneInfoEvaluator(ITimeZoneInfo tzi) : base(tzi)
        {
        }

        #endregion       
 
        #region Overrides

        public override IList<IPeriod> Evaluate(IDateTime referenceDate, DateTime periodStart, DateTime periodEnd, bool includeReferenceDateInResults)
        {
            // Time zones must include an effective start date/time
            // and must provide an evaluator.
            if (TimeZoneInfo != null)
            {
                // Always include the reference date in the results
                IList<IPeriod> periods = base.Evaluate(referenceDate, periodStart, periodEnd, true);
                return periods;
            }

            return new List<IPeriod>();            
        }

        #endregion
    }
}
