using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Data.Models;
using NUnit.Framework;
using Shnexy.DDay.iCal;

namespace ShnexyTest.Fixtures
{
    partial class FixtureData
    {




   
        public EmailAddress TestEmail1()
        {

            return new EmailAddress(emailAddressRepo)
            {
                Email = "alexlucre1@gmail.com",
                Id = 1,
                Name = "Alex Lucre1"
            };
        }

        public EmailAddress TestEmail2()
        {

            return new EmailAddress(emailAddressRepo)
            {
                Email = "joetest2@edelstein.org",
                Id = 2,
                Name = "Joe Test Account 2"
            };
        }

    }
}

