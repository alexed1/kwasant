using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using Shnexy.Models;
using Shnexy.DataAccessLayer;

namespace Shnexy.Controllers
{
    public class MessageController : Controller
    {
        private readonly IMessageRepository repo;
        public MessageController()
        {
            //this.messageRepository = new MessageRepository(new ShnexyDbContext());

        }

        public MessageController(IMessageRepository messageRepository)
        {
            repo = messageRepository;
        }

        // GET: /Message/
        public ActionResult Index()
        {
            var messages = repo.GetMessages();
            return View(messages);
        }

        // GET: /Message/Details/5
        public ViewResult Details(int id)
        {

            Message message = repo.GetMessageById(id); // db.Messages.Find(id);

            return View(message);
        }

        // GET: /Message/Create
        public ActionResult Create()
        {
            return View();
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
                repo.InsertMessage(message); //db.Messages.Add(message);
                repo.Save(); //db.SaveChanges();
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
            Message message = repo.GetMessageById(id); // db.Messages.Find(id);
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
                repo.UpdateMessage(message); // db.Entry(message).State = EntityState.Modified;
                repo.Save(); // db.SaveChanges();
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
            Message message = repo.GetMessageById(id);  //db.Messages.Find(id);
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
            Message message = repo.GetMessageById(id);  //db.Messages.Find(id);
            repo.DeleteMessage(id); // db.Messages.Remove(message);
            repo.Save(); // db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                repo.Dispose(); // db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
