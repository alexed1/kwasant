using System.Collections.Generic;

namespace Shnexy.Models
{
    public interface IEmail
    {

        void Configure(Event curEvent);

        IEnumerable<Email> GetAll();

        Email GetByKey(int Id);
    }
}