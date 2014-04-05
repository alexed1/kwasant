using System;
using S22.Imap;
using Shnexy.DataAccessLayer.Repositories;
using Shnexy.Models;
using StructureMap;

namespace Daemons
{
    public class InboundEmailHander : Daemon
    {
        private readonly ImapClient _client;

        private string GetUserName()
        {
            return "alexlucre1";
        }
        private string GetPassword()
        {
            return "lucrelucre";
        }

        public InboundEmailHander()
        {
            var username = GetUserName();
            var password = GetPassword();
            _client = new ImapClient("imap.gmail.com", 993, username, password, AuthMethod.Login, true);
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
    }
}
