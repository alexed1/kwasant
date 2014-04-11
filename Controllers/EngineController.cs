using Data.DataAccessLayer.Interfaces;
using Data.DataAccessLayer.Repositories;
using DBTools;
using S22.Imap;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Mail;
using System.Web.Mvc;
using StructureMap;

namespace Shnexy.Controllers
{
    public class EngineController : Controller
    {
        public IEmailRepository _emailRepo;
        //public IMessageRepository messageRepo;

        public EngineController(IEmailRepository emailRepo)
        {
            _emailRepo = emailRepo;
        }

        //start the system
        public ActionResult Bootstrap()
        {
            //CustomerRepository customerRepo = new CustomerRepository(new UnitOfWork(new ShnexyDbContext()));
            //Customer curCustomer = new Customer(customerRepo);
            //curCustomer.Id = 23;
            //curCustomer.email = new EmailAddress();
            //curCustomer.email.Email = "alex@edelstein.org";

            
            ////Customer curCustomer = new Customer(customerRepo);
            //curCustomer.Add();
            //customerRepo.UnitOfWork.SaveChanges();


            string username = "alexlucre1";
            string password = "lucrelucre";
            IEnumerable<MailMessage> messages;
            // Connect on port 993 using SSL.
            using (ImapClient Client = new ImapClient("imap.gmail.com", 993, username, password, AuthMethod.Login, true))
            {
                Debug.WriteLine("We are connected!");
                IEnumerable<uint> uids = Client.Search(SearchCondition.Unseen());                
                messages = Client.GetMessages(uids);               
            }

            var unitOfWork = ObjectFactory.GetInstance<IUnitOfWork>();
            var emailRepository = new EmailRepository(unitOfWork);
            foreach (var message in messages)
            {
                EmailHelper.ConvertMailMessageToEmail(emailRepository, message);
            }
            emailRepository.UnitOfWork.SaveChanges();
          
            return RedirectToAction("Index", "Admin");

        }

        public ActionResult ProcessQueues()
        {
            //curEngine.ProcessQueues();
            return View();
        }

     
 
	}
}