using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Data.Authentication;
using Data.Repositories;
using Data.States;
using Data.States.Templates;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Data.Entities;
using Data.Infrastructure;
using Data.Interfaces;
using Newtonsoft.Json;
using Utilities;

namespace Data.Migrations
{

    public sealed class MigrationConfiguration : DbMigrationsConfiguration<KwasantDbContext>
    {
        private readonly Account _account;
        private readonly IConfigRepository _configRepository;

        public MigrationConfiguration()
        {
            //Do not ever turn this on! It will break database upgrades
            AutomaticMigrationsEnabled = false;

            //Do not modify this, otherwise migrations will run twice!
            ContextKey = "Data.Infrastructure.KwasantDbContext";
            _account = new Account();
            _configRepository = new ConfigRepository();
        }

        protected override void Seed(KwasantDbContext context)
        {
            //  This method will be called after migrating to the latest version.

            /* Be sure to use AddOrUpdate when creating seed data - otherwise we will get duplicates! */

            //This is not a mistake that we're using new UnitOfWork, rather than calling the ObjectFactory. 
            //The object factory decides what context to use, based on the environment.
            //In this situation, we need to be sure to use the provided context.

            //This class is _not_ mockable - it's a core part of EF. Some seeding, however, is mockable (see the static function Seed and how MockedKwasantDbContext uses it).
            var unitOfWork = new UnitOfWork(context);
            Seed(unitOfWork);

            AddRoles(unitOfWork);
            AddAdmins(unitOfWork);
            AddCustomers(unitOfWork);
            AddBookingRequest(unitOfWork);

            SeedRemoteCalendarProviders(unitOfWork);

            AddCalendars(unitOfWork);
            AddEvents(unitOfWork);
        }

        //Method to let us seed into memory as well
        public static void Seed(IUnitOfWork context)
        {
            SeedConstants(context);

            SeedInstructions(context);
        }

        //This method will automatically seed any constants file
        //It looks for rows which implement IConstantRow<>
        //For example, BookingRequestStateRow implements IConstantRow<BookingRequestState>
        //The below method will then generate a new row for each constant found in BookingRequestState.
        private static void SeedConstants(IUnitOfWork context)
        {
            var constantsToSeed =
                typeof(MigrationConfiguration).Assembly.GetTypes()
                    .Select(t => new
                    {
                        RowType = t,
                        ConstantsType =
                            t.GetInterfaces()
                                .Where(i => i.IsGenericType)
                                .FirstOrDefault(i => i.GetGenericTypeDefinition() == typeof(IStateTemplate<>))
                    })
                    .Where(t => t.ConstantsType != null).ToList();

            foreach (var constantToSeed in constantsToSeed)
            {
                var rowType = constantToSeed.RowType;
                var constantType = constantToSeed.ConstantsType.GenericTypeArguments.First();

                var idParam = Expression.Parameter(typeof(int));
                var nameParam = Expression.Parameter(typeof(String));

                //The below uses the .NET Expression builder to construct this:
                // (id, name) => new [rowType] { Id = id, Name = name };
                //We need to build it with the expression builder, as we don't know what type to construct yet, and the method requires type arguments.

                //We can't build constructor intialization with Expressions, so this is the logic for it:
                // 1, we create a variable called 'constructedRowType'. This variable is used within the expressions
                // Note that there are _two_ variables. The first one being a usual C# variable, pointing to an expression.
                // The second one is what's actually used within the block. Examining the expression block will show it as '$constructedRowType'

                //The code generated looks like this:
                // rowType generatedFunction(int id, string name) [for example, BookingRequestStateRow generatedFunction(int id, string name)
                // {
                //     rowType $constructedRowType; [for example, BookingRequestStateRow $constructedRowType]
                //     $constructedRowType = new rowType() [for example, $constructedRowType = new BookingRequestStateRow()]
                //     $constructedRowType.Id = id;
                //     $constructedRowType.Name = name;
                //} //Note that there is no return call. Whatever is last on the expression list will be kept at the top of the stack, acting like a 'return' 
                // (if you've worked with bytecode, it's the same idea).
                //We have 'constructedRowType' as the last expression, which tells us it will be returned

                var constructedRowType = Expression.Variable(rowType, "constructedRowType");
                var fullMethod = Expression.Block(
                new[] { constructedRowType },
                Expression.Assign(constructedRowType, Expression.New(rowType)),
                Expression.Assign(Expression.Property(constructedRowType, "Id"), idParam),
                Expression.Assign(Expression.Property(constructedRowType, "Name"), nameParam),
                constructedRowType);

                //Now we take that expression and compile it. It's still typed as a 'Delegate', but it is now castable to Func<int, string, TConstantDO>
                //For example, it could be Func<int, string, BookingRequestStateRow>

                var compiledExpression = Expression.Lambda(fullMethod, new[] { idParam, nameParam }).Compile();

                //Now we have our expression, we need to call something similar to this:
                //SeedConstants<constantType, rowType>(context, compiledExpression)

                var seedMethod = typeof(MigrationConfiguration)
                    .GetMethods(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static)
                    .FirstOrDefault(m => m.Name == "SeedConstants" && m.IsGenericMethod)
                    .MakeGenericMethod(constantType, rowType);

                seedMethod.Invoke(null, new object[] { context, compiledExpression });
            }
        }

