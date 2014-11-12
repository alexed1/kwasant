﻿using System;
using System.Collections.Generic;
using System.Linq;
using Data.Entities;
using Data.Interfaces;
using Data.States;
using Data.Validations;
using FluentValidation;
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

        public EnvelopeDO ConfigurePlainEmail(IEmailDO email)
        {
            if (email == null)
                throw new ArgumentNullException("email");
            return ConfigureEnvelope(email, EnvelopeDO.SendGridHander);
        }

        public EnvelopeDO ConfigureTemplatedEmail(IEmailDO email, string templateName, IDictionary<string, string> mergeData)
        {
            if (email == null)
                throw new ArgumentNullException("email");
            if (string.IsNullOrEmpty(templateName))
                throw new ArgumentNullException("templateName", "Template name is null or empty.");

            return ConfigureEnvelope(email, EnvelopeDO.SendGridHander, templateName, mergeData);
        }

        private EnvelopeDO ConfigureEnvelope(IEmailDO email, string handler, string templateName = null, IDictionary<string, string> mergeData = null)
        {
            var envelope = new EnvelopeDO
            {
                TemplateName = templateName,
                Handler = handler
            };
           
            if (mergeData != null)
            {
                if (!mergeData.ContainsKey("kwasantBaseURL"))
                {
                    var firstTo = email.To.SingleOrDefault();
                    if (firstTo != null)
                    {
                        var userDO =  UnitOfWork.UserRepository.GetOrCreateUser(firstTo);

                        var tokenURL = UnitOfWork.AuthorizationTokenRepository.GetAuthorizationTokenURL(Server.ServerUrl, userDO);
                        mergeData["kwasantBaseURL"] = tokenURL;
                    }
                }
                foreach (var pair in mergeData)
                {
                    envelope.MergeData.Add(pair);
                }
            }
            email.EmailStatus = EmailState.Queued;
            ((IEnvelopeDO)envelope).Email = email;
            envelope.EmailID = email.Id;
            
            UnitOfWork.EnvelopeRepository.Add(envelope);
            return envelope;
        }
    }

    public interface IEnvelopeRepository : IGenericRepository<EnvelopeDO>
    {
        
    }

}
