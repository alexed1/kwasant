using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KwasantCore.Interfaces
{
    public interface INotification
    {
        bool IsInNotificationWindow(string startTimeConfigName, string endTimeConfigName);
    }
}