        private void SeedRemoteCalendarProviders(IUnitOfWork uow)
        {
            var providers = new[]
                                {
                                    new RemoteCalendarProviderDO()
                                        {
                                            Name = "Google",
                                            AuthType = ServiceAuthorizationType.OAuth2,
                                            AppCreds = JsonConvert.SerializeObject(
                                                new
                                                    {
                                                        ClientId = _configRepository.Get("GoogleCalendarClientId"),
                                                        ClientSecret = _configRepository.Get("GoogleCalendarClientSecret"),
                                                        Scopes = "https://www.googleapis.com/auth/calendar"
                                                    }),
                                            CalDAVEndPoint = "https://apidata.googleusercontent.com/caldav/v2"
                                        },
                                };
            foreach (var provider in providers)
            {
                if (uow.RemoteCalendarProviderRepository.GetByName(provider.Name) == null)
                    uow.RemoteCalendarProviderRepository.Add(provider);
            }
        }

        //Do not remove. Resharper says it's not in use, but it's being used via reflection
        // ReSharper disable UnusedMember.Local
        private static void SeedConstants<TConstantsType, TConstantDO>(IUnitOfWork uow, Func<int, string, TConstantDO> creatorFunc)
            // ReSharper restore UnusedMember.Local
            where TConstantDO : class, IStateTemplate<TConstantsType>
        {
            var instructionsToAdd = new List<TConstantDO>();

            FieldInfo[] constants = typeof(TConstantsType).GetFields();
            foreach (FieldInfo constant in constants)
            {
                string name = constant.Name;
                object value = constant.GetValue(null);
                instructionsToAdd.Add(creatorFunc((int)value, name));
            }
            //First, we find rows in the DB that don't exist in our seeding. We delete those.
            //Then, we find rows in our seeding that don't exist in the DB. We create those ones (or update the name).

            var repo = new GenericRepository<TConstantDO>(uow);
            var allRows = new GenericRepository<TConstantDO>(uow).GetAll().ToList();
            foreach (var row in allRows)
            {
                if (!instructionsToAdd.Select(i => i.Id).Contains(row.Id))
                {
                    repo.Remove(row);
                }
            }
            foreach (var row in instructionsToAdd)
            {
                var matchingRow = allRows.FirstOrDefault(r => r.Id == row.Id);
                if (matchingRow == null)
                {
                    matchingRow = row;
                    repo.Add(matchingRow);
                }
                matchingRow.Id = row.Id;
                matchingRow.Name = row.Name;
            }
        }

        private static void SeedInstructions(IUnitOfWork unitOfWork)
        {
            Type[] nestedTypes = typeof(InstructionConstants).GetNestedTypes();
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
                        Id = (int)value,
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

            var roles = roleManager.Roles.ToList();
            if (roleManager.RoleExists("Admin") == false)
            {
                roleManager.Create(new IdentityRole("Admin"));
            }

            if (roleManager.RoleExists("Customer") == false)
            {
                roleManager.Create(new IdentityRole("Customer"));
            }

            if (roleManager.RoleExists("Booker") == false)
            {
                roleManager.Create(new IdentityRole("Booker"));
            }
        }

        /// <summary>
        /// Add users with 'Admin' role.
        /// </summary>
        /// <param name="unitOfWork">of type ShnexyKwasantDbContext</param>
        /// <returns>True if created successfully otherwise false</returns>
        private void AddAdmins(IUnitOfWork unitOfWork)
        {
            CreateAdmin("alex@kwasant.com", "alex@1234", unitOfWork);
            CreateAdmin("pabitra@hotmail.com", "pabi1234", unitOfWork);
            CreateAdmin("rjrudman@gmail.com", "robert1234", unitOfWork);
            CreateAdmin("quader.mamun@gmail.com", "abdul1234", unitOfWork);
            CreateAdmin("mkostyrkin@gmail.com", "mk@1234", unitOfWork);
        }

        /// <summary>
        /// Add users with 'Admin' role.
        /// </summary>
        /// <param name="unitOfWork">of type ShnexyKwasantDbContext</param>
        /// <returns>True if created successfully otherwise false</returns>
        private void AddCustomers(IUnitOfWork unitOfWork)
        {
            CreateCustomer("alexlucre1@gmail.com", "lucrelucre", unitOfWork);
        }

        /// <summary>
        /// Craete a user with role 'Admin'
        /// </summary>
        /// <param name="curUserName"></param>
        /// <param name="curPassword"></param>
        /// <param name="unitOfWork"></param>
        /// <returns></returns>
        private void CreateAdmin(string curUserName, string curPassword, IUnitOfWork uow)
        {

            string userId = _account.Create(curUserName, curPassword, uow);
            _account.AddRole("Admin", userId, uow);
            _account.AddRole("Customer", userId, uow);
        }

