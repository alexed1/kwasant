using System.Net.Mail;
using Data.Constants;
using Data.DataAccessLayer.Interfaces;
using Data.DataAccessLayer.Repositories;
using DBTools;
using StructureMap;

namespace Data.DataAccessLayer.Infrastructure
{
    public class ShnexyInitializer : System.Data.Entity.DropCreateDatabaseIfModelChanges<ShnexyDbContext>
    {

        protected override void Seed(ShnexyDbContext context)
        {
            var uow = ObjectFactory.GetInstance<IUnitOfWork>();
            EmailStatusConstants.ApplySeedData(uow);
            uow.SaveChanges();
            SeedFakeEmail();
        }

        private void SeedFakeEmail()
        {
            IUnitOfWork uow = ObjectFactory.GetInstance<IUnitOfWork>();
            var emailRepository = new EmailRepository(uow);

            var mailMessage = new MailMessage(new MailAddress("AClient@gmail.com", "Client Smith"),
                new MailAddress("kwa@sant.com", "Booqit Service"))
            {
                Subject = "Book me a meeting!",
                Body = "Book it in office A at 10:30am on Tuesday"
            };

            var email = EmailHelper.ConvertMailMessageToEmail(emailRepository, mailMessage);
            email.StatusID = EmailStatusConstants.UNPROCESSED;

            emailRepository.UnitOfWork.SaveChanges();
        }
    }
}