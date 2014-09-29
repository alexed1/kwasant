using System;
using System.Collections.Generic;
using System.Linq;
using Data.Entities;
using Newtonsoft.Json;

namespace KwasantCore.Managers.APIManagers.Packagers.Mandrill.APIStructures
{
    public class MandrillBasePackage
    {
        public class MandrilEmail
        {
            public class MandrilEmailAddress
            {
                public string Email;
                public string Name;
                public string Type;
            }

            public class MandrilHeader
            {
                public string ReplyTo;
            }

            public class MandrilAttachment
            {
                public String Type;
                public String Name;
                public String Content;
            }

            public string HTML;
            public string Subject;
            public string FromEmail;
            public string FromName;
            public List<MandrilEmailAddress> To;
            public MandrilHeader Headers;
            public bool Important;
            public List<MandrilAttachment> Attachments;
            public List<MandrilAttachment> Images;
            public List<String> Tags;
            public List<MandrillDynamicContentChunk> GlobalMergeVars;
        }

        public string Key;
        [JsonIgnore]
        public EmailDO Email;

        public MandrilEmail Message;
        public bool Async = false;

        public MandrillBasePackage(string curKey, EmailDO message)
        {
            Key = curKey;
            Email = message;

            Message = new MandrilEmail
            {
                HTML = message.HTMLText,
                Subject = message.Subject,
                FromEmail = message.From.Address,
                FromName = message.From.Name,
                To = message.To.Select(t => new MandrilEmail.MandrilEmailAddress { Email = t.Address, Name = t.Name, Type = "to" }).ToList(),
                Headers = message.ReplyTo != null ? new MandrilEmail.MandrilHeader { ReplyTo = message.ReplyTo.Address } : null,
                Important = false,
                Tags = new List<string> { Email.Id.ToString() },
                Attachments = message.Attachments.Select(a =>
                {
                    byte[] file = a.Bytes;
                    string base64Version = Convert.ToBase64String(file, 0, file.Length);

                    return new MandrilEmail.MandrilAttachment
                    {
                        Content = base64Version,
                        Name = a.OriginalName,
                        Type = a.Type
                    };
                }).ToList(),
            };
        }
    }
}