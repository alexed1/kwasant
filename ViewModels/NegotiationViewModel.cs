using System.Collections.Generic;

namespace KwasantWeb.ViewModels
{
    public class NegotiationViewModel
    {
        public NegotiationViewModel()
        {
            Questions = new List<QuestionViewModel>();
        }

        public int Id { get; set; }        
        public int BookingRequestID { get; set; }
        public int State { get; set; }
        public string Name { get; set; }
        public List<QuestionViewModel> Questions { get; set; }                  
    }
}