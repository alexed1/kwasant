using System.Web.Mvc;
using Data.Interfaces;
using StructureMap;

namespace KwasantWeb.Controllers
{
    public class KController : Controller
    {
        public IUnitOfWork GetUnitOfWork()
        {
            return ObjectFactory.GetInstance<IUnitOfWork>();
        }
    }
}
