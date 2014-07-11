using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Data.Entities;
using Data.Interfaces;
using Utilities;

namespace KwasantCore.Services
{
    public class Report
    {
        public object Generate(IUnitOfWork uow, DateRange dateRange, string type)
        {
            switch (type)
            {
                case "usage":
                    return GenerateUsageReport(uow, dateRange);
                //case "incident":
            }
            return this;
        }
        private List<FactDO> GenerateUsageReport(IUnitOfWork uow, DateRange dateRange)
        {
            return uow.FactRepository.GetAll().Where(e => e.CreateDate > dateRange.StartTime && e.CreateDate < dateRange.EndTime).ToList();
        }
    }
}
