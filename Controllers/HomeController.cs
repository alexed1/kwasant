using System.Web.Mvc;

using Data.Entities;
using Data.Interfaces;
using Data.Repositories;
using KwasantCore.StructureMap;

using StructureMap;

namespace KwasantWeb.Controllers
{
    public class HomeController : Controller
    {
        private IUnitOfWork _uow;
        IPersonRepository personRepo;
        ICalendarRepository calendarRepo;

        public CalendarDO SetupCalendarForTests()
        {
            PersonDO curPersonDO = new PersonDO 
            {
                FirstName="aaa",
                LastName ="bbb"
            };

            personRepo.Add(curPersonDO);
            personRepo.UnitOfWork.SaveChanges();

            CalendarDO calendarDO = new CalendarDO()
            {
                Name = "Calendar Test",
                PersonId = curPersonDO.PersonId
            };

            return calendarDO;
        }

        public ActionResult Index()
        {
            StructureMapBootStrapper.ConfigureDependencies("test");
            _uow = ObjectFactory.GetInstance<IUnitOfWork>();

            personRepo = new PersonRepository(_uow);
            calendarRepo = new CalendarRepository(_uow);

            CalendarDO curOriginalCalendarDO = SetupCalendarForTests();

            //EXECUTE
            calendarRepo.Add(curOriginalCalendarDO);
            calendarRepo.UnitOfWork.SaveChanges();

            return Redirect("/index.html");
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}