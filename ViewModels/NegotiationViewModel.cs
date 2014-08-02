using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Data.Entities.Constants;

namespace KwasantWeb.ViewModels
{
    public class NegotiationViewModel
    {
        public int Id { get; set; }        
        public int RequestId { get; set; }
        public NegotiationStateRow State { get; set; }
        public string Name { get; set; }

        public List<QuestionViewModel> Questions { get; set; }                  
    }
}