using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Infrastructure;
using Data.Infrastructure;
using Data.Interfaces;
using Data.States;
using Data.States.Templates;
using StructureMap;
using Utilities;

namespace Data.Entities
{
    public class BookingRequestDO : EmailDO, ICreateHook, IModifyHook
    {
        public BookingRequestDO()
        {
            Calendars = new List<CalendarDO>();
            Negotiations = new List<NegotiationDO>();
            State = BookingRequestState.Unstarted;
        }

        [Required, ForeignKey("User")]
        public string UserID { get; set; }        
        public virtual UserDO User { get; set; }

        [Required, ForeignKey("BookingRequestStateTemplate")]
        public int? State { get; set; }
        public virtual _BookingRequestStateTemplate BookingRequestStateTemplate { get; set; }

        [ForeignKey("Booker")]
        public string BookerID { get; set; }
        public virtual UserDO Booker { get; set; }

        //Do not add InverseProperty - The relationship is handled in KwasantDbContext
        public virtual List<InstructionDO> Instructions { get; set; }

        [InverseProperty("BookingRequest")]
        public virtual IList<NegotiationDO> Negotiations { get; set; }

        [InverseProperty("BookingRequests")]
        public virtual List<CalendarDO> Calendars { get; set; }

        [InverseProperty("Conversation")]
        public virtual List<EmailDO> ConversationMembers { get; set; }
        
        public void AfterCreate()
        {
            AlertManager.BookingRequestCreated(Id);
        }

        public void OnModify(DbPropertyValues originalValues, DbPropertyValues currentValues)
        {
            var reflectionHelper = new ReflectionHelper<BookingRequestDO>();
            
            var userIDPropertyName = reflectionHelper.GetPropertyName(br => br.UserID);
            if (!MiscUtils.AreEqual(originalValues[userIDPropertyName], currentValues[userIDPropertyName]))
            {
                using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
                {
                    var newUser = uow.UserRepository.GetByKey(UserID);
                    var newUserName = newUser.UserName;
                    AlertManager.TrackablePropertyUpdated("User changed", "BookingRequest", Id, newUserName);    
                }
            }

            var statePropertyName = reflectionHelper.GetPropertyName(br => br.State);
            if (!MiscUtils.AreEqual(originalValues[statePropertyName], currentValues[statePropertyName]))
            {
                using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
                {
                    AlertManager.TrackablePropertyUpdated("State changed", "BookingRequest", Id, uow.BookingRequestStatusRepository.GetByKey(State).Name);
                }
            }

            var bookerPropertyName = reflectionHelper.GetPropertyName(br => br.BookerID);
            if (!MiscUtils.AreEqual(originalValues[bookerPropertyName], currentValues[bookerPropertyName]))
            {
                using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
                {
                    var newBooker = uow.UserRepository.GetByKey(BookerID);
                    var bookerName = newBooker == null ? "No-one" : newBooker.UserName;
                    AlertManager.TrackablePropertyUpdated("Booker changed", "BookingRequest", Id, bookerName);
                }
            }
        }

       
    }
}
