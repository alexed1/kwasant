using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using Data.Interfaces;
using StructureMap;
using System.Web.Mvc;

namespace Data.Infrastructure
{


    public class KController : Controller
    {

        public IUnitOfWork UOW()
        {
            return ObjectFactory.GetInstance<IUnitOfWork>();
        }
    }
}
