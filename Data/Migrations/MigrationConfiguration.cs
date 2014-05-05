using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.Reflection;
using Data.Constants;
using Data.Entities;
using Data.Infrastructure;
using Data.Interfaces;

namespace Data.Migrations
{
    public sealed class MigrationConfiguration : DbMigrationsConfiguration<ShnexyDbContext>
    {
        public MigrationConfiguration()
        {
            AutomaticMigrationsEnabled = true;
            ContextKey = "Data.Infrastructure.ShnexyDbContext";
        }

        protected override void Seed(ShnexyDbContext context)
        {
            //  This method will be called after migrating to the latest version.

            /* Be sure to use AddOrUpdate when creating seed data - otherwise we will get duplicates! */

            Seed(context);
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
    }
}
