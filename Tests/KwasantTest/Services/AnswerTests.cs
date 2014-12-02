using System.Collections.Generic;
using Data.Entities;
using Data.Interfaces;
using Data.States;
using FluentValidation;
using KwasantCore.Interfaces;
using KwasantCore.Services;
using KwasantTest.Fixtures;
using NUnit.Framework;
using StructureMap;
using System.Linq;
using Data.Repositories;
using System;

namespace KwasantTest.Services
{
    [TestFixture]
    class AnswerTests : BaseTest
    {
        [Test]
        public void Answer_Update_CanGenerateNullException()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var fixture = new FixtureData(uow);
                var _answer = new Answer();
                var curAnswerDO = fixture.TestAnswer4();
                uow.AnswerRepository.Add(curAnswerDO);
                uow.SaveChanges();
                Assert.Throws<NullReferenceException>(() =>
                {
                    _answer.Update(uow, null);
                });

            }
        }

        [Test]
        public void Answer_Update_CanUpdateAnswer()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var fixture = new FixtureData(uow);
                var _answer = new Answer();
                var curAnswerDO = fixture.TestAnswer4();
                var submittedAnsData = fixture.TestAnswer5();
                uow.AnswerRepository.Add(curAnswerDO);
                uow.SaveChanges();
                _answer.Update(uow, submittedAnsData);
                uow.SaveChanges();
                var retrievedAnswerDO = uow.AnswerRepository.GetByKey(submittedAnsData.Id);
                Assert.AreEqual(submittedAnsData.Text, retrievedAnswerDO.Text);

            }
        }
    }
}
