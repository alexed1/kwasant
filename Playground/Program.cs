using System.Data.Entity;
using System.Net.Mail;
using System.Web.Mvc;
using Data.DataAccessLayer.Infrastructure;
using Data.DataAccessLayer.Interfaces;
using Data.DataAccessLayer.Repositories;
using Data.DataAccessLayer.StructureMap;
using Data.Tools;
using StructureMap;

namespace Playground
{
    class Program
    {
        /// <summary>
        /// This is a sandbox for devs to use. Useful for directly calling some library without needing to launch the main application
        /// </summary>
        /// <param name="args"></param>
        private static void Main(string[] args)
        {
            StructureMapBootStrapper.ConfigureDependencies("test"); //set to either "test" or "dev"
            ControllerBuilder.Current.SetControllerFactory(new StructureMapControllerFactory());

            Database.SetInitializer(new ShnexyInitializer());
            ShnexyDbContext db = new ShnexyDbContext();
            db.Database.Initialize(true);


            IUnitOfWork uow = ObjectFactory.GetInstance<IUnitOfWork>();
            EmailRepository emailRepository = new EmailRepository(uow);

            MailMessage mailMessage = new MailMessage(new MailAddress("AClient@gmail.com", "Client Smith"),
                new MailAddress("kwa@sant.com", "Booqit Service"))
            {
                Subject = "Book me a meeting!",
                Body = "Book it in office A at 10:30am on Tuesday"
            };

            EmailHelper.ConvertMailMessageToEmail(emailRepository, mailMessage);
            emailRepository.UnitOfWork.SaveChanges();
        }
    }
}
