using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Infrastructure;
using Data.Infrastructure;
using Data.Interfaces;
using Data.Repositories;
using Data.States.Templates;
using StructureMap;
using Utilities;

namespace Data.Entities
{
    public class NegotiationDO : BaseDO, IModifyHook, ICreateHook
    {
        public NegotiationDO()
        {
            Questions = new List<QuestionDO>();
            Attendees = new List<AttendeeDO>();

            NegotiationState = States.NegotiationState.InProcess;
        }

        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime DateCreated { get; set; }

        [ForeignKey("NegotiationStateTemplate")]
        public int? NegotiationState { get; set; }
        public _NegotiationStateTemplate NegotiationStateTemplate { get; set; }
       
        [ForeignKey("BookingRequest"), Required]
        public int? BookingRequestID { get; set; }
        public virtual BookingRequestDO BookingRequest { get; set; }

        [InverseProperty("Negotiation")]
        public virtual IList<CalendarDO> Calendars { get; set; }

        [InverseProperty("Negotiation")]
        public virtual IList<AttendeeDO> Attendees { get; set; }

        [InverseProperty("Negotiation")]
        public virtual IList<QuestionDO> Questions { get; set; }

        public void OnModify(DbPropertyValues originalValues, DbPropertyValues currentValues)
        {
            var reflectionHelper = new ReflectionHelper<NegotiationDO>();

            var statePropertyName = reflectionHelper.GetPropertyName(br => br.NegotiationState);
            if (!MiscUtils.AreEqual(originalValues[statePropertyName], currentValues[statePropertyName]))
            {
                using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
                {
                    AlertManager.TrackablePropertyUpdated("State changed", "NegotiationRequest", Id, new GenericRepository<_NegotiationStateTemplate>(uow).GetByKey(NegotiationState).Name);
    }
}

        }


        public void AfterCreate()
        {
            AlertManager.TrackablePropertyCreated("Negotiation Request created", "NegotiationRequest", Id, "Name: " + Name);
        }
    }
}
