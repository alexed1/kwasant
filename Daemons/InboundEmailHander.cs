using System;
using S22.Imap;
using Shnexy.DataAccessLayer.Repositories;
using Shnexy.Models;
using StructureMap;

namespace Daemons
{
    public class InboundEmailHander : BaseDaemon
    {
        private readonly ImapClient m_Client;

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
            m_Client = new ImapClient("imap.gmail.com", 993, username, password, AuthMethod.Login, true);
        }

        public override int WaitTimeBetweenExecution
        {
            get { return (int)TimeSpan.FromSeconds(10).TotalMilliseconds; }
        }

        protected override void Run()
        {
            var uids = m_Client.Search(SearchCondition.Unseen());
            var messages = m_Client.GetMessages(uids);

            var emailRepo = ObjectFactory.GetInstance<IEmailRepository>();
            foreach (var message in messages)
            {
                var curEmail = new Email(message, emailRepo);
                curEmail.Save();
            }
        }
    }
}
