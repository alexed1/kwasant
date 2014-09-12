using System;
using System.Collections.Generic;

namespace KwasantWeb.ViewModels
{
	public class NegotiationResponseVM : NegotiationVM
	{

	}

    public class NegotiationResponseQuestionVM : NegotiationQuestionVM
    {
    }

    public class NegotiationResponseAnswerVM : NegotiationAnswerVM
    {
        public bool UserAnswer { get; set; }
    }
}