using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;
using Shnexy.DataAccessLayer.Interfaces;
using Shnexy.Utilities;
using Shnexy.DataAccessLayer;

namespace Shnexy.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string foobar { get; set; }
      

       


        private IUnitOfWork _uow;
        private ShnexyDbContext db;


        public User(IUnitOfWork uow)
        {
            _uow = uow;
            db = (ShnexyDbContext)uow.Db; //don't know why we have to have an explicit cast here
        }



        public User ()
        {
           

        }

        public User Get(int userId)
        {
            return db.Users.Find(userId);

        }

    }



}