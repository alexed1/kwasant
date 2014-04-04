using System;
using System.Linq;
using Shnexy.DataAccessLayer;

namespace Daemons
{
    public class OutboundEmailHandler : BaseDaemon
    {
        public OutboundEmailHandler()
        {
            //register alertEmailSent event
            //register alertEmailRejected event
        }

        public override int WaitTimeBetweenExecution
        {
            get { return (int)TimeSpan.FromSeconds(10).TotalMilliseconds; }
        }

        protected override void Run()
        {
            while(ProcessNextEventNoWait()) {}
            using (var db = new ShnexyDbContext())
            {
                foreach (var email in db.Emails.Where(e => e.Status == "queued"))
                {
                    email.Send();
                }
            }
        }
    }
}
