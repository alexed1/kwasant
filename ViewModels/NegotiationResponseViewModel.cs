using System;

namespace KwasantWeb.ViewModels
{
	public class NegotiationResponseViewModel : NegotiationViewModel
	{

	}

    public class NegotiationResponseQuestionViewModel : NegotiationQuestionViewModel
    {
        public int? SelectedAnswerID { get; set; }
        public String SelectedText { get; set; }
    }

    public class NegotiationResponseAnswerViewModel : NegotiationAnswerViewModel
    {
        
    }
}