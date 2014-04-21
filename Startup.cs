using System;
using System.Configuration;
using System.Net.Mail;
using Configuration;
using Daemons;
using Data.Constants;
using Data.DataAccessLayer.Interfaces;
using Data.DataAccessLayer.Repositories;
using Data.Models;
using KwasantCore.Services;
using Microsoft.Owin;
using Owin;
using StructureMap;

[assembly: OwinStartupAttribute(typeof(Shnexy.Startup))]

namespace Shnexy
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureDaemons();

            //ConfigureAuth(app);

            CustomerDO customer = SeedFakeCustomer();
            SeedFakeBookingRequest(customer);
            SeedFakeBookingRequest(customer);
        }

        private static void ConfigureDaemons()
        {
            DaemonSettings daemonConfig = ConfigurationManager.GetSection("daemonSettings") as DaemonSettings;
            if (daemonConfig != null)
            {
                if (daemonConfig.Enabled)
                {
                    foreach (DaemonConfig daemon in daemonConfig.Daemons)
                    {
                        if (daemon.Enabled)
                        {
                            Type type = Type.GetType(daemon.InitClass, true);
                            Daemon obj = Activator.CreateInstance(type) as Daemon;
                            if (obj == null)
                                throw new ArgumentException(
                                    string.Format(
                                        "An daemon must implement IDaemon. Type '{0}' does not implement the interface.",
                                        type.Name));
                            obj.Start();
                        }
                    }
                }
            }
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

            BookingRequestDO email = EmailServices.ConvertMailMessageToEmail(emailRepository, mailMessage);
            email.Customer = customer;
            email.StatusID = EmailStatusConstants.UNPROCESSED;

            emailRepository.UnitOfWork.SaveChanges();
        }
    }
}
