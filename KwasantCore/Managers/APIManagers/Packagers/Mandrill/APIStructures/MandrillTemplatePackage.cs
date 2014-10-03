using System.Collections.Generic;
using Data.Entities;

namespace KwasantCore.Managers.APIManagers.Packagers.Mandrill.APIStructures
{
    /// <summary>
    /// This package combines an email message with mandrill-specific template chunks and a Mandrill key
    /// </summary>
    public class MandrillTemplatePackage : MandrillBasePackage
    {
        public string TemplateName;
        public List<MandrillDynamicContentChunk> TemplateContent;

        public MandrillTemplatePackage(string curKey, EmailDO email)
            : base(curKey, email)
        {

        }
    }
}