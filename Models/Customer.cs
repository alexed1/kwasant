using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Liki.App.Services.AlertManager;
using Shnexy.DataAccessLayer;
using Shnexy.DataAccessLayer.Interfaces;
using Shnexy.DataAccessLayer.Repositories;

namespace Shnexy.Models
{
    public class Customer :  ICustomer
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public EmailAddress emailAddr { get; set; }


        private ICustomerRepository _customerRepo ;

        //parameterless constructor seems to be required by EF. But we should not invoke it manually because we need to inject a repo
       
        public Customer(ICustomerRepository customerRepo)

        {
            _customerRepo = customerRepo;
        }

        //public Customer()
        //{
            
        //}
        public void Add()
        {
            _customerRepo.Add(this);

            //probably need to test to see if this is a new customer or not, or we'll send too many welcome letters
            AlertManager.CustomerCreated(
                String.Format("name=CustomerCreated, createdate=" + DateTime.Now + ", CustomerId=" + this.Id));
        }

        public Customer GetByKey(int Id)
        {
            
            return _customerRepo.GetByKey(Id);
        }

    }
}