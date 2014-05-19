using System;
using System.Reflection;
using Data.Entities;
using Data.Interfaces;
using Data.Repositories;

namespace Data.Constants
{
    public class InstructionConstants
    {
        public class TravelTime
        {
            public const int Add30MinutesTravelTime = 1;
            public const int Add60MinutesTravelTime = 2;
            public const int Add90MinutesTravelTime = 3;
            public const int Add120MinutesTravelTime = 4;
        }

        public class EventDuration
        {
            public const int MarkAsAllDayEvent = 5;
        }


        //Generates seed data for instructions.
        //Instructions must be in a nested class to derive the category. If they're not in a nested class _they will not be seeded_.
        public static void ApplySeedData(IUnitOfWork uow)
        {
            Type[] nestedTypes = typeof(InstructionConstants).GetNestedTypes();

            InstructionRepository instructionRepo = new InstructionRepository(uow);
            foreach (Type nestedType in nestedTypes)
            {
                FieldInfo[] constants = nestedType.GetFields();
                foreach (FieldInfo constant in constants)
                {
                    string name = constant.Name;
                    object value = constant.GetValue(null);
                    instructionRepo.Add(new InstructionDO
                    {
                        InstructionID = (int)value,
                        Name = name,
                        Category = nestedType.Name
                    });
                }
            }
        }
    }
}
