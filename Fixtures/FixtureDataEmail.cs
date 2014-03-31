using System;
using System.Web;
using System.Text;
using System.Linq;
using Shnexy.Models;
using Shnexy.DataAccessLayer;
using Shnexy.DataAccessLayer.Repositories;
using Shnexy.DataAccessLayer.Interfaces;
using System.Collections.Generic;

namespace Shnexy.Fixtures
{
    static public class FixtureDataEmail
    {

       static public void AddEmailMessage()
       {
           ShnexyDbContext db = new ShnexyDbContext();

           //

           EmailAddress emailAddress;
           Email email = new Email();          

           StringBuilder sb = new StringBuilder();

           //Email 1
           emailAddress = new EmailAddress("Gmail Team", "mail-noreply@google.com");
           db.EmailAddresses.Add(emailAddress);
           db.SaveChanges();           

           email.Subject = "Three tips to get the most out of Gmail";

           sb.AppendLine("Hi Alex,");
           sb.AppendLine("");
           sb.AppendLine("Tips to get the most out of Gmail");
           sb.AppendLine("");
           sb.AppendLine("Bring your contacts and mail into Gmail");
           sb.AppendLine("");
           sb.AppendLine("On your computer, you can copy your contacts and emails from");
           sb.AppendLine("");
           sb.AppendLine("Find what you need fast");
           sb.AppendLine("With the power of Google Search right in your inbox, it's easy to ");
           sb.AppendLine("sort your email. Find what you're looking for with predictions based");
           sb.AppendLine("on email content, past searches and contacts.");
           sb.AppendLine("");
           sb.AppendLine("Much more than email");
           sb.AppendLine("You can send text messages and make video calls with Hangouts");
           sb.AppendLine("right from Gmail. To use this feature on mobile, download the");
           sb.AppendLine("Hangouts app for Android and Apple devices.");
           sb.AppendLine("");
           sb.AppendLine("Happy emailing,");
           sb.AppendLine("The Gmail Team");

           email.Body = sb.ToString();
           email.Sender = emailAddress;
           email.Status = "Unprocess";

           db.Emails.Add(email);

           emailAddress = new EmailAddress("Alex Foozle", "alexlucre1@gmail.com");           
           db.EmailAddresses.Add(emailAddress);
           db.SaveChanges();

           try
           {
               db.Database.ExecuteSqlCommand(String.Format("UPDATE dbo.EmailAddresses SET Email_Id2={0} WHERE Id={1}", email.Id, emailAddress.Id));               
           }
           catch(Exception e)
           {
           }

           //Email 2
           emailAddress = new EmailAddress("Gmail Team", "mail-noreply@google.com");
           db.EmailAddresses.Add(emailAddress);
           db.SaveChanges();

           email = new Email();
           email.Subject = "The best of Gmail, wherever you are";

           sb = new StringBuilder();
           sb.AppendLine("Hi Alex");
           sb.AppendLine("");
           sb.AppendLine("Get the official Gmail app");
           sb.AppendLine("");
           sb.AppendLine("The best features of Gmail are only available on your phone and ");
           sb.AppendLine("tablet with the official Gmail app. Download the app or go to ");
           sb.AppendLine("gmail.com on your computer or mobile device to get started.");
           sb.AppendLine("");
           sb.AppendLine("Happy emailing,");
           sb.AppendLine("The Gmail Team");

           email.Body = sb.ToString();
           email.Sender = emailAddress;
           email.Status = "Unprocess";

           db.Emails.Add(email);

           emailAddress = new EmailAddress("Alex Foozle", "alexlucre1@gmail.com");
           db.EmailAddresses.Add(emailAddress);
           db.SaveChanges();

           try
           {
               db.Database.ExecuteSqlCommand(String.Format("UPDATE dbo.EmailAddresses SET Email_Id2={0} WHERE Id={1}", email.Id, emailAddress.Id));               
           }
           catch (Exception e)
           {
           }

           //Email 3
           emailAddress = new EmailAddress("Gmail Team", "mail-noreply@google.com");
           db.EmailAddresses.Add(emailAddress);
           db.SaveChanges();

           email = new Email();
           email.Subject = "Stay more organized with Gmail's inbox";

           sb = new StringBuilder();
           sb.AppendLine("Hi Alex");
           sb.AppendLine("");
           sb.AppendLine("Gmail's inbox puts you in control");
           sb.AppendLine("");
           sb.AppendLine("Meet the inbox ");
           sb.AppendLine("");
           sb.AppendLine("Gmail's inbox sorts your email into categories so you can see  ");
           sb.AppendLine("what's new at a glance, decide which emails you want to read when ");
           sb.AppendLine("and view similar types of emails together. Watch the video");
           sb.AppendLine("");
           sb.AppendLine("Choose your categories");
           sb.AppendLine("");
           sb.AppendLine("The Social and Promotions categories are on by default. Add ");
           sb.AppendLine("categories like Updates and Forums or remove categories to have ");
           sb.AppendLine("those emails show up in your Primary inbox. Learn how to choose ");
           sb.AppendLine("categories");
           sb.AppendLine("");
           sb.AppendLine("Customize your inbox");
           sb.AppendLine("");
           sb.AppendLine("If you see a message you want in a different category, you can ");
           sb.AppendLine("move it there. On mobile devices, you can even choose which ");
           sb.AppendLine("categories create a notification. More customization tips");
           sb.AppendLine("");
           sb.AppendLine("To learn more about Gmail's inbox, check out the help center or watch the video.");
           sb.AppendLine("");
           sb.AppendLine("");
           sb.AppendLine("Happy emailing,");
           sb.AppendLine("The Gmail Team");

           email.Body = sb.ToString();
           email.Sender = emailAddress;
           email.Status = "Unprocess";

           db.Emails.Add(email);

           emailAddress = new EmailAddress("Alex Foozle", "alexlucre1@gmail.com");
           db.EmailAddresses.Add(emailAddress);
           db.SaveChanges();

           try
           {
               db.Database.ExecuteSqlCommand(String.Format("UPDATE dbo.EmailAddresses SET Email_Id2={0} WHERE Id={1}", email.Id, emailAddress.Id));               
           }
           catch (Exception e)
           {
           }

           //Email 4
           emailAddress = new EmailAddress("Google+ team", "noreply-daa26fef@plus.google.com");
           db.EmailAddresses.Add(emailAddress);
           db.SaveChanges();

           email = new Email();
           email.Subject = "Getting started on Google+";

           sb = new StringBuilder();
           sb.AppendLine("Welcome to Google+, Alex!");
           sb.AppendLine("");
           sb.AppendLine("Share with the people you care about, and explore the stuff you're into.");
           sb.AppendLine("");
           sb.AppendLine("Share and stay in touch with just the right people");
           sb.AppendLine("");
           sb.AppendLine("Enhance and back up your photos automatically");
           sb.AppendLine("");
           sb.AppendLine("Keep updated on-the-go on Android and iPhone");
           sb.AppendLine("");

           email.Body = sb.ToString();
           email.Sender = emailAddress;
           email.Status = "Unprocess";

           db.Emails.Add(email);

           emailAddress = new EmailAddress("Alex Foozle", "alexlucre1@gmail.com");
           db.EmailAddresses.Add(emailAddress);
           db.SaveChanges();

           try
           {
               db.Database.ExecuteSqlCommand(String.Format("UPDATE dbo.EmailAddresses SET Email_Id2={0} WHERE Id={1}", email.Id, emailAddress.Id));               
           }
           catch (Exception e)
           {
           }

           //Email 5
           emailAddress = new EmailAddress("Alex Edelstein", "alex@edelstein.org");
           db.EmailAddresses.Add(emailAddress);
           db.SaveChanges();

           email = new Email();
           email.Subject = "test message 1";

           sb = new StringBuilder();
           sb.AppendLine("");
           sb.AppendLine("");

           email.Body = sb.ToString();
           email.Sender = emailAddress;
           email.Status = "Unprocess";

           db.Emails.Add(email);

           emailAddress = new EmailAddress("alexlucre1", "alexlucre1@gmail.com");
           db.EmailAddresses.Add(emailAddress);
           db.SaveChanges();

           try
           {
               db.Database.ExecuteSqlCommand(String.Format("UPDATE dbo.EmailAddresses SET Email_Id2={0} WHERE Id={1}", email.Id, emailAddress.Id));               
           }
           catch (Exception e)
           {
           }

           //Email 6
           emailAddress = new EmailAddress("Alex Edelstein", "alexlucre1@gmail.com");
           db.EmailAddresses.Add(emailAddress);
           db.SaveChanges();

           email = new Email();
           email.Subject = "Updated Invitation: Meeting: Alex Edelstein & Mark Perutz @ Thu Mar 27, 2014 5:30pm - 6:30pm (alexlucre1@gmail.com)";

           sb = new StringBuilder();
           sb.AppendLine("");
           sb.AppendLine("");

           email.Body = sb.ToString();
           email.Sender = emailAddress;
           email.Status = "Unprocess";

           db.Emails.Add(email);

           emailAddress = new EmailAddress("alex", "alex@edelstein.org");
           db.EmailAddresses.Add(emailAddress);
           db.SaveChanges();

           try
           {
               db.Database.ExecuteSqlCommand(String.Format("UPDATE dbo.EmailAddresses SET Email_Id2={0} WHERE Id={1}", email.Id, emailAddress.Id));               
           }
           catch(Exception e)
           {
           }

           //Email 7
           emailAddress = new EmailAddress("LeaseItKeepIt", "sender@edelstein.org");
           db.EmailAddresses.Add(emailAddress);
           db.SaveChanges();

           email.Subject = "test message";

           sb = new StringBuilder();
           sb.AppendLine("");
           sb.AppendLine("foo");
           sb.AppendLine("");

           email.Body = sb.ToString();
           email.Sender = emailAddress;
           email.Status = "Unprocess";

           db.Emails.Add(email);

           emailAddress = new EmailAddress("Test Sender", "alexlucre1@gmail.com");
           db.EmailAddresses.Add(emailAddress);
           db.SaveChanges();

           try
           {
               db.Database.ExecuteSqlCommand(String.Format("UPDATE dbo.EmailAddresses SET Email_Id2={0} WHERE Id={1}", email.Id, emailAddress.Id));               
           }
           catch (Exception e)
           {
           }

           //Email 8
           emailAddress = new EmailAddress("Alex Edelstein", "alexed15@gmail.com");
           db.EmailAddresses.Add(emailAddress);
           db.SaveChanges();

           email.Subject = "Invitation: meeting 1 @ Sun Mar 30, 2014 3:30am - 4:30am (alexlucre1@gmail.com)";

           sb = new StringBuilder();
           sb.AppendLine("");
           sb.AppendLine("meeting 1");
           sb.AppendLine("");
           sb.AppendLine("When      Sun Mar 30, 2014 3:30am – 4:30am GMT (no daylight saving)");
           sb.AppendLine("");
           sb.AppendLine("Where     sdfsdf (map)");
           sb.AppendLine("");
           sb.AppendLine("Calendar  alexlucre1@gmail.com");
           sb.AppendLine("");
           sb.AppendLine("Who       •Alex Edelstein - organizer");
           sb.AppendLine("          •Alex Foozle");
           sb.AppendLine("");
           sb.AppendLine("Going?   Yes - Maybe - No    more options »");
           sb.AppendLine("");

           email.Body = sb.ToString();
           email.Sender = emailAddress;
           email.Status = "Unprocess";

           db.Emails.Add(email);

           emailAddress = new EmailAddress("Test Sender", "alexlucre1@gmail.com");
           db.EmailAddresses.Add(emailAddress);
           db.SaveChanges();

           try
           {
               db.Database.ExecuteSqlCommand(String.Format("UPDATE dbo.EmailAddresses SET Email_Id2={0} WHERE Id={1}", email.Id, emailAddress.Id));               
           }
           catch (Exception e)
           {
           }

           //Email 9
           emailAddress = new EmailAddress("Alexed", "alex@edelstein.org");
           db.EmailAddresses.Add(emailAddress);
           db.SaveChanges();

           email.Subject = "Meeting Request from Alex Edelstein Sent by Booqit";

           sb = new StringBuilder();
           sb.AppendLine("");
           sb.AppendLine("this the HTML ");
           sb.AppendLine("");

           email.Body = sb.ToString();
           email.Sender = emailAddress;
           email.Status = "Unprocess";

           db.Emails.Add(email);

           emailAddress = new EmailAddress("Alex Lucre1", "alexlucre1@gmail.com");
           db.EmailAddresses.Add(emailAddress);
           db.SaveChanges();

           try
           {
               db.Database.ExecuteSqlCommand(String.Format("UPDATE dbo.EmailAddresses SET Email_Id2={0} WHERE Id={1}", email.Id, emailAddress.Id));               
           }
           catch (Exception e)
           {

           }
           db.SaveChanges();
       }
    }
}