using System.Collections.Generic;

namespace Data.Interfaces
{
    public interface IEnvelope
    {
        int Id { get; set; }
        string Handler { get; set; }
        string TemplateName { get; set; }
        IDictionary<string, string> MergeData { get; }
        IEmail Email { get; set; }
    }
}
