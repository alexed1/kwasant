namespace Shnexy.Models
{
    public interface ICustomer
    {
        int Id { get; set; }
        EmailAddress emailAddr { get; set; }

        Customer GetByKey(int Id);
    }


}