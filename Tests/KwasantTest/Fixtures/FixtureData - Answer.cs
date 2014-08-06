using Data.Entities;

namespace KwasantTest.Fixtures
{
    public partial class FixtureData
    {
        public AnswerDO TestAnswer1()
        {
            var curAnswerDO = new AnswerDO

            {
                Id = 1,
                Text = "Starbucks on Valencia",
            };
            return curAnswerDO;
        }
    }
}
