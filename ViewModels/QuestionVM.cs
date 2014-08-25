using System.Collections.Generic;

namespace KwasantWeb.ViewModels
{
    public class QuestionVM
    {
        public QuestionVM()
        {
            Answers = new List<AnswerVM>();
        }

        public int Id { get; set; }
        public string Text { get; set; }
        public int Status { get; set; }
        public int NegotiationId { get; set; }
        public List<AnswerVM> Answers { get; set; }
        public string AnswerType { get; set; }
    }
}