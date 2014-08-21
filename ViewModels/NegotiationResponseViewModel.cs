using System;
using System.Collections.Generic;

namespace KwasantWeb.ViewModels
{
	public class NegotiationResponseViewModel : NegotiationViewModel
	{

	}

    public class NegotiationResponseQuestionViewModel : NegotiationQuestionViewModel
    {
        public int? SelectedAnswerID { get; set; }
        public int? SelectedCalendarID { get; set; }
        public String SelectedText { get; set; }
    }

    public class NegotiationResponseAnswerViewModel : NegotiationAnswerViewModel
    {
        public List<QuestionCalendarEventViewModel> CalendarEvents { get; set; }
    }
}