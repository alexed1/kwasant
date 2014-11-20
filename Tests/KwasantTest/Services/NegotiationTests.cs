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
    class NegotiationTests : BaseTest
    {

        
        [Test]
        public void Negotiation_Update_CanUpdateNegotiation()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var fixture = new FixtureData(uow);
                var objNegotiation = new Negotiation();
                var curNegotiationDO = fixture.TestNegotiation1();
                var subNegotiationDO = fixture.TestNegotiation2();
                uow.NegotiationsRepository.Add(curNegotiationDO);
                uow.SaveChanges();
                objNegotiation.Update(uow, subNegotiationDO);
                uow.SaveChanges();
                var updatedNegotiationDO =uow.NegotiationsRepository.GetByKey(subNegotiationDO.Id);
                Assert.AreEqual(subNegotiationDO.Name, updatedNegotiationDO.Name);
                Assert.AreEqual(subNegotiationDO.BookingRequestID, updatedNegotiationDO.BookingRequestID);

            }
        }

       
    }
}
