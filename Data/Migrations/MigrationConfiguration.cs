using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Web.UI.WebControls;
using Data.Authentication;
using Data.Entities.Enumerations;
using Data.Repositories;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Data.Entity.Validation;
using Data.Constants;
using Data.Entities;
using Data.Infrastructure;
using Data.Interfaces;
using Newtonsoft.Json;
using StructureMap;
using Utilities;
using EventCreateType = Data.Entities.Enumerations.EventCreateType;
using EventSyncStatus = Data.Entities.Enumerations.EventSyncStatus;

namespace Data.Migrations
{
    
    public sealed class MigrationConfiguration : DbMigrationsConfiguration<KwasantDbContext>
    {
        private Account _account;
        public MigrationConfiguration()
        {
            //Do not ever turn this on! It will break database upgrades
            AutomaticMigrationsEnabled = false;

            //Do not modify this, otherwise migrations will run twice!
            ContextKey = "Data.Infrastructure.KwasantDbContext";
            _account = new Account();

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

            SeedRemoteCalendarProviders(unitOfWork);
        }

        //Method to let us seed into memory as well
        public static void Seed(IUnitOfWork context)
        {
            SeedConstants<EventState, EventStatus>(context, (id, name) => new EventStatus { Id = id, Name = name });
            SeedConstants<BRState, BookingRequestState>(context, (id, name) => new BookingRequestState { Id = id, Name = name });
            SeedConstants<CRState, ClarificationRequestState>(context, (id, name) => new ClarificationRequestState() { Id = id, Name = name });
            SeedConstants<Constants.EventCreateType, EventCreateType>(context, (id, name) => new EventCreateType { Id = id, Name = name });
            SeedConstants<Constants.EventSyncStatus, EventSyncStatus>(context, (id, name) => new EventSyncStatus { Id = id, Name = name });
            SeedConstants<ServiceAuthType, ServiceAuthorizationType>(context, (id, name) => new ServiceAuthorizationType() { Id = id, Name = name });
            SeedInstructions(context);
        }

        private static void SeedRemoteCalendarProviders(IUnitOfWork uow)
        {
            var providers = new []
                                {
                                    new RemoteCalendarProviderDO()
                                        {
                                            Name = "Google",
                                            AuthTypeID = ServiceAuthType.OAuth2,
                                            AppCreds = JsonConvert.SerializeObject(
                                                new
                                                    {
                                                        ClientId = ConfigRepository.Get("GoogleCalendarClientId"),
                                                        ClientSecret = ConfigRepository.Get("GoogleCalendarClientSecret"),
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

        private static void SeedConstants<TConstantsType, TConstantDO>(IUnitOfWork uow, Func<int, string, TConstantDO> creatorFunc)
            where TConstantDO : class
        {
            var instructionsToAdd = new List<TConstantDO>();

            FieldInfo[] constants = typeof(TConstantsType).GetFields();
            foreach (FieldInfo constant in constants)
            {
                string name = constant.Name;
                object value = constant.GetValue(null);
                instructionsToAdd.Add(creatorFunc((int)value, name));
            }
            var param = Expression.Parameter(typeof (TConstantDO));
            var exp = Expression.Lambda(Expression.Convert(Expression.Property(param, "Id"), typeof(object)), param) as Expression<Func<TConstantDO, object>>;

            var repo = new GenericRepository<TConstantDO>(uow);
            repo.DBSet.AddOrUpdate(
                    exp,
                    instructionsToAdd.ToArray()
            );
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



   
            
        }
    }

