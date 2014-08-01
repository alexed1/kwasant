using System.Linq;
using System.Web.Mvc;
using Data.Constants;
using Data.Entities;
using Data.Interfaces;
using KwasantWeb.ViewModels;
using StructureMap;
using System.Web.Script.Serialization;


namespace KwasantWeb.Controllers
{
    //[KwasantAuthorize(Roles = "Admin")]
    public class NegotiationController : Controller
    {
        private static int BookingRequestID { get; set; }

        #region "Negotiation"

        public ActionResult Edit(int bookingRequestID)
        {
            BookingRequestID = bookingRequestID;
            return View();
        }

        [HttpGet]
        public ActionResult ShowNegotiation(long id)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {

                NegotiationViewModel NegotiationQuestions = uow.NegotiationsRepository.GetAll().Where(e => e.RequestId == id && e.NegotiationStateID != NegotiationState.Resolved).Select(s => new NegotiationViewModel
                {
                    Id = s.Id,
                    Name = s.Name,
                    RequestId = BookingRequestID,
                    NegotiationStateID = s.NegotiationStateID,

                    Questions = uow.QuestionsRepository.GetAll().Where(que => que.NegotiationId == s.Id).Select(quel => new QuestionViewModel
                    {
                        Id = quel.Id,
                        Text = quel.Text,
                        Status = quel.QuestionStatusID,
                        NegotiationId = quel.NegotiationId,
                        Answers = uow.AnswersRepository.GetAll().Where(ans => ans.QuestionID == quel.Id).Select(ansl => new AnswerViewModel
                        {
                            Id = ansl.Id,
                            QuestionID = ansl.QuestionID,
                            AnswerStatusID = ansl.AnswerStatusID,
                            ObjectsType = ansl.ObjectsType,
                        }).ToList()
                    }).ToList()
                }).FirstOrDefault();

                //return View(NegotiationQuestions);
                return Json(NegotiationQuestions, JsonRequestBehavior.AllowGet);
            }
        }


        [HttpPost]
        public JsonResult ProcessSubmittedForm(string negotiation)
        {
            JavaScriptSerializer json_serializer = new JavaScriptSerializer();
            NegotiationViewModel viewModel = json_serializer.Deserialize<NegotiationViewModel>(negotiation);

            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                EmailDO emailDO = uow.EmailRepository.FindOne(el => el.Id == BookingRequestID);
                UserDO userDO = uow.UserRepository.FindOne(ur => ur.EmailAddressID == emailDO.FromID);

                int negotiationId = uow.NegotiationsRepository.FindOne(n => n.RequestId == BookingRequestID && n.NegotiationStateID != NegotiationState.Resolved).Id;

                if (negotiationId == null)
                {
                    //Add New Negotiation
                    NegotiationDO negotiationDO = new NegotiationDO
                    {
                        Name = viewModel.Name,
                        RequestId = BookingRequestID,
                        NegotiationStateID = NegotiationState.InProcess,
                        //State = "InProcess",
                        Email = emailDO
                    };

                    uow.NegotiationsRepository.Add(negotiationDO);
                    uow.SaveChanges();

                    foreach (var question in viewModel.Questions)
                    {

                        QuestionDO questionDO = new QuestionDO
                        {
                            Negotiation = negotiationDO,
                            QuestionStatusID = question.Status,
                            Text = question.Text,
                            AnswerType = "Text",
                        };

                        uow.QuestionsRepository.Add(questionDO);
                        uow.SaveChanges();

                        foreach (var answers in question.Answers)
                        {
                            AnswerDO answerDO = new AnswerDO
                            {
                                QuestionID = questionDO.Id,
                                AnswerStatusID = AnswerStatus.Proposed,
                                ObjectsType = answers.Text,
                                User = userDO,
                            };

                            uow.AnswersRepository.Add(answerDO);
                            uow.SaveChanges();
                        }
                    }
                    var result = new { Success = "True", BookingRequestID = emailDO.Id, NegotiationId = negotiationDO.Id };
                    return Json(result, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    //Update Negotiation
                    NegotiationDO negotiationDO = uow.NegotiationsRepository.FindOne(n => n.Id == viewModel.Id);
                    negotiationDO.Name = viewModel.Name;
                    negotiationDO.NegotiationStateID = viewModel.NegotiationStateID;
                    uow.SaveChanges();

                    foreach (var question in viewModel.Questions)
                    {
                        QuestionDO questionDO = uow.QuestionsRepository.FindOne(q => q.Id == question.Id);

                        questionDO.QuestionStatusID = question.Status;
                        questionDO.Text = question.Text;
                        questionDO.AnswerType = "Text";
                        uow.SaveChanges();

                        foreach (var answers in question.Answers)
                        {
                            AnswerDO answerDO = uow.AnswersRepository.FindOne(a => a.Id == answers.Id);
                            answerDO.AnswerStatusID = answers.AnswerStatusID;
                            answerDO.ObjectsType = answers.Text;
                            uow.SaveChanges();
                        }
                    }

                    var result = new { Success = "True", BookingRequestID = emailDO.Id, NegotiationId = negotiationDO.Id };
                    return Json(result, JsonRequestBehavior.AllowGet);
                }
            }
        }

        public PartialViewResult Addquestion(int questionID)
        {
            return PartialView("_Question", questionID);
        }

        public PartialViewResult AddtextAnswer(int answerID)
        {
            return PartialView("_TextAnswer", answerID);
        }

        public PartialViewResult AddTimeslotAnswer(int answerID)
        {
            return PartialView("_TimeslotAnswer", answerID);
        }

        #endregion "Negotiation"

    }
}
