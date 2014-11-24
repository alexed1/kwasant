using Data.Entities;

namespace KwasantTest.Fixtures
{
    public partial class FixtureData
    {
        public QuestionDO TestQuestion1()
        {
            var curQuestionDO = new QuestionDO

            {
                Id = 1,
                NegotiationId=1,
                Text = "Where should the meeting take place?",
                AnswerType = "text"
            };
            var answer = TestAnswer1();
            answer.Question = curQuestionDO;
            curQuestionDO.Answers.Add(answer);
            return curQuestionDO;
        }

        public QuestionDO TestQuestion2()
        {
            var curQuestionDO = new QuestionDO
            {
                Id = 0,
                NegotiationId = 1,
                Text = "Where should we go now?",
                AnswerType = "text"
            };
            var answer = TestAnswer1();
            answer.Question = curQuestionDO;
            curQuestionDO.Answers.Add(answer);
            return curQuestionDO;
        }

        public QuestionDO TestQuestion3()
        {
            var curQuestionDO = new QuestionDO
            {
                Id = 2,
                NegotiationId = 1,
                Text = "Where should we go now?",
                AnswerType = "text"
            };
            var answer = TestAnswer2();
            answer.Question = curQuestionDO;
            curQuestionDO.Answers.Add(answer);
            return curQuestionDO;
        }
        

    }
}
