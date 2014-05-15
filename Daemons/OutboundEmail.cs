using System;
using Data.Constants;
using Data.Entities;
using Data.Entities.Enumerations;
using Data.Interfaces;
using Data.Repositories;
using KwasantCore.Services;
using StructureMap;
using UtilitiesLib.Logging;

namespace Daemons
{
    public class OutboundEmail : Daemon
    {
        public OutboundEmail()
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
            var numSent = 0;
            foreach (EmailDO email in emailRepository.FindList(e => e.Status == EmailStatus.QUEUED))
            {
                new Email(unitOfWork, email).Send();
                numSent++;
            }
            Logger.GetLogger().Info(numSent + " emails sent.");
        }
    }
}
