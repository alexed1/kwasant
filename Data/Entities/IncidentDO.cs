using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Infrastructure;
using Data.Infrastructure;
using Data.Interfaces;
using Utilities;

namespace Data.Entities
{
    public class IncidentDO : BaseDO, IReportItemDO
    {
        public IncidentDO()
        {
            Priority = 1;
            //Notes = "No additional notes";
        }

        [Key]
        public int Id { get; set; }
        public String PrimaryCategory { get; set; }
        public String SecondaryCategory { get; set; }
        public String Activity { get; set; }
        public int Priority { get; set; }
        public int ObjectId { get; set; }
        public string CustomerId { get; set; }
        public string BookerId { get; set; }
        public string Data { get; set; }
        public string Status { get; set; }

        [NotMapped]
        public bool IsHighPriority { get { return Priority >= 5; } }

        public override void OnModify(DbPropertyValues originalValues, DbPropertyValues currentValues)
        {
            base.OnModify(originalValues, currentValues);

            var reflectionHelper = new ReflectionHelper<IncidentDO>();
            var priorityPropertyName = reflectionHelper.GetPropertyName(i => i.Priority);
            if (!MiscUtils.AreEqual(originalValues[priorityPropertyName], currentValues[priorityPropertyName])
                && IsHighPriority)
            {
                AlertManager.HighPriorityIncidentCreated(Id);
            }
        }

        public override void AfterCreate()
        {
            base.AfterCreate();

            if (IsHighPriority)
            {
                AlertManager.HighPriorityIncidentCreated(Id);
            }
        }
    }
}
