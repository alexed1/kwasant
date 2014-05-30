using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Reflection;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Data.Entity.Validation;
using System.Text;

using Data.Constants;
using Data.Entities;
using Data.Infrastructure;
using Data.Interfaces;
using StructureMap;

namespace Data.Migrations
{
    public sealed class MigrationConfiguration : DbMigrationsConfiguration<KwasantDbContext>
    {
        public MigrationConfiguration()
        {
            AutomaticMigrationsEnabled = false;
            ContextKey = "Data.Infrastructure.KwasantDbContext";
        }

        protected override void Seed(KwasantDbContext context)
        {
            //  This method will be called after migrating to the latest version.

            /* Be sure to use AddOrUpdate when creating seed data - otherwise we will get duplicates! */

            //This is not a mistake that we're using new UnitOfWork, rather than calling the ObjectFactory. 
            //The object factory decides what context to use, based on the environment.
            //In this situation, we need to be sure to use the provided context.

            //This class is _not_ mockable - it's a core part of EF. Some seeding, however, is mockable (see the static function Seed and how MockedDBContext uses it).
            var unitOfWork = new UnitOfWork(context);
            Seed(unitOfWork);

            AddRoles(unitOfWork);
            AddAdmins(unitOfWork);

            unitOfWork.SaveChanges();
        }

        //Method to let us seed into memory as well
        public static void Seed(IUnitOfWork context)
        {
            SeedInstructions(context);
        }

        private static void SeedInstructions(IUnitOfWork unitOfWork)
        {
            Type[] nestedTypes = typeof (InstructionConstants).GetNestedTypes();
            var instructionsToAdd = new List<InstructionDO>();
            foreach (Type nestedType in nestedTypes)
            {
                FieldInfo[] constants = nestedType.GetFields();
                foreach (FieldInfo constant in constants)
                {
                    string name = constant.Name;
                    object value = constant.GetValue(null);
                    instructionsToAdd.Add(new InstructionDO
                    {
                        Id = (int) value,
                        Name = name,
                        Category = nestedType.Name
                    });
                }
            }

            unitOfWork.InstructionRepository.DBSet.AddOrUpdate(
                    i => i.Id,
                    instructionsToAdd.ToArray()
                );
        }

        /// <summary>
        /// Add roles of type 'Admin' and 'Customer' in DB
        /// </summary>
        /// <param name="unitOfWork"></param>
        private void AddRoles(IUnitOfWork unitOfWork)
        {
            var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(unitOfWork.Db as KwasantDbContext));

            if (roleManager.RoleExists("Admin") == false)
            {
                roleManager.Create(new IdentityRole("Admin"));
            }

            if (roleManager.RoleExists("Customer") == false)
            {
                roleManager.Create(new IdentityRole("Customer"));
            }
        }

        /// <summary>
        /// Add 'Admin' roles. Curretly only user with Email "alex@kwasant.com" and password 'alex@1234'
        /// has been added.
        /// </summary>
        /// <param name="unitOfWork">of type ShnexyDbContext</param>
        /// <returns>True if created successfully otherwise false</returns>
        private void AddAdmins(IUnitOfWork unitOfWork)
        {
            CreateAdmin("alex@kwasant.com", "alex@1234", unitOfWork);
            CreateAdmin("pabitra@hotmail.com", "pabi1234", unitOfWork);
            CreateAdmin("rjrudman@gmail.com", "robert1234", unitOfWork);
            CreateAdmin("quader.mamun@gmail.com", "abdul1234", unitOfWork);
        }

        /// <summary>
        /// Craete a user with role 'Admin'
        /// </summary>
        /// <param name="curUserName"></param>
        /// <param name="curPassword"></param>
        /// <param name="unitOfWork"></param>
        /// <returns></returns>
        private void CreateAdmin(string curUserName, string curPassword, IUnitOfWork unitOfWork)
        {
            try
            {
                var um = new UserManager<UserDO>(new UserStore<UserDO>(unitOfWork.Db as KwasantDbContext));
                if (um.FindByName(curUserName) == null)
                {
                    var user = new UserDO
                    {
                        UserName = curUserName,
                        EmailAddress = unitOfWork.EmailAddressRepository.GetOrCreateEmailAddress(curUserName),    
                        FirstName = curUserName,
                        EmailConfirmed = true
                    };

                    IdentityResult ir = um.Create(user, curPassword);
                    if (!ir.Succeeded)
                        return;

                    um.AddToRole(user.Id, "Admin");
                }
            }
            catch (DbEntityValidationException ex)
            {
                var sb = new StringBuilder();

                foreach (var failure in ex.EntityValidationErrors)
                {
                    sb.AppendFormat("{0} failed validation\n", failure.Entry.Entity.GetType());
                    foreach (var error in failure.ValidationErrors)
                    {
                        sb.AppendFormat("- {0} : {1}", error.PropertyName, error.ErrorMessage);
                        sb.AppendLine();
                    }
                }

                throw new DbEntityValidationException("Entity Validation Failed - errors follow:\n" + sb, ex); // Add the original exception as the innerException
            }
        }
    }
}
