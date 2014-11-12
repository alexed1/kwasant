using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using Data.Infrastructure;
using Data.Interfaces;
using Data.Repositories;
using KwasantCore.Managers.APIManagers.Packagers.SendGrid;
using KwasantCore.StructureMap;
using SendGrid;
using StructureMap;

namespace Playground
{
    public class Program
    {
        /// <summary>
        /// This is a sandbox for devs to use. Useful for directly calling some library without needing to launch the main application
        /// </summary>
        /// <param name="args"></param>
        private static void Main(string[] args)
        {
            StructureMapBootStrapper.ConfigureDependencies(StructureMapBootStrapper.DependencyType.LIVE); //set to either "test" or "dev"
            KwasantDbContext db = new KwasantDbContext();
            db.Database.Initialize(true);

            DoIt();
            var a = 0;
        }

        private static void DoIt()
        {

            //Reproduce sendgrid error
            var mailMessage = new SendGridMessage() { From = new MailAddress("rjrudman@gmail.com", "Rob") };
            mailMessage.Subject = "TestSubj";

            mailMessage.To = new [] { new MailAddress("rjrudman@gmail.com") };

            mailMessage.Html = "<html></html>";
            mailMessage.Text = "";
            mailMessage.EnableTemplateEngine("09a7919f-e5d3-4c98-b6b8-d8ac6171401d");
            mailMessage.AddSubstitution("RESP_URL", new List<string>() { null });


            var credentials = new NetworkCredential
            {
                UserName = "alexed",
                Password = "thorium65"
            };
            var web = new Web(credentials);
            try
            {
                web.Deliver(mailMessage);
            }
            catch (Exception ex)
            {
                var y = 0;
                throw;
            }
        }


    }
}
