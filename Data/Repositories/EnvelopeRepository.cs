using System;
using System.Collections.Generic;
using System.Diagnostics;
using Data.Entities;
using Data.Interfaces;
using Data.States;
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

        public EnvelopeDO ConfigurePlainEmail(IEmail email)
        {
            if (email == null)
                throw new ArgumentNullException("email");
            return ConfigureEnvelope(email, EnvelopeDO.GmailHander);
        }

        public EnvelopeDO ConfigureTemplatedEmail(IEmail email, string templateName, IDictionary<string, string> mergeData)
        {
            if (email == null)
                throw new ArgumentNullException("email");
            if (string.IsNullOrEmpty(templateName))
                throw new ArgumentNullException("templateName", "Template name is null or empty.");
            return ConfigureEnvelope(email, EnvelopeDO.MandrillHander, templateName, mergeData);
        }

        private EnvelopeDO ConfigureEnvelope(IEmail email, string handler, string templateName = null, IDictionary<string, string> mergeData = null)
        {
            Debug.Assert(email != null);

            var envelope = new EnvelopeDO {Handler = handler};
            ((IEnvelope)envelope).Email = email;
            envelope.TemplateName = templateName;
            if (mergeData != null)
            {
                foreach (var pair in mergeData)
                {
                    envelope.MergeData.Add(pair);
                }
            }
            email.EmailStatus = EmailState.Queued;
            UnitOfWork.EnvelopeRepository.Add(envelope);
            return envelope;
        }
    }

    public interface IEnvelopeRepository : IGenericRepository<EnvelopeDO>
    {
        
    }

}
