using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net.Mail;
using System.Web;
using Shnexy.Models;

namespace Shnexy.DataAccessLayer
{


    public class ShnexyDbContext : DbContext
    {


        static  ShnexyDbContext()
        {
            
        }

        public ShnexyDbContext() : base("ShnexyTestDbauto")
        {
           
        }

      
        

        public DbSet<Email> Emails { get; set; }

        public DbSet<Customer> Customers { get; set; }

        public DbSet<EmailAddress> EmailAddresses { get; set; }

        //public System.Data.Entity.DbSet<Shnexy.Models.AppointmentTable> AppointmentTables { get; set; }
        
        
      
    }

    //public class DevContext : ShnexyDbContext
    //{
    //    public DevContext() : base("localShnexyDb")
    //    {
            
    //    }

    //}
    //public class TestContext : ShnexyDbContext
    //{
    //    public TestContext()
    //        : base("ShnexyTestDb")
    //    {

    //    }

    

}