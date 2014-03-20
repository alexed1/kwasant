namespace Shnexy.Models
{
    public interface IEmail
    {

        void Configure(EmailAddress destEmailAddress, int eventId, string filename);
    }
}