using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Shnexy.DataAccessLayer;
using Shnexy.DataAccessLayer.Interfaces;
using Shnexy.DataAccessLayer.Repositories;

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



        public Customer GetByKey(int Id)
        {
            return customerRepo.GetByKey(Id);
        }

    }
}