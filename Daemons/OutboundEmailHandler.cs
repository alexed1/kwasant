using System;
using Shnexy.DataAccessLayer.Repositories;
using StructureMap;

namespace Daemons
{
    public class OutboundEmailHandler : Daemon
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
            while (ProcessNextEventNoWait()) { }
            var emailRepo = ObjectFactory.GetInstance<IEmailRepository>();

            foreach (var email in emailRepo.FindList(e => e.Status == "queued"))
            {
                email.Send();
            }
        }
    }
}
