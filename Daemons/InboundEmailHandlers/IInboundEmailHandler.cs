using System.Collections.Generic;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace Daemons.InboundEmailHandlers
{
    interface IInboundEmailHandler
    {
        /// <summary>
        /// Tries to process passed email message.
        /// </summary>
        /// <param name="message"></param>
        /// <returns>If the message is of a wrong type returns false. Otherwise, if the message handled successfully, returns true.</returns>
        bool Process(MailMessage message);
    }
}
