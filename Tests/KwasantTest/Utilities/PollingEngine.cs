using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using Daemons;
using Data.Entities;
using Data.Interfaces;
using Data.Repositories;
using KwasantCore.Services;
using KwasantTest.Daemons;
using NUnit.Framework;
using S22.Imap;

namespace KwasantTest.Utilities
{
    class PollingEngine
    {
        private IUnitOfWork _uow;
        private OutboundEmail _outboundDaemon;
        //POLLING MACHINERY

        public Stopwatch totalOperationDuration = new Stopwatch();
        public Stopwatch pollingDuration = new Stopwatch();
        public Stopwatch requestToEmailDuration = new Stopwatch();
        public TimeSpan maxPollingTime = TimeSpan.FromSeconds(90);
        public TimeSpan requestToEmailTimeout = TimeSpan.FromSeconds(60);
        public TimeSpan totalOperationTimeout = TimeSpan.FromSeconds(120);



        public PollingEngine(IUnitOfWork UOW)
        {
            _uow = UOW;
             _outboundDaemon = new OutboundEmail();
        }

        public void StartTimer()
        {
            //adding user for alerts at outboundemail.cs  //If we don't add user, AlertManager at outboundemail generates error and test fails.
            //AddNewTestCustomer(testEmail.From); This should not be necessary. better approach is to create the test user at the same time we generate other fixtures, and save them together.
            
            totalOperationDuration.Start();
            pollingDuration.Start();
        }

        public void FlushOutboundEmailQueues()
        {
            DaemonTests.RunDaemonOnce(_outboundDaemon);
        }

        //Check the specified account until some non-null query results are returned, or until timeout. 
        //The actual query is passed in as a delegate method called injectedQuery, which is of type InjectedEmailQuery
        //targetCriteria is passed through this method into the injectedQuery
        //this allows this method's machinery to be reused for many different kinds of email-related queries.
        public List<EmailDO> PollForEmail(InjectedEmailQuery injectedQuery, EmailDO targetCriteria, string targetType, ImapClient client)
        {
            List<EmailDO> queryResults;
            List<EmailDO> unreadMessages;
            //run inbound daemon, checking for a generated BookingRequest, until success or timeout
            InboundEmail inboundDaemon = new InboundEmail();
            BookingRequestDO request;
            do
            {
                if (targetType == "intake")
                { 
                    //querying one of our intake accounts. Might need to kick the Daemon, like getting a car engine to turn over
                    DaemonTests.RunDaemonOnce(inboundDaemon);
                }

                unreadMessages = GetUnreadMessages(client);
                queryResults = injectedQuery(targetCriteria, unreadMessages).ToList();
                Console.WriteLine(String.Format("queryResults count is {0}",queryResults.Count()));
            } while (queryResults.Count == 0 && pollingDuration.Elapsed < maxPollingTime);
            pollingDuration.Stop();
            return queryResults;
        }

        //Loads unread messages from an Imap account
        public List<EmailDO> GetUnreadMessages(IImapClient client)
        {
            IEnumerable<uint> uids = client.Search(SearchCondition.Unseen()).ToList();
            MailMessage message;
            List<EmailDO> emailList = new List<EmailDO>();
            EmailRepository emailRepo = _uow.EmailRepository;
            foreach (var uid in uids)
            {
                EmailDO email = Email.ConvertMailMessageToEmail(emailRepo,  client.GetMessage(uid));
                emailList.Add(email);
            }
            return emailList;
        }



        //this delegate allows queries to be passed into the polling mechanism
        public delegate IEnumerable<EmailDO> InjectedEmailQuery(EmailDO targetCriteria, List<EmailDO> unreadMessages );

        public void CheckTimeouts()
        {
            Assert.Less(pollingDuration.Elapsed, maxPollingTime, "Polling Duration timed out.");

            //these are old and should be generalized:
            Assert.Less(requestToEmailDuration.Elapsed, requestToEmailTimeout, "BookingRequest to Invitation conversion timed out.");
            Assert.Less(totalOperationDuration.Elapsed, totalOperationTimeout, "Workflow timed out.");
        }
              //====================================

       
        public ImapClient ConfigurePollingTarget(UserDO curTarget, string password)
        {
            var client = new ImapClient("imap.gmail.com", 993, curTarget.EmailAddress.Address, password, AuthMethod.Login, true);

            return client;
        }
        private void AddNewTestCustomer(EmailAddressDO emailAddress)
        {
            var role = new Role();
            role.Add(_uow, new KwasantTest.Fixtures.FixtureData().TestRole());
            var u = new UserDO();
            var user = new User();
            UserDO currUserDO = new UserDO();
            currUserDO.EmailAddress = emailAddress;
            currUserDO.Calendars = new List<CalendarDO>() { 
                new CalendarDO()    {
                        Id=1,
                        Name="test"
                }
            };
            CalendarDO t = new CalendarDO();
            t.Name = "test";
            t.Id = 1;
            _uow.UserRepository.Add(currUserDO);
        }
    }
}
