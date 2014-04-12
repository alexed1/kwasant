using System;
using Data.Constants;
using Data.DataAccessLayer.Interfaces;
using Data.DataAccessLayer.Repositories;
using Data.Models;
using Data.Tools;
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
            IUnitOfWork unitOfWork = ObjectFactory.GetInstance<IUnitOfWork>();
            EmailRepository emailRepository = new EmailRepository(unitOfWork);

            foreach (EmailDO email in emailRepository.FindList(e => e.Status.EmailStatusID == EmailStatusConstants.QUEUED))
            {
                EmailHelper.SendEmail(email);
            }
        }
    }
}