        /// <summary>
        /// Craete a user with role 'Customer'
        /// </summary>
        /// <param name="curUserName"></param>
        /// <param name="curPassword"></param>
        /// <param name="unitOfWork"></param>
        /// <returns></returns>
        private void CreateCustomer(string curUserName, string curPassword, IUnitOfWork uow)
        {
            string userId = _account.Create(curUserName, curPassword, uow);
            _account.AddRole("Customer", userId, uow);
        }

        private void AddBookingRequest(IUnitOfWork unitOfWork)
        {
            if (unitOfWork.BookingRequestRepository.GetQuery().Count() == 0)
            {
                CreateBookingRequest("alexlucre1@gmail.com", "First Booking request subject", "First Booking request text", unitOfWork);
                CreateBookingRequest("alexlucre1@gmail.com", "Second Booking request subject", "Second Booking request text", unitOfWork);
            }
        }

        private void CreateBookingRequest(string curUserName, string subject, string htmlText, IUnitOfWork uow)
        {
            var curBookingRequestDO = new BookingRequestDO
            {
                DateCreated = DateTimeOffset.UtcNow,
                From = uow.EmailAddressRepository.GetOrCreateEmailAddress(curUserName),
                Subject = subject,
                HTMLText = htmlText,
                EmailStatus = EmailState.Unprocessed,
                DateReceived = DateTimeOffset.UtcNow,
                State = BookingRequestState.Unstarted,
                User = new UserManager<UserDO>(new UserStore<UserDO>(uow.Db as KwasantDbContext)).FindByName(curUserName)
            };
            foreach (var calendar in curBookingRequestDO.User.Calendars)
                curBookingRequestDO.Calendars.Add(calendar);
            uow.BookingRequestRepository.Add(curBookingRequestDO);
        }

        private void AddCalendars(IUnitOfWork uow)
        {
            if (!uow.CalendarRepository.GetAll().Where(e => e.Name == "Test Calendar 1").Any())
            {
                CreateCalendars("Test Calendar 1", "alexlucre1@gmail.com", uow);
                CreateCalendars("Test Calendar 2", "alexlucre1@gmail.com", uow);
            }
        }

        private void CreateCalendars(string calendarName,string curUserEmail, IUnitOfWork uow) 
        {
            UserDO curUser = uow.UserRepository.FindOne(e => e.EmailAddress.Address == curUserEmail);
            var curCalendar = new CalendarDO
            {
                Name = calendarName,
                Owner = curUser,
                OwnerID = curUser.Id
            };
            curUser.Calendars.Add(curCalendar);
        }

        private void AddEvents(IUnitOfWork uow)
        {
            if (!uow.EventRepository.GetAll().Where(e => e.Description == "Test Event 1").Any())
            {
                CreateEvents(uow, "alexlucre1@gmail.com", "Test Calendar 1");
                CreateEvents(uow, "alexlucre1@gmail.com", "Test Calendar 2");
            }
        }
        
        //Creating 10 events for each calendar
        private void CreateEvents(IUnitOfWork uow, string curUserEmail,string calendarName)
        {
            uow.SaveChanges();
            int bookingRequestID, calendarID;

            UserDO curUser = uow.UserRepository.FindOne(e => e.EmailAddress.Address == curUserEmail);
            bookingRequestID = uow.BookingRequestRepository.GetAll().Where(e => e.User.Id == curUser.Id).FirstOrDefault().Id;
            calendarID = curUser.Calendars.Where(e => e.Name == calendarName).FirstOrDefault().Id;


            for (int eventNumber = 1; eventNumber < 11; eventNumber++)
            {
                DateTimeOffset start = GetRandomEventStartTime();
                DateTimeOffset end = start.AddMinutes(new Random().Next(30, 120));
                EventDO createdEvent = new EventDO();
                createdEvent.BookingRequestID = bookingRequestID;
                createdEvent.CalendarID = calendarID;
                createdEvent.StartDate = start;
                createdEvent.EndDate = end;
                createdEvent.Description = "Test Event " + eventNumber.ToString();
                createdEvent.Summary = "Test Event " + eventNumber.ToString();
                createdEvent.IsAllDay = false;
                createdEvent.CreatedBy = curUser;
                createdEvent.CreatedByID = curUser.Id;
                createdEvent.DateCreated = DateTimeOffset.UtcNow;
                createdEvent.EventStatus = EventState.Booking;
                uow.EventRepository.Add(createdEvent);
            }
        }

        //Getting random working time within next 3 days
        private DateTimeOffset GetRandomEventStartTime()
        {
            TimeSpan timeSpan = DateTime.Now.AddDays(3) - DateTime.Now;
            var randomTest = new Random();
            TimeSpan newSpan = new TimeSpan(0, randomTest.Next(0, (int)timeSpan.TotalMinutes), 0);
            DateTime newDate = DateTime.Now + newSpan;
            while (newDate.TimeOfDay.Hours < 9)
            {
                newDate = newDate.Add(new TimeSpan(1, 0, 0));
            }
            while (newDate.TimeOfDay.Hours > 16)
            {
                newDate = newDate.Add(new TimeSpan(-1, 0, 0));
            }
            return newDate;
        }


    }
}

