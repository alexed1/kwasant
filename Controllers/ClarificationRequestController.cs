using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using AutoMapper;
using Data.Entities;
using Data.Interfaces;
using DayPilot.Web.Mvc.Json;
using KwasantCore.Exceptions;
using KwasantCore.Managers;
using KwasantCore.Services;
using KwasantWeb.App_Start;
using KwasantWeb.Filters;
using KwasantWeb.ViewModels;
using StructureMap;

namespace KwasantWeb.Controllers
{
    [HandleError]
    public class ClarificationRequestController : Controller
    {
        [KwasantAuthorize(Roles = "Admin")]
        public ActionResult Create(int bookingRequestId, int clarificationRequestId = 0, int negotiationId=0)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var cr = new ClarificationRequest();
                try
                {
                    //NegotiationDO negotiationDO = uow.NegotiationsRepository.FindOne(br => br.RequestId == bookingRequestId);
                    var curClarificationRequestDO = cr.GetOrCreateClarificationRequest(uow, bookingRequestId, clarificationRequestId, negotiationId);
                    return View(Mapper.Map<ClarificationRequestViewModel>(curClarificationRequestDO));
                }
                catch (EntityNotFoundException<IBookingRequest>)
                {
                    return HttpNotFound("Booking request not found.");
                }
            }
        }

        [KwasantAuthorize(Roles = "Admin")]
        public ActionResult Edit(int bookingRequestId, int clarificationRequestId = 0)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var cr = new ClarificationRequest();

                try
                {
                    NegotiationDO negotiationDO = uow.NegotiationsRepository.FindOne(br => br.BookingRequestID == bookingRequestId);
                    var curClarificationRequestDO = cr.GetOrCreateClarificationRequest(uow, bookingRequestId, clarificationRequestId, negotiationDO.Id);
                    return View(Mapper.Map<ClarificationRequestViewModel>(curClarificationRequestDO));
                }
                catch (EntityNotFoundException<IBookingRequest>)
                {
                    return HttpNotFound("Booking request not found.");
                }
            }
        }


        //[KwasantAuthorize(Roles = "Admin")]
        //[HttpPost]
        //public ActionResult Send(ClarificationRequestViewModel viewModel)
        //{
        //    var submittedClarificationRequestDO = Mapper.Map<ClarificationRequestDO>(viewModel);
        //    using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
        //    {
        //        var cr = new ClarificationRequest();
        //        try
        //        {
        //            NegotiationDO negotiationDO = uow.NegotiationsRepository.FindOne(br => br.RequestId == submittedClarificationRequestDO.BookingRequestId);
        //            var curClarificationRequestDO = cr.GetOrCreateClarificationRequest(uow, submittedClarificationRequestDO.BookingRequestId, submittedClarificationRequestDO.Id, negotiationDO.Id);
        //            cr.UpdateClarificationRequest(uow, curClarificationRequestDO, submittedClarificationRequestDO);
        //            var responseUrlFormat = string.Concat(Url.Action("", RouteConfig.ShowClarificationResponseUrl, new { }, this.Request.Url.Scheme), "?{0}");
        //            var responseUrl = cr.GenerateResponseURL(curClarificationRequestDO, responseUrlFormat);
        //            cr.Send(uow, curClarificationRequestDO, responseUrl); 
        //            uow.SaveChanges();
        //            return Json(new { success = true });
        //        }
        //        catch (EntityNotFoundException ex)
        //        {
        //            return HttpNotFound(ex.Message);
        //        }
        //        catch (Exception ex)
        //        {
        //            return Json(new { success = false, error = ex.Message });
        //        }
        //    }
        //}

        [RequestParamsEncryptedFilter]
        public ActionResult ShowClarificationResponse(long id) //here we need to get emailDo id...or NegotiationId
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                //var curClarificationRequestDO = uow.ClarificationRequestRepository.GetByKey(id);
                //if (curClarificationRequestDO == null)
                //    return HttpNotFound("Clarification request not found.");
                //if (curClarificationRequestDO.BookingRequest == null)
                //    return HttpNotFound("Booking request not found.");
                if (uow.QuestionRepository.GetAll().Where(e => e.ClarificationRequestId == id && e.Status == QuestionStatus.Unanswered).Count() == 0)
                    return View("~/Views/ClarificationResponse/AllAnswered.cshtml");
                //var curClarificationResponseViewModel = Mapper.Map<ClarificationRequestDO, ClarificationResponseViewModel>(curClarificationRequestDO);
                //return View("~/Views/ClarificationResponse/New.cshtml", curClarificationResponseViewModel);


                var NegotiationQuestions = uow.QuestionRepository.GetAll().Where(e => e.ClarificationRequestId == id && e.Status == QuestionStatus.Unanswered).Select(s => new NegotiationQuestionViewModel
                {
                    RequestId = s.ClarificationRequestId,
                    Id = s.Id,
                    Response = s.Response,
                    Status = s.Status,
                    Text = s.Text,

                    Answers = uow.AnswerRepository.GetAll().Where(ans => ans.QuestionID == s.Id).Select(anss => new NegotiationAnswerViewModel
                    {
                        Id = anss.Id,
                        ObjectType = anss.ObjectsType,
                        QuestionId = anss.QuestionID,
                        Status = anss.Status,
                        UserId = anss.User.Id
                    }).ToList()
                }).ToList();

                NegotiationQuestions.AddRange(NegotiationQuestions);

                return View("~/Views/ClarificationResponse/New.cshtml", NegotiationQuestions);
            }
        }

        public ActionResult ProcessResponse(string answerArray)
        {
            JavaScriptSerializer json_serializer = new JavaScriptSerializer();
            string[] arr = json_serializer.Deserialize<string[]>(answerArray);
            //string[] ans


            //Generates Error for now as we need to get user and assign it to answers...
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                foreach (string ans in arr)
                {
                    if (!ans.Contains("~"))
                    {
                        AnswerDO answer = uow.AnswerRepository.GetByKey(Convert.ToInt32(ans));
                        answer.Status = "Selected";
    }
                    else {
                        string questionId = ans.Split(new char[] { '~' })[0];
                        string suggestedAnswer = ans.Split(new char[] { '~' })[1];
                        uow.AnswerRepository.Add(new AnswerDO() { 
                        ObjectsType = suggestedAnswer,
                        Question = uow.QuestionRepository.GetByKey(Convert.ToInt32(questionId)),
                        QuestionID = Convert.ToInt32(questionId),
                        Status= "Selected"
                        //, User = 
                        });
                    }
                }
                uow.SaveChanges();
            }

            return Content("Success");
        }
    }
}