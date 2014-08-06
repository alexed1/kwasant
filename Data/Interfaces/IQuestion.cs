namespace Data.Interfaces
{
    public interface IQuestion
    {
        int Id { get; set; }
        int QuestionStatus { get; set; }
        string Text { get; set; }
        string Response { get; set; }
    }
}
