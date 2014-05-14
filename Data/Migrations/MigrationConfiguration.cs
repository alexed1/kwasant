using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.Reflection;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

using Data.Constants;
using Data.Entities;
using Data.Infrastructure;
using Data.Interfaces;

namespace Data.Migrations
{
    public sealed class MigrationConfiguration : DbMigrationsConfiguration<KwasantDbContext>
    {
        public MigrationConfiguration()
        {
            AutomaticMigrationsEnabled = true;
            ContextKey = "Data.Infrastructure.KwasantDbContext";
        }

        protected override void Seed(KwasantDbContext context)
        {
            //  This method will be called after migrating to the latest version.

            /* Be sure to use AddOrUpdate when creating seed data - otherwise we will get duplicates! */

            Seed(context);

            AddRoles(context);
            AddAdmins(context);
        }

        //Method to let us seed into memory as well
        public static void Seed(IDBContext context)
        {
            SeedInstructions(context);
        }

        private static void SeedInstructions(IDBContext context)
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
                        InstructionID = (int) value,
                        Name = name,
                        Category = nestedType.Name
                    });
                }
            }

            context.Instructions.AddOrUpdate(
                    i => i.InstructionID,
                    instructionsToAdd.ToArray()
                );
        }

        /// <summary>
        /// Add roles of type 'Admin' and 'Customer' in DB
        /// </summary>
        /// <param name="context"></param>
        private void AddRoles(KwasantDbContext context)
        {
            var RoleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(context));

            IdentityResult roleresult;
            if (RoleManager.RoleExists("Admin") == false)
            {
                roleresult = RoleManager.Create(new IdentityRole("Admin"));
            }

            if (RoleManager.RoleExists("Customer") == false)
            {
                roleresult = RoleManager.Create(new IdentityRole("Customer"));
            }
        }

        /// <summary>
        /// Add 'Admin' roles. Curretly only user with Email "alex@kwasant.com" and password 'alex@1234'
        /// has been added.
        /// </summary>
        /// <param name="context">of type ShnexyDbContext</param>
        /// <returns>True if created successfully otherwise false</returns>
        private void AddAdmins(KwasantDbContext context)
        {
            CreateAdmin("alex@kwasant.com", "alex@1234", context);
            CreateAdmin("pabitra@hotmail.com", "pabi1234", context);
            CreateAdmin("rjrudman@gmail.com", "robert1234", context);
            CreateAdmin("quader.mamun@gmail.com", "abdul1234", context);
        }

        /// <summary>
        /// Craete a user with role 'Admin'
        /// </summary>
        /// <param name="curUserName"></param>
        /// <param name="curPassword"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        private bool CreateAdmin(string curUserName, string curPassword, KwasantDbContext context)
        {
            IdentityResult ir;
            var rm = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(context));

            var um = new UserManager<UserDO>(new UserStore<UserDO>(context));

            var user = new UserDO()
            {
                UserName = curUserName,
                Email = curUserName,
                EmailConfirmed = true
            };

            ir = um.Create(user, curPassword);
            if (ir.Succeeded == false)
                return ir.Succeeded;

            ir = um.AddToRole(user.Id, "Admin");

            return ir.Succeeded;
        }
    }
}
