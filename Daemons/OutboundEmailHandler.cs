using System;
using Data.Constants;
using Data.DataAccessLayer.Interfaces;
using Data.DataAccessLayer.Repositories;
using DBTools;
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
            var unitOfWork = ObjectFactory.GetInstance<IUnitOfWork>();
            var emailRepository = new EmailRepository(unitOfWork);

            foreach (var email in emailRepository.FindList(e => e.Status.EmailStatusID == EmailStatusConstants.QUEUED))
            {
                EmailHelper.SendEmail(email);
            }
        }
    }
}
