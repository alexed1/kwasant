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
        public EmailAddress email { get; set; }

        private ICustomerRepository customerRepo ;

        public Customer(ICustomerRepository customerRepo)
            
        {

        }

        public void Add()
        {
            customerRepo.Add(this); 
        }

        public Customer GetByKey(int Id)
        {
            return customerRepo.GetByKey(Id);
        }

    }
}