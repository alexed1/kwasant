﻿using System;
using System.Collections.Generic;
using System.Linq;
using Data.Entities;
using Data.Interfaces;
using Data.States;
using Data.Validations;
using FluentValidation;
using StructureMap;
using Utilities;

namespace Data.Repositories
{
    public class EnvelopeRepository : GenericRepository<EnvelopeDO>, IEnvelopeRepository
    {
        private static readonly IConfigRepository _ConfigRepository = ObjectFactory.GetInstance<IConfigRepository>();
        public static readonly Dictionary<String, String> TemplateDescriptionMapping = new Dictionary<string, string>
        {
            { _ConfigRepository.Get("welcome_to_kwasant_template"), "Welcome to Kwasant" },
            { _ConfigRepository.Get("CR_template_for_creator"), "Negotiation request" },
            { _ConfigRepository.Get("CR_template_for_precustomer"), "Negotiation request" },
            { _ConfigRepository.Get("ForgotPassword_template"), "Forgot Password" },
            { _ConfigRepository.Get("User_Settings_Notification"), "User Settings Notification" },
            { _ConfigRepository.Get("user_credentials"), "User Credentials" },
            { _ConfigRepository.Get("InvitationInitial_template"), "Event Invitation" },
            { _ConfigRepository.Get("InvitationUpdate_template"), "Event Invitation Update" },
            { _ConfigRepository.Get("SimpleEmail_template"), "Simple Email" },
        };
        
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
            if (email == null)
                throw new ArgumentNullException("email");
            return ConfigureEnvelope(email, EnvelopeDO.SendGridHander);
        }

        public EnvelopeDO ConfigureTemplatedEmail(EmailDO email, string templateName, IDictionary<string, object> mergeData = null)
        {
            if (mergeData == null)
                mergeData = new Dictionary<string, object>();
            if (email == null)
                throw new ArgumentNullException("email");
            if (string.IsNullOrEmpty(templateName))
                throw new ArgumentNullException("templateName", "Template name is null or empty.");

            return ConfigureEnvelope(email, EnvelopeDO.SendGridHander, templateName, mergeData);
        }

        private EnvelopeDO ConfigureEnvelope(EmailDO email, string handler, string templateName = null, IDictionary<string, object> mergeData = null)
        {
            var envelope = new EnvelopeDO
            {
                TemplateName = templateName,
                Handler = handler
            };

            if (!String.IsNullOrEmpty(templateName) && TemplateDescriptionMapping.ContainsKey(templateName))
                envelope.TemplateDescription = TemplateDescriptionMapping[templateName];
           
            if (mergeData == null)
                mergeData = new Dictionary<string, object>();
            
            var baseUrls = new List<String>();
            const string baseUrlKey = "kwasantBaseURL";
            if (mergeData.ContainsKey(baseUrlKey))
            {
                var currentBaseURL = mergeData[baseUrlKey];
                var baseUrlList = currentBaseURL as List<String>;
                if (baseUrlList == null)
                    baseUrls = new List<string> {currentBaseURL as String};
                else
                    baseUrls = baseUrlList;
            }
            foreach(var recipient in email.Recipients)
            {
                var userDO = UnitOfWork.UserRepository.GetOrCreateUser(recipient.EmailAddress);

                var tokenURL = UnitOfWork.AuthorizationTokenRepository.GetAuthorizationTokenURL(Server.ServerUrl, userDO);
                baseUrls.Add(tokenURL);
            }
            mergeData[baseUrlKey] = baseUrls;

            foreach (var pair in mergeData)
            {
                envelope.MergeData.Add(pair);
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
