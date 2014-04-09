using Data.Models;

namespace Data.DataAccessLayer.Interfaces
{
    public interface ICustomer
    {
        int Id { get; set; }
        EmailAddress emailAddr { get; set; }

        Customer GetByKey(int Id);
    }


}