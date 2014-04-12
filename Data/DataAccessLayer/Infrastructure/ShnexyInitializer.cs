using System.Net.Mail;
using Data.Constants;
using Data.DataAccessLayer.Interfaces;
using Data.DataAccessLayer.Repositories;
using Data.Models;
using Data.Tools;
using StructureMap;

namespace Data.DataAccessLayer.Infrastructure
{
    public class ShnexyInitializer : System.Data.Entity.DropCreateDatabaseIfModelChanges<ShnexyDbContext>
    {
        protected override void Seed(ShnexyDbContext context)
        {
            IUnitOfWork uow = ObjectFactory.GetInstance<IUnitOfWork>();
            EmailStatusConstants.ApplySeedData(uow);
            uow.SaveChanges();
            var customer = SeedFakeCustomer();
            SeedFakeBookingRequest(customer);
            SeedFakeBookingRequest(customer);
        }

        private static CustomerDO SeedFakeCustomer()
        {
            IUnitOfWork uow = ObjectFactory.GetInstance<IUnitOfWork>();
            CustomerRepository customerRepository = new CustomerRepository(uow);
            CustomerDO newCustomer = new CustomerDO
            {
                FirstName = "Mr",
                LastName = "Client"
            };
            customerRepository.Add(newCustomer);
            return newCustomer;
        }

        private static void SeedFakeBookingRequest(CustomerDO customer)
        {
            IUnitOfWork uow = ObjectFactory.GetInstance<IUnitOfWork>();
            BookingRequestRepository emailRepository = new BookingRequestRepository(uow);

            MailMessage mailMessage = new MailMessage(new MailAddress("AClient@gmail.com", "Client Smith"),
                new MailAddress("kwa@sant.com", "Booqit Service"))
            {
                Subject = "Book me a meeting!",
                Body = "Book it in office A at 10:30am on Tuesday"
            };

            BookingRequestDO email = EmailHelper.ConvertMailMessageToEmail(emailRepository, mailMessage);
            email.Customer = customer;
            email.StatusID = EmailStatusConstants.UNPROCESSED;

            emailRepository.UnitOfWork.SaveChanges();
        }
    }
}