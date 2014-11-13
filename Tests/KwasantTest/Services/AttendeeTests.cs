using System;
using System.Linq;
using Data.Entities;
using Data.Interfaces;
using KwasantCore.Interfaces;
using NUnit.Framework;
using StructureMap;

namespace KwasantTest.Services
{
    [TestFixture]
    public class AttendeeTests : BaseTest
    {
        [Test]
        public void TestBasicEmail()
        {
            var emailAddress = ObjectFactory.GetInstance<IEmailAddress>();
            var result = emailAddress.ExtractFromString("rjrudman@gmail.com");

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(String.Empty, result[0].Name);
            Assert.AreEqual("rjrudman@gmail.com", result[0].Email);
        }

        [Test]
        public void TestMultipleBasicEmails()
        {
            var emailAddress = ObjectFactory.GetInstance<IEmailAddress>();
            var result = emailAddress.ExtractFromString("rjrudman@gmail.com,otheremail@gmail.com");

            Assert.AreEqual(2, result.Count);

            Assert.AreEqual(String.Empty, result[0].Name);
            Assert.AreEqual("rjrudman@gmail.com", result[0].Email);

            Assert.AreEqual(String.Empty, result[1].Name);
            Assert.AreEqual("otheremail@gmail.com", result[1].Email);
        }

        [Test]
        public void TestEmailWithName()
        {
            var emailAddress = ObjectFactory.GetInstance<IEmailAddress>();
            var result = emailAddress.ExtractFromString("<Robert>rjrudman@gmail.com");

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("Robert", result[0].Name);
            Assert.AreEqual("rjrudman@gmail.com", result[0].Email);
        }

        [Test]
        public void TestEmailWithFullName()
        {
            var emailAddress = ObjectFactory.GetInstance<IEmailAddress>();
            var result = emailAddress.ExtractFromString("<Robert Robert>rjrudman@gmail.com");

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("Robert Robert", result[0].Name);
            Assert.AreEqual("rjrudman@gmail.com", result[0].Email);
        }

        [Test]
        public void TestNameWithNumbers()
        {
            var emailAddress = ObjectFactory.GetInstance<IEmailAddress>();
            var result = emailAddress.ExtractFromString("<Robert23>rjrudman@gmail.com");

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("Robert23", result[0].Name);
            Assert.AreEqual("rjrudman@gmail.com", result[0].Email);
        }

        [Test]
        public void TestEmailNameWithNumbers()
        {
            var emailAddress = ObjectFactory.GetInstance<IEmailAddress>();
            var result = emailAddress.ExtractFromString("rjrudman23@gmail.com");

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(String.Empty, result[0].Name);
            Assert.AreEqual("rjrudman23@gmail.com", result[0].Email);
        }

        [Test]
        public void TestDomainlNameWithNumbers()
        {
            var emailAddress = ObjectFactory.GetInstance<IEmailAddress>();
            var result = emailAddress.ExtractFromString("rjrudman@g23mail.com");

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(String.Empty, result[0].Name);
            Assert.AreEqual("rjrudman@g23mail.com", result[0].Email);
        }

        [Test]
        public void TestInvalidTLD_Short()
        {
            var emailAddress = ObjectFactory.GetInstance<IEmailAddress>();
            var result = emailAddress.ExtractFromString("rjrudman@gmail.c");

            Assert.AreEqual(0, result.Count);
        }

        [Test]
        public void TestComplexTLD()
        {
            var emailAddress = ObjectFactory.GetInstance<IEmailAddress>();
            //This is valid TLD - as per http://data.iana.org/TLD/tlds-alpha-by-domain.txt
            var result = emailAddress.ExtractFromString("rjrudman@gmail.XN--CLCHC0EA0B2G2A9GCD");

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(String.Empty, result[0].Name);
            Assert.AreEqual("rjrudman@gmail.xn--clchc0ea0b2g2a9gcd", result[0].Email);
        }

        [Test]
        public void TestEmailAddressDOCreated()
        {
            var emailAddress = ObjectFactory.GetInstance<IEmailAddress>();
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                Assert.AreEqual(0, uow.EmailAddressRepository.GetQuery().Count());
                emailAddress.GetEmailAddresses(uow, "rjrudman@gmail.com");
                uow.SaveChanges();
                Assert.AreEqual(1, uow.EmailAddressRepository.GetQuery().Count());
            }
        }

        [Test]
        public void TestEmailAddressDODuplicateNotCreated()
        {
            var emailAddress = ObjectFactory.GetInstance<IEmailAddress>();
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                Assert.AreEqual(0, uow.EmailAddressRepository.GetQuery().Count());
                uow.EmailAddressRepository.Add(new EmailAddressDO
                {
                    Address = "rjrudman@gmail.com",
                    Name = "rjrudman@gmail.com"
                });
                uow.SaveChanges();
                Assert.AreEqual(1, uow.EmailAddressRepository.GetQuery().Count());
                emailAddress.GetEmailAddresses(uow, "rjrudman@gmail.com");
                uow.SaveChanges();
                Assert.AreEqual(1, uow.EmailAddressRepository.GetQuery().Count());
            }
        }

        [Test]
        public void TestCorruptEmailNotParsed()
        {
            var emailAddress = ObjectFactory.GetInstance<IEmailAddress>();
            var result = emailAddress.ExtractFromString("hq@kwasant.comalex@edelstein.org");

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(String.Empty, result[0].Name);
            Assert.AreEqual("hq@kwasant.comalex", result[0].Email); //Technically a valid email. What we're really testing for, though, is that '@edelstein.org' is not parsed as a seperate email
        }

        [Test]
        public void TestDashInDomain()
        {
            var emailAddress = ObjectFactory.GetInstance<IEmailAddress>();
            var result = emailAddress.ExtractFromString("DGerrard@gerrard-cox.com");

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(String.Empty, result[0].Name);
            Assert.AreEqual("dgerrard@gerrard-cox.com", result[0].Email); 
        }
    }
}
