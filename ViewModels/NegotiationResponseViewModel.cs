using System;
using System.Collections.Generic;

namespace KwasantWeb.ViewModels
{
    public class NegotiationResponseViewModel
    {
        public int NegotiationID { get; set; }
        public List<NegotiationQuestionAnswerPair> Responses { get; set; }
    }

    public class NegotiationQuestionAnswerPair
    {
        public int QuestionID { get; set; }
        public int? AnswerID { get; set; }
        public String Response { get; set; }
    }
}