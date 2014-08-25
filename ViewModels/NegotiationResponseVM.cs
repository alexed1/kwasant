using System;
using System.Collections.Generic;

namespace KwasantWeb.ViewModels
{
	public class NegotiationResponseVM : NegotiationVM
	{

	}

    public class NegotiationResponseQuestionVM : NegotiationQuestionVM
    {
        public int? SelectedAnswerID { get; set; }
        public int? SelectedCalendarID { get; set; }
        public String SelectedText { get; set; }
    }

    public class NegotiationResponseAnswerVM : NegotiationAnswerVM
    {
        public List<QuestionCalendarEventVM> CalendarEvents { get; set; }
    }
}