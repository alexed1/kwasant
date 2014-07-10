using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Data.Interfaces;

namespace KwasantCore.Managers.APIManager.CalDAV
{
    public interface ICalDAVClientFactory
    {
        ICalDAVClient Create(IUser user);
    }
}
