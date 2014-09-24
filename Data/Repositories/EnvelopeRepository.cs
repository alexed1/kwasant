using System.Collections.Generic;
using System.Linq;
using Data.Entities;
using Data.Interfaces;
using Data.Validations;
using FluentValidation;
using KwasantCore.Services;
using Utilities;

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
            //Unless we're explicitly given a base url, we should override it
            var envelope = new EnvelopeDO()
            {
                TemplateName = templateName,
                Handler = EnvelopeDO.MandrillHander
            };

            if (mergeData != null)
            {
                if (!mergeData.ContainsKey("kwasantBaseURL"))
                {
                    var firstTo = email.To.SingleOrDefault();
                    if (firstTo != null)
                    {
                        var authToken = new AuthorizationToken();
                        var user = new User();
                        var tokenURL = authToken.GetAuthorizationTokenURL(UnitOfWork, Server.ServerUrl, user.GetOrCreateFromBR(UnitOfWork, firstTo));
                        mergeData["kwasantURL"] = tokenURL;
                    }
                }
                foreach (var pair in mergeData)
                {
                    envelope.MergeData.Add(pair);
                }
            }
            
            ((IEnvelope) envelope).Email = email;
            
            UnitOfWork.EnvelopeRepository.Add(envelope);
            return envelope;
        }
    }

    public interface IEnvelopeRepository : IGenericRepository<EnvelopeDO>
    {
        
    }

}
