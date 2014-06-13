using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KwasantCore.Managers.APIManager.Packagers
{
    public class UnknownEmailPackagerException : ApplicationException
    {
        public UnknownEmailPackagerException()
        {
            
        }

        public UnknownEmailPackagerException(string message)
            : base(message)
        {
            
        }

        public UnknownEmailPackagerException(string message, Exception innerException)
            : base(message, innerException)
        {
            
        }
    }
}
