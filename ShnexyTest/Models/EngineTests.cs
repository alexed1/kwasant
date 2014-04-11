using Data.DataAccessLayer.Interfaces;
using Data.Models;
using Moq;
using NUnit.Framework;

namespace ShnexyTest.Models
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
       
    }
}
