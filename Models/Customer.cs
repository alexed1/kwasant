using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Shnexy.DataAccessLayer;
using Shnexy.DataAccessLayer.Interfaces;
using Shnexy.DataAccessLayer.Repositories;
using Syncfusion.Mvc.Shared;

namespace Shnexy.Models
{
    public class Customer :  ICustomer
    {
        public int Id { get; set; }
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
        }

        public Customer GetByKey(int Id)
        {
            
            return _customerRepo.GetByKey(Id);
        }

    }
}