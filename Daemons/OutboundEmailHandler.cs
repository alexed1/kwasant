using System;
using Data.Constants;
using Data.Entities;
using Data.Interfaces;
using Data.Repositories;
using KwasantCore.Services;
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
                new Email(unitOfWork, email).Send();
            }
        }
    }
}
