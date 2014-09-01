using System.Collections.Generic;
using Data.Entities;
using Data.Interfaces;
using Data.Validations;
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

        public EnvelopeDO ConfigurePlainEmail(EmailDO email)
        {
            var envelope = new EnvelopeDO()
            {
                Email = email,
                Handler = EnvelopeDO.GmailHander
            };
            UnitOfWork.EnvelopeRepository.Add(envelope);
            return envelope;
        }

        public EnvelopeDO ConfigureTemplatedEmail(IEmail email, string templateName, IDictionary<string, string> mergeData)
        {
            var envelope = new EnvelopeDO()
            {
                TemplateName = templateName,
                Handler = EnvelopeDO.MandrillHander
            };
            ((IEnvelope) envelope).Email = email;
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
