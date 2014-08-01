using System.Collections.Generic;

namespace KwasantWeb.ViewModels
{
    public class NegotiationViewModel
    {
        public int Id { get; set; }        
        public int RequestId { get; set; }

        public int NegotiationStateID { get; set; }
        public string Name { get; set; }

        public List<QuestionViewModel> Questions { get; set; }                  
    }
}