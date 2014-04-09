using System.Collections.Generic;
using Data.Models;

namespace Data.DataAccessLayer.Interfaces
{
    public interface IEmail
    {

        void Configure(Event curEvent);

        IEnumerable<Email> GetAll();

        Email GetByKey(int Id);
    }
}