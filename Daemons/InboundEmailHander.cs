using System;
using Data.DataAccessLayer.Repositories;
using Data.Models;
using S22.Imap;

using StructureMap;

namespace Daemons
{
    public class InboundEmailHander : Daemon
    {
        private readonly ImapClient _client;

        private static string GetIMAPServer()
        {
            return "imap.gmail.com";
        }

        private static int GetIMAPPort()
        {
            return 993;
        }

        private string GetUserName()
        {
            return "alexlucre1";
        }
        private string GetPassword()
        {
            return "lucrelucre";
        }

        private static bool UseSSL()
        {
            return true;
        }

        public InboundEmailHander()
        {
            _client = new ImapClient(GetIMAPServer(), GetIMAPPort(), GetUserName(), GetPassword(), AuthMethod.Login, UseSSL());
        }

        public override int WaitTimeBetweenExecution
        {
            get { return (int)TimeSpan.FromSeconds(10).TotalMilliseconds; }
        }

        protected override void Run()
        {
            var uids = _client.Search(SearchCondition.Unseen());
            var messages = _client.GetMessages(uids);

            var emailRepo = ObjectFactory.GetInstance<IEmailRepository>();
            foreach (var message in messages)
            {
                var curEmail = new Email(message, emailRepo);
                curEmail.Save();
            }
        }

        protected override void CleanUp()
        {
            _client.Dispose();
        }
    }
}
