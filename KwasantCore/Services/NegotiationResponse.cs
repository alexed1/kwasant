using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Data.Entities;
using Data.Infrastructure;
using Data.Interfaces;
using Data.States;
using KwasantWeb.ViewModels;
using StructureMap;

namespace KwasantCore.Services
{
    internal class NegotiationResponse : INegotiationResponse
    {
        private INegotiation _negotiation;

        public NegotiationResponse()
        {
            _negotiation = ObjectFactory.GetInstance<INegotiation>();
        }

        public void Process(NegotiationVM curNegotiationVM, string userID)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var curUserDO = uow.UserRepository.GetByKey(userID);
                var curNegotiationDO = uow.NegotiationsRepository.GetByKey(curNegotiationVM.Id);
                if (curNegotiationDO == null)
                    throw new HttpException(404, "Negotiation not found.");
                var questionAnswer = new Dictionary<QuestionDO, AnswerDO>();

                //Here we add/update questions based on our proposed negotiation
                foreach (var question in curNegotiationVM.Questions)
                {
                    var currentSelectedAnswers = ProcessQuestion(uow, question, curUserDO, questionAnswer);


                    var previousAnswers = uow.QuestionResponseRepository.GetQuery()
                        .Where(qr =>
                            qr.Answer.QuestionID == question.Id &&
                            qr.UserID == curUserDO.Id).ToList();

                    var previousAnswerIds = previousAnswers.Select(a => a.AnswerID).ToList();

                    var currentSelectedAnswerIDs = question.Answers.Where(a => a.Selected).Select(a => a.Id).ToList();

                    //First, remove old answers
                    foreach (
                        var previousAnswer in
                            previousAnswers.Where(
                                previousAnswer =>
                                    !previousAnswer.AnswerID.HasValue ||
                                    !currentSelectedAnswerIDs.Contains(previousAnswer.AnswerID.Value)))
                    {
                        uow.QuestionResponseRepository.Remove(previousAnswer);
                    }



                    //Add new answers
                    foreach (
                        var currentSelectedAnswer in
                            currentSelectedAnswers.Where(a => !previousAnswerIds.Contains(a.Id)))
                    {
                        var newAnswer = new QuestionResponseDO
                        {
                            Answer = currentSelectedAnswer,
                            UserID = curUserDO.Id
                        };
                        uow.QuestionResponseRepository.Add(newAnswer);
                    }
                }

                if (curNegotiationDO.NegotiationState == NegotiationState.Resolved)
                {
                    AlertManager.PostResolutionNegotiationResponseReceived(curNegotiationDO.Id);
                }
                _negotiation = new Negotiation();
                _negotiation.CreateQuasiEmailForBookingRequest(uow, curNegotiationDO, curUserDO, questionAnswer);

                uow.SaveChanges();
            }
        }



        public List<AnswerDO> ProcessQuestion(IUnitOfWork uow, NegotiationQuestionVM question, UserDO curUserDO,
            Dictionary<QuestionDO, AnswerDO> questionAnswer)
        {
            if (question.Id == 0)
                throw new HttpException(400, "Invalid parameter: Id of question cannot be 0.");

            var questionDO = uow.QuestionRepository.GetByKey(question.Id);

            var currentSelectedAnswers = new List<AnswerDO>();

            //Previous answers are read-only, we only allow updating of new answers
            foreach (var answer in question.Answers)
            {
                if (answer.Selected)
                {
                    AnswerDO answerDO;
                    if (answer.Id == 0)
                    {
                        answerDO = new AnswerDO();
                        uow.AnswerRepository.Add(answerDO);

                        answerDO.Question = questionDO;
                        if (answerDO.AnswerStatus == 0)
                            answerDO.AnswerStatus = AnswerState.Proposed;

                        answerDO.Text = answer.Text;
                        answerDO.EventID = answer.EventID;
                        answerDO.UserID = curUserDO.Id;
                    }
                    else
                    {
                        answerDO = uow.AnswerRepository.GetByKey(answer.Id);
                    }
                    questionAnswer[questionDO] = answerDO;
                    currentSelectedAnswers.Add(answerDO);
                }
            }
            return currentSelectedAnswers;
        }
    }
}

