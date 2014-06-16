using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
