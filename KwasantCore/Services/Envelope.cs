using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Data.Entities;

namespace KwasantCore.Services
{
    public class Envelope
    {
        public static EnvelopeDO CreateGmailEnvelope(EmailDO email)
        {
            return new EnvelopeDO()
                       {
                           Email = email, 
                           Handler = EnvelopeDO.GmailHander
                       };
        }

        public static EnvelopeDO CreateMandrillEnvelope(EmailDO email, string temlateName, IDictionary<string, string> mergeData)
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
            return envelope;
        }
    }
}
