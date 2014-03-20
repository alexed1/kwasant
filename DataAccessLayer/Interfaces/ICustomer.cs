namespace Shnexy.Models
{
    public interface ICustomer
    {
        int Id { get; set; }
        EmailAddress email { get; set; }

        Customer GetByKey(int Id);
    }


}