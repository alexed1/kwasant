using System.Collections.Generic;
using Data.Entities;
using Data.Interfaces;

namespace Data.Repositories
{
    public class EnvelopeRepository : GenericRepository<EnvelopeDO>
    {
        internal EnvelopeRepository(IUnitOfWork uow) : base(uow)
        {
        }

        public EnvelopeDO CreateGmailEnvelope(EmailDO email)
        {
            var envelope = new EnvelopeDO()
            {
                Email = email,
                Handler = EnvelopeDO.GmailHander
            };
            UnitOfWork.EnvelopeRepository.Add(envelope);
            return envelope;
        }

        public EnvelopeDO CreateMandrillEnvelope(EmailDO email, string temlateName, IDictionary<string, string> mergeData)
        {
            var envelope = new EnvelopeDO()
            {
                Email = email,
                TemplateName = temlateName,
                Handler = EnvelopeDO.MandrillHander
            };
            if (mergeData != null)
            {
                foreach (var pair in mergeData)
                {
                    envelope.MergeData.Add(pair);
                }
            }

            UnitOfWork.EnvelopeRepository.Add(envelope);
            return envelope;
        }
    }

    public interface IEnvelopeRepository : IGenericRepository<EnvelopeDO>
    {
        
    }

}
