using System.Collections.Generic;

namespace KwasantWeb.ViewModels
{
    public class EventWindowVM
    {
        public List<int> LinkedCalendarIDs { get; set; }
        public int ActiveCalendarID { get; set; }

        public bool MergeEvents { get; set; }
        public bool RequiresConfirmation { get; set; }
        public bool ClickEditEnabled { get; set; }

        public EventWindowViewModel()
        {
            RequiresConfirmation = true;
            ClickEditEnabled = true;
        }
    }
}