using System.Collections.Generic;
using Data.Entities.Enumerations;

namespace KwasantWeb.ViewModels
{
    public class NegotiationViewModel
    {
        public int Id { get; set; }        
        public int RequestId { get; set; }
        public NegotiationState State { get; set; }
        public string Name { get; set; }

        public List<QuestionViewModel> Questions { get; set; }                  
    }
}