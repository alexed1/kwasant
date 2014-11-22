using System;
using System.Collections.Generic;
using System.Linq;
using Data.Entities;
using Data.Interfaces;
using Utilities;

namespace KwasantCore.Services
{
    public class Report
    {
        public const string DateStandardFormat = @"yyyy-MM-ddTHH\:mm\:ss.fffffff"; //This allows javascript to parse the date properly

        public object Generate(IUnitOfWork uow, DateRange dateRange, string type, int start,
            int length, out int recordcount)
        {
            recordcount = 0;
            switch (type)
            {
                case "alllogs":
                    return ShowAllLogs(uow, dateRange, start,
             length, out recordcount);
                case "usage":
                    return GenerateUsageReport(uow, dateRange, start,
             length, out recordcount);
                case "incident":
                    return ShowAllIncidents(uow, dateRange, start,
            length, out recordcount);
                case "fiveRecentIncident":
                    return ShowMostRecent5Incidents(uow, dateRange, start,
             length, out recordcount);
                case "showbookerthroughput":
                    return ShowBookerThroughput(uow, dateRange, start,
             length, out recordcount);
            }
            return this;
        } 

        private List<object> ShowAllLogs(IUnitOfWork uow, DateRange dateRange, int start,
            int length, out int count)
        {
            var logDO = uow.LogRepository.GetAll()
                .Where(e => e.Date > dateRange.StartTime && e.Date < dateRange.EndTime);

            count = logDO.Count();

            return logDO.Skip(start).Take(length).OrderByDescending(e => e.Date)
                .Select(l => (object)new
                {
                    Date = l.Date.ToString(DateStandardFormat),
                    l.Name,
                    l.Level,
                    l.Message
                }).ToList();

        }

        private object GenerateUsageReport(IUnitOfWork uow, DateRange dateRange, int start,
            int length, out int count)
        {
            var factDO = uow.FactRepository.GetAll()
                    .Where(e => e.CreateDate > dateRange.StartTime && e.CreateDate < dateRange.EndTime);

            count = factDO.Count();

            return factDO.Skip(start).Take(length)
                   .Select(
                       f => new
                       {
                           PrimaryCategory = f.PrimaryCategory,
                           SecondaryCategory = f.SecondaryCategory,
                           Activity = f.Activity,
                           Status = f.Status,
                           Data = f.Data,
                           CreateDate = f.CreateDate.ToString(DateStandardFormat),
                       }).ToList();

        }

        private object ShowAllIncidents(IUnitOfWork uow, DateRange dateRange, int start,
            int length, out int count)
        {
            var incidentDO = uow.IncidentRepository.GetAll().Where(e => e.CreateDate > dateRange.StartTime && e.CreateDate < dateRange.EndTime);

            count = incidentDO.Count();

            return incidentDO.Skip(start)
                    .Take(length).Select(
                        f => new
                        {
                            PrimaryCategory = f.PrimaryCategory,
                            SecondaryCategory = f.SecondaryCategory,
                            Activity = f.Activity,
                            Data = f.Notes,
                            CreateDate = f.CreateDate.ToString(DateStandardFormat),
                            ObjectId = f.ObjectId

                        }).ToList();

        }

        private object ShowMostRecent5Incidents(IUnitOfWork uow, DateRange dateRange, int start,
            int length, out int count)
        {
            var incidentDO = uow.IncidentRepository.GetAll().Skip(start)
                    .Take(length).OrderByDescending(x => x.CreateDate).Take(5);

            count = incidentDO.Count();
            return incidentDO.Select(
                        f => new
                        {
                            PrimaryCategory = f.PrimaryCategory,
                            SecondaryCategory = f.SecondaryCategory,
                            Activity = f.Activity,
                            Data = f.Notes,
                            CreateDate = f.CreateDate.ToString(DateStandardFormat),
                        }).ToList();

        }

        public object GenerateHistoryReport(IUnitOfWork uow, DateRange dateRange, string primaryCategory, string bookingRequestId)
        {
            return uow.FactRepository.GetAll()
                .Where(e => e.PrimaryCategory == primaryCategory && (e.ObjectId == bookingRequestId) && e.CreateDate >= dateRange.StartTime && e.CreateDate <= dateRange.EndTime).OrderByDescending(e => e.CreateDate)
               .Select(
                        e =>
                            new
                            {
                                PrimaryCategory = e.PrimaryCategory,
                                SecondaryCategory = e.SecondaryCategory,
                                Activity = e.Activity,
                                Status = e.Status,
                                Data = e.Data,
                                CreateDate = e.CreateDate.ToString(DateStandardFormat),
                            })
                    .ToList();
        }

        public object GenerateHistoryByBookingRequestId(IUnitOfWork uow, int bookingRequestId)
        {
            return uow.FactRepository.GetAll().Where(e => e.ObjectId == bookingRequestId.ToString())
                    .OrderByDescending(e => e.CreateDate)
                    .Select(
                        e =>
                            new
                            {
                                PrimaryCategory = e.PrimaryCategory,
                                Activity = e.Activity,
                                Status = e.Status,
                                Data = e.Data,
                                CreateDate = e.CreateDate.ToString(DateStandardFormat),
                            })
                    .ToList();
        }

        private object ShowBookerThroughput(IUnitOfWork uow, DateRange dateRange, int start,
            int length, out int count)
        {
            Booker _booker = new Booker();

            var incidentDO = uow.IncidentRepository.GetAll().Where(e => e.CreateDate > dateRange.StartTime && e.CreateDate < dateRange.EndTime && e.PrimaryCategory ==
"BookingRequest" && e.Activity == "MarkedProcessed").Skip(start).Take(length).GroupBy(e => e.BookerId);

            count = incidentDO.Count();

            return incidentDO.Select(
                                    e => new
                                    {
                                        BRNameAndCount = _booker.GetName(uow, e.FirstOrDefault().BookerId) + " marked as processed " + e.Count() + " distinct BRs",
                                    }).ToList();

         }

       
    }
}
