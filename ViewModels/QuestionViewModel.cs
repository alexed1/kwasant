using System.Collections.Generic;

namespace KwasantWeb.ViewModels
{
    public class QuestionViewModel
    {
        public int Id { get; set; }
        public string Text { get; set; }
        public int Status { get; set; }
        public int NegotiationId { get; set; }
        public List<AnswerViewModel> Answers { get; set; }
    }
}