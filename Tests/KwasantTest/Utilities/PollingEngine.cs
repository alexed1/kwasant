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
        class Timer : IDisposable
        {
            private readonly TimeSpan _timeout;
            private readonly Stopwatch _timer = new Stopwatch();

            public Timer(TimeSpan timeout, string description = null)
            {
                _timeout = timeout;
                Description = description;
            }

            public void Start()
            {
                _timer.Start();
            }

            public bool TimedOut { get { return _timer.Elapsed >= _timeout; } }

            public string Description { get; private set; }

            public void Dispose()
            {
                _timer.Stop();
            }
        }

        private IUnitOfWork _uow;
        private OutboundEmail _outboundDaemon;
        //POLLING MACHINERY

        private readonly List<Timer> _timers = new List<Timer>(); 

/*
        public Stopwatch totalOperationDuration = new Stopwatch();
        public List<Stopwatch> pollingDurations = new List<Stopwatch>();
        public Stopwatch requestToEmailDuration = new Stopwatch();
*/
        public TimeSpan maxPollingTime = TimeSpan.FromSeconds(90);
        public TimeSpan requestToEmailTimeout = TimeSpan.FromSeconds(60);
        public TimeSpan totalOperationTimeout = TimeSpan.FromSeconds(120);



        public PollingEngine(IUnitOfWork UOW)
        {
            _uow = UOW;
             _outboundDaemon = new OutboundEmail();
        }

/*
        public void StartTimer()
        {
            //adding user for alerts at outboundemail.cs  //If we don't add user, AlertManager at outboundemail generates error and test fails.
            //AddNewTestCustomer(testEmail.From); This should not be necessary. better approach is to create the test user at the same time we generate other fixtures, and save them together.
            
            totalOperationDuration.Start();
        }
*/

        private Timer CreateTimer(TimeSpan timeout, string description = null)
        {
            var timer = new Timer(timeout, description);
            _timers.Add(timer);
            timer.Start();
            return timer;
        }

        public IDisposable NewTimer(TimeSpan timeout, string description = null)
        {
            return CreateTimer(timeout, description);
        }

        public void FlushOutboundEmailQueues()
        {
            DaemonTests.RunDaemonOnce(_outboundDaemon);
        }

        //Check the specified account until some non-null query results are returned, or until timeout. 
        //The actual query is passed in as a delegate method called injectedQuery, which is of type InjectedEmailQuery
        //targetCriteria is passed through this method into the injectedQuery
        //this allows this method's machinery to be reused for many different kinds of email-related queries.
        public List<EmailDO> PollForEmail(InjectedEmailQuery injectedQuery, EmailDO targetCriteria, string targetType,  ImapClient curClient, InboundEmail inboundDaemon = null)
        {
            List<EmailDO> queryResults;
            List<EmailDO> unreadMessages;
            

            //run inbound daemon, checking for a generated BookingRequest, until success or timeout
            using (var timer = this.CreateTimer(maxPollingTime, "Polling"))
            {
                BookingRequestDO request;
                do
                {
                    if (inboundDaemon != null && targetType == "intake")
                    {
                        //querying one of our intake accounts. Might need to kick the Daemon, like getting a car engine to turn over
                        DaemonTests.RunDaemonOnce(inboundDaemon);
                    }


                    unreadMessages = GetUnreadMessages(curClient);
                    queryResults = injectedQuery(targetCriteria, unreadMessages).ToList();
                    Console.WriteLine(String.Format("queryResults count is {0}", queryResults.Count()));
                } while (queryResults.Count == 0 && !timer.TimedOut);
            }
            return queryResults;
        }

        //Loads unread messages from an Imap account
        public List<EmailDO> GetUnreadMessages(ImapClient client)
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
            Assert.That(_timers.All(timer => !timer.TimedOut), string.Join("\r\n", _timers.Where(timer => timer.TimedOut).Select(t => string.Format("{0} timed out.", t.Description))));

/*
            //these are old and should be generalized:
            Assert.Less(requestToEmailDuration.Elapsed, requestToEmailTimeout, "BookingRequest to Invitation conversion timed out.");
            Assert.Less(totalOperationDuration.Elapsed, totalOperationTimeout, "Workflow timed out.");
*/
        }
              //====================================


       
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
