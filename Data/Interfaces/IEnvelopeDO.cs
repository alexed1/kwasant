using System.Collections.Generic;

namespace Data.Interfaces
{
    public interface IEnvelopeDO : IBaseDO
    {
        int Id { get; set; }
        string Handler { get; set; }
        string TemplateName { get; set; }
        IDictionary<string, string> MergeData { get; }
        IEmailDO Email { get; set; }
    }
}
