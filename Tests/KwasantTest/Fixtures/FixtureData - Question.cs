using Data.Constants;
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
                Text = "Where should the meeting take place?",
                AnswerType = "text"
            };
            curQuestionDO.Answers.Add(TestAnswer1());
            return curQuestionDO;
        }
    }
}
