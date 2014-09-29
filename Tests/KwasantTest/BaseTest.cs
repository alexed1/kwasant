using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Data.Infrastructure.StructureMap;
using Data.Interfaces;
using KwasantCore.StructureMap;
using KwasantWeb.App_Start;
using NUnit.Framework;
using StructureMap;

namespace KwasantTest
{
    [TestFixture]
    public class BaseTest
    {
        [SetUp]
        public virtual void SetUp()
        {
            StructureMapBootStrapper.ConfigureDependencies(StructureMapBootStrapper.DependencyType.TEST);
            MockedDBContext.WipeMockedDatabase();
            AutoMapperBootStrapper.ConfigureAutoMapper();
            
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>()) //Get the seeding done first
                uow.SaveChanges();
        }

        [Test]
        public void AssertAllTestsImplementBaseTest()
        {
            var failedTypes = new List<Type>();
            foreach (var testClass in GetType().Assembly.GetTypes().Where(t => t.GetCustomAttributes<TestFixtureAttribute>().Any()))
            {
                if (testClass != typeof(BaseTest) && !testClass.IsSubclassOf(typeof(BaseTest)))
                    failedTypes.Add(testClass);
            }
            var exceptionMessages = new List<String>();
            foreach (var failedType in failedTypes)
            {
                var testClassName = failedType.Name;
                exceptionMessages.Add(testClassName + " must implement 'BaseTest'");
            }
            if (exceptionMessages.Any())
                Assert.Fail(String.Join(Environment.NewLine, exceptionMessages));
        }
    }
}
