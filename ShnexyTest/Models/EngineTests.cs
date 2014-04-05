using System;
using Data.DataAccessLayer.Interfaces;
using Data.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using NUnit.Framework;

namespace ShnexyTest
{
    [TestFixture]
    public class UnitTest1
    {
        private Mock<IEvent> mockEvent;

        [SetUp]
        public void Setup()
        {
           
            mockEvent = new Mock<IEvent>();
            
           
            
        }
        [Test]
        [Category("Engine Tests")]
        public void Engine_ProcessEvents_Works()
        {
            mockEvent.Setup(curEvent => curEvent.Dispatch());
            Engine curEngine = new Engine();
            curEngine.ProcessEvents(); //not useful, this test
        }

       
    }
}
