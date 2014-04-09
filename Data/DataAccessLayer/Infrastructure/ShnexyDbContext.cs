using System.Data.Entity;
using Data.Models;

namespace Data.DataAccessLayer.Infrastructure
{


    public class ShnexyDbContext : DbContext
    {

       
        //see web.config for connection string names.
        //azure is AzureDbContext
        public ShnexyDbContext()
            : base("name=AzureDbContext")
        {
            
        }

        //public ShnexyDbContext(string mode) : base(mode)
        //{
           
        //}

      
        

        public DbSet<Email> Emails { get; set; }

        public DbSet<Customer> Customers { get; set; }

        public DbSet<EmailAddress> EmailAddresses { get; set; }

        public DbSet<EventFile> EventFiles { get; set; }

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