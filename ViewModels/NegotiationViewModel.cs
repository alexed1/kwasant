using System;
using System.Collections.Generic;
using Data.Entities;

namespace KwasantWeb.ViewModels
{
    public class NegotiationViewModel
    {
        public NegotiationViewModel()
        {
            Questions = new List<QuestionViewModel>();
            Attendees = new List<string>();
        }

        public int? Id { get; set; }        
        public int BookingRequestID { get; set; }
        public int State { get; set; }
        public string Name { get; set; }
        public List<QuestionViewModel> Questions { get; set; }
        public List<String> Attendees { get; set; } 
    }
}