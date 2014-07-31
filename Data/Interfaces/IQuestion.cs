namespace Data.Interfaces
{
    public interface IQuestion
    {
        int Id { get; set; }
        int? ClarificationRequestId { get; set; }
        int QuestionStatusID { get; set; }
        string Text { get; set; }
        string Response { get; set; }
    }
}
