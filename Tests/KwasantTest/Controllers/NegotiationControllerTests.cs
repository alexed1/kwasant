using System;
using System.Collections.Generic;
using Data.Entities;
using Data.Interfaces;
using Data.States;
using KwasantTest.Fixtures;
using KwasantWeb.Controllers;
using KwasantWeb.ViewModels;
using NUnit.Framework;
using StructureMap;
using Utilities;

namespace KwasantTest.Controllers
{
    [TestFixture]
    public class NegotiationControllerTests : BaseTest
    {
        [Test]
        public void Negotiation_SubmittedForm_CanGenerateNullException()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var curNegotiationController = new NegotiationController();
                Assert.Throws<NullReferenceException>(() =>
                {
                    curNegotiationController.ProcessSubmittedForm(null); 
                });              
                
            }

        }


        [Test]
        public void Negotiation_SubmittedForm_CanAddNewNegotiation()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var fixture = new FixtureData(uow);
                var curNegotiationDO = fixture.TestNegotiation6();
                uow.NegotiationsRepository.Add(curNegotiationDO);
                uow.SaveChanges();

                List<NegotiationQuestionVM> questionsListVM = new List<NegotiationQuestionVM>();
                List<NegotiationAnswerVM> answersListVM = new List<NegotiationAnswerVM>();

                 answersListVM.Add(new NegotiationAnswerVM{
                     Id = curNegotiationDO.Questions[0].Answers[0].Id,
                     Text = curNegotiationDO.Questions[0].Answers[0].Text,
                     AnswerState = curNegotiationDO.Questions[0].Answers[0].AnswerStatus
                 }
                 );
                
                questionsListVM.Add(new NegotiationQuestionVM { 
                    Id=curNegotiationDO.Questions[0].Id,
                    Text = curNegotiationDO.Questions[0].Text,
                    Type = curNegotiationDO.Questions[0].AnswerType,
                    Answers=answersListVM
                });

                var curNegotiationController = new NegotiationController();
                var curNegotiationVM = new NegotiationVM
                {
                    BookingRequestID = curNegotiationDO.BookingRequestID,
                    Name = curNegotiationDO.Name,
                    Questions = questionsListVM     

                };
                curNegotiationController.ProcessSubmittedForm(curNegotiationVM); 

            }

        }

        [Test]
        public void Negotiation_SubmittedForm_CanUpateNegotiation()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var fixture = new FixtureData(uow);
                var curNegotiationDO = fixture.TestNegotiation1();
                uow.NegotiationsRepository.Add(curNegotiationDO);
                uow.SaveChanges();

                List<NegotiationQuestionVM> questionsListVM = new List<NegotiationQuestionVM>();
                List<NegotiationAnswerVM> answersListVM = new List<NegotiationAnswerVM>();

                answersListVM.Add(new NegotiationAnswerVM
                {
                    Id = curNegotiationDO.Questions[0].Answers[0].Id,
                    Text = curNegotiationDO.Questions[0].Answers[0].Text,
                    AnswerState = curNegotiationDO.Questions[0].Answers[0].AnswerStatus
                }
                );

                questionsListVM.Add(new NegotiationQuestionVM
                {
                    Id = curNegotiationDO.Questions[0].Id,
                    Text = curNegotiationDO.Questions[0].Text,
                    Type = curNegotiationDO.Questions[0].AnswerType,
                    Answers = answersListVM
                });

                var curNegotiationController = new NegotiationController();
                var curNegotiationVM = new NegotiationVM
                {
                    Id=curNegotiationDO.Id,
                    BookingRequestID = curNegotiationDO.BookingRequestID,
                    Name = curNegotiationDO.Name,
                    Questions = questionsListVM

                };
                curNegotiationController.ProcessSubmittedForm(curNegotiationVM);

            }

        }
       
    }
}
