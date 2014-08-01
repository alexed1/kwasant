namespace KwasantWeb.ViewModels
{   
    public class AnswerViewModel
    {
        public int Id { get; set; }        
        public int QuestionID { get; set; }

        public int AnswerStatusID { get; set; }        
        public string ObjectsType { get; set; }
        public string Text { get; set; }
    }
}