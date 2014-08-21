using System;
using System.Collections.Generic;
using System.Diagnostics;
using Daemons;
using Data.Entities;
using KwasantTest.Daemons;
using NUnit.Framework;

namespace KwasantTest.Utilities
{
    class PollingEngine
    {

        #region Polling Machinery
        //POLLING MACHINERY

        public Stopwatch totalOperationDuration = new Stopwatch();
        public Stopwatch pollingDuration = new Stopwatch();
        public Stopwatch requestToEmailDuration = new Stopwatch();
        public TimeSpan maxPollingTime = TimeSpan.FromSeconds(60);
        public TimeSpan requestToEmailTimeout = TimeSpan.FromSeconds(60);
        public TimeSpan totalOperationTimeout = TimeSpan.FromSeconds(120);
        public List<EmailDO> PollForEmail(InjectedEmailQuery injectedQuery, EmailDO targetCriteria)
        {
            List<EmailDO> queryResults;
            //run inbound daemon, checking for a generated BookingRequest, until success or timeout
            InboundEmail inboundDaemon = new InboundEmail();
            BookingRequestDO request;
            do
            {
                DaemonTests.RunDaemonOnce(inboundDaemon);
                queryResults = injectedQuery(targetCriteria);
            } while (queryResults.Count == 0 && pollingDuration.Elapsed < maxPollingTime);
            pollingDuration.Stop();
            return queryResults;
        }

        //this delegate allows queries to be passed into the polling mechanism
        public delegate List<EmailDO> InjectedEmailQuery(EmailDO targetCriteria);

        public void CheckTimeouts()
        {
            Assert.Less(pollingDuration.Elapsed, maxPollingTime, "Polling Duration timed out.");

            //these are old and should be generalized:
            Assert.Less(requestToEmailDuration.Elapsed, requestToEmailTimeout, "BookingRequest to Invitation conversion timed out.");
            Assert.Less(totalOperationDuration.Elapsed, totalOperationTimeout, "Workflow timed out.");
        }
              //====================================
        #endregion Polling Machinery
    }
}
