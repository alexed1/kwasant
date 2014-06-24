using System.Collections.Generic;
using Data.Entities;
using Data.Interfaces;
using Data.Validators;
using FluentValidation;

namespace Data.Repositories
{
    public class EnvelopeRepository : GenericRepository<EnvelopeDO>, IEnvelopeRepository
    {
        private readonly EnvelopeValidator _validator;

        internal EnvelopeRepository(IUnitOfWork uow) : base(uow)
        {
            _validator = new EnvelopeValidator();
        }

        public override void Add(EnvelopeDO entity)
        {
            _validator.ValidateAndThrow(entity);
            base.Add(entity);
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
