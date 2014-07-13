using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KwasantCore.Managers.APIManager.CalDAV
{
    public interface ICalDAVEndPoint
    {
        string EventsUrlFormat { get; }
        string EventUrlFormat { get; }
    }
}
