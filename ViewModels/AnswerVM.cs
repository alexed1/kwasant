namespace KwasantWeb.ViewModels
{   
    public class AnswerVM
    {
        public int Id { get; set; }        
        public int QuestionID { get; set; }       
        public int AnswerState { get; set; }
        public string ObjectsType { get; set; }
        public string Text { get; set; }
        public int? CalendarID { get; set; }
    }
}