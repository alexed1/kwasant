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
                case "incident":
                    return GenerateIncidentReport(uow, dateRange);
            }
            return this;
        }
        private List<FactDO> GenerateUsageReport(IUnitOfWork uow, DateRange dateRange)
        {
            return uow.FactRepository.GetAll().Where(e => e.CreateDate > dateRange.StartTime && e.CreateDate < dateRange.EndTime).ToList();
        }

        private List<IncidentDO> GenerateIncidentReport(IUnitOfWork uow, DateRange dateRange)
        {
            return uow.IncidentRepository.GetAll().Where(e => e.CreateTime > dateRange.StartTime && e.CreateTime < dateRange.EndTime).ToList();
        }
        public List<FactDO> GenerateHistoryReport(IUnitOfWork uow, DateRange dateRange,string primaryCategory, string bookingRequestId)
        {
            int objectId = 0;
            if (bookingRequestId != "")
                objectId = Convert.ToInt32(bookingRequestId);
            return uow.FactRepository.GetAll().Where(e => e.PrimaryCategory == primaryCategory && (objectId > 0 ? e.ObjectId == objectId : e.ObjectId != 0) && e.CreateDate > dateRange.StartTime && e.CreateDate < dateRange.EndTime).OrderByDescending(e=>e.CreateDate).ToList();
        }

        public List<FactDO> GenerateHistoryByBookingRequestId(IUnitOfWork uow, int bookingRequestId)
        {
            return uow.FactRepository.GetAll().Where(e => e.ObjectId == bookingRequestId).ToList();
        }
    }
}
