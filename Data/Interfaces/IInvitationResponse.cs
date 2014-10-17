namespace Data.Interfaces
{
    public interface IInvitationResponse : IEmail
    {
        int? AttendeeId { get; set; }
    }
}
