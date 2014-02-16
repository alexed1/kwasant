using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using Shnexy.Models;
using Shnexy.DataAccessLayer;
using shnexy.Migrations;

namespace Shnexy.Controllers
{
    public class MessageController : Controller
    {
        private IMessageRepository messageRepository;
        private IQueueRepository queueRepo;
        public MessageController()
        {
            // this.messageRepository = new MessageRepository(new ShnexyDbContext());
            queueRepo = new QueueRepository(new UnitOfWork(new ShnexyDbContext()));

        }

        public MessageController(IMessageRepository messageRepository) : this()
        {
            this.messageRepository = messageRepository;

        }

        // GET: /Message/
        public ActionResult Index()
        {
            var messages = messageRepository.GetQuery();
            return View(messages);
        }

        // GET: /Message/Details/5
        public ViewResult Details(int id)
        {

            Message message = messageRepository.GetByKey(id); // db.Messages.Find(id);

            return View(message);
        }

        // GET: /Message/Create
        public ActionResult Create()
        {
            return View();
        }


        // GET: /Message/Send
        public ActionResult Send()
        {
            int id = 1;


            Address address806 = new Address();
            address806.Body = "14158067915";
            address806.ServiceName = "WhatsApp";

            Address address871 = new Address();
            address871.Body = "14158710872";
            address871.ServiceName = "WhatsApp";

            Message message = new Message(messageRepository); //why does this work, but not the version in Configuration#Seed, which causes the RecipientList to be null?
            message.RecipientList.Add(address806);
            message.Sender = address871;
            message.Body = "Hello, Come here, please! ";
            message.Id = 45;
            messageRepository.Add(message);
            messageRepository.Save(message);


            //Message message = messageRepository.GetByKey(id);
            //message.RecipientList.Add(address806);
            message.Send(queueRepo);
            return View("Index", "Admin");
        }

        // POST: /Message/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,ServiceName")] Message message)
        {
            if (ModelState.IsValid)
            {
                messageRepository.Add(message); //db.Messages.Add(message);
                messageRepository.Save(message); //db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(message);
        }

        // GET: /Message/Edit/5
        public ActionResult Edit(int id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Message message = messageRepository.GetByKey(id); // db.Messages.Find(id);
            if (message == null)
            {
                return HttpNotFound();
            }
            return View(message);
        }

        // POST: /Message/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,ServiceName")] Message message)
        {
            if (ModelState.IsValid)
            {
                messageRepository.Update(message); // db.Entry(message).State = EntityState.Modified;
                messageRepository.Save(message); // db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(message);
        }

        // GET: /Message/Delete/5
        public ActionResult Delete(int id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Message message = messageRepository.GetByKey(id);  //db.Messages.Find(id);
            if (message == null)
            {
                return HttpNotFound();
            }
            return View(message);
        }

        // POST: /Message/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Message message = messageRepository.GetByKey(id);  //db.Messages.Find(id);
            messageRepository.Remove(message); // db.Messages.Remove(message);
            //messageRepository.Save(message); // db.SaveChanges();

            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                messageRepository.Dispose(); // db.Dispose();
            }
            base.Dispose(disposing);
        }
    }

}
