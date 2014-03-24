using System.Collections.Generic;

namespace Shnexy.Models
{
    public interface IEmail
    {

        void Configure(EmailAddress destEmailAddress, int eventId, string filename);

        IEnumerable<Email> GetAll();

        Email GetByKey(int Id);
    }
}