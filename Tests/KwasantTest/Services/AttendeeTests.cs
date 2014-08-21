using System;
using KwasantCore.Services;
using NUnit.Framework;

namespace KwasantTest.Services
{
    [TestFixture]
    public class AttendeeTests
    {
        [Test]
        public void TestBasicEmail()
        {
            var att = new Attendee();
            var result = att.GetEmailAddresses("rjrudman@gmail.com");

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(String.Empty, result[0].Name);
            Assert.AreEqual("rjrudman@gmail.com", result[0].Email);
        }

        [Test]
        public void TestMultipleBasicEmails()
        {
            var att = new Attendee();
            var result = att.GetEmailAddresses("rjrudman@gmail.com,otheremail@gmail.com");

            Assert.AreEqual(2, result.Count);

            Assert.AreEqual(String.Empty, result[0].Name);
            Assert.AreEqual("rjrudman@gmail.com", result[0].Email);

            Assert.AreEqual(String.Empty, result[1].Name);
            Assert.AreEqual("otheremail@gmail.com", result[1].Email);
        }

        [Test]
        public void TestEmailWithName()
        {
            var att = new Attendee();
            var result = att.GetEmailAddresses("<Robert>rjrudman@gmail.com");

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("Robert", result[0].Name);
            Assert.AreEqual("rjrudman@gmail.com", result[0].Email);
        }

        [Test]
        public void TestEmailWithFullName()
        {
            var att = new Attendee();
            var result = att.GetEmailAddresses("<Robert Robert>rjrudman@gmail.com");

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("Robert Robert", result[0].Name);
            Assert.AreEqual("rjrudman@gmail.com", result[0].Email);
        }

        [Test]
        public void TestNameWithNumbers()
        {
            var att = new Attendee();
            var result = att.GetEmailAddresses("<Robert23>rjrudman@gmail.com");

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("Robert23", result[0].Name);
            Assert.AreEqual("rjrudman@gmail.com", result[0].Email);
        }

        [Test]
        public void TestEmailNameWithNumbers()
        {
            var att = new Attendee();
            var result = att.GetEmailAddresses("rjrudman23@gmail.com");

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(String.Empty, result[0].Name);
            Assert.AreEqual("rjrudman23@gmail.com", result[0].Email);
        }

        [Test]
        public void TestDomainlNameWithNumbers()
        {
            var att = new Attendee();
            var result = att.GetEmailAddresses("rjrudman@g23mail.com");

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(String.Empty, result[0].Name);
            Assert.AreEqual("rjrudman@g23mail.com", result[0].Email);
        }

        [Test]
        public void TestInvalidTLD_Short()
        {
            var att = new Attendee();
            var result = att.GetEmailAddresses("rjrudman@gmail.c");

            Assert.AreEqual(0, result.Count);
        }

        [Test]
        public void TestComplexTLD()
        {
            var att = new Attendee();
            //This is valid TLD - as per http://data.iana.org/TLD/tlds-alpha-by-domain.txt
            var result = att.GetEmailAddresses("rjrudman@gmail.XN--CLCHC0EA0B2G2A9GCD");

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(String.Empty, result[0].Name);
            Assert.AreEqual("rjrudman@gmail.XN--CLCHC0EA0B2G2A9GCD", result[0].Email);
        }

    }
}
