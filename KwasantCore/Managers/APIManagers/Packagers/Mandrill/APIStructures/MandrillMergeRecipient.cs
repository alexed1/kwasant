using System;
using System.Collections.Generic;

namespace KwasantCore.Managers.APIManagers.Packagers.Mandrill.APIStructures
{
    /// <summary>
    /// In the Mandrill JSON, dynamic merge data must be provided on a per-recipient basis. Each recipient can have a List of dynamic chunks.
    /// </summary>
    [Serializable]
    public class MandrillMergeRecipient
    {
        public string Rcpt;
        public List<MandrillDynamicContentChunk> Vars;

        public MandrillMergeRecipient()
        {
            Vars = new List<MandrillDynamicContentChunk> { };
        }
    }
}