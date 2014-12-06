using System;
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

        public EnvelopeDO ConfigurePlainEmail(IEmailDO email)
        {
            if (email == null)
                throw new ArgumentNullException("email");
            return ConfigureEnvelope(email, EnvelopeDO.SendGridHander);
        }

        public EnvelopeDO ConfigureTemplatedEmail(IEmailDO email, string templateName, IDictionary<string, string> mergeData = null)
        {
            if (mergeData == null)
                mergeData = new Dictionary<string, string>();
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

            if (!String.IsNullOrEmpty(templateName) && TemplateDescriptionMapping.ContainsKey(templateName))
                envelope.TemplateDescription = TemplateDescriptionMapping[templateName];
           
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
