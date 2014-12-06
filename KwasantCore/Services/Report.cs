using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Data.Entities;
using Data.Interfaces;
using Utilities;

namespace KwasantCore.Services
{
    public class Report
    {
        public const string DateStandardFormat = @"yyyy-MM-ddTHH\:mm\:ss.fffffff"; //This allows javascript to parse the date properly
        private readonly User _user;
        private readonly Email _email;
        private Dictionary<string, string> _dataUrlMappings;

        public Report() 
        {
            _user = new User();
            _email = new Email();
        }

        
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
                case "showBookerThroughput":
                    return ShowBookerThroughput(uow, dateRange, start,
             length, out recordcount);
                case "showBRResponsiveness":
                    return ShowBRResponsiveness(uow, dateRange, start,
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
            _dataUrlMappings = new Dictionary<string, string>();
            _dataUrlMappings.Add("BookingRequest", "/Dashboard/Index/");
            _dataUrlMappings.Add("Email", "/Dashboard/Index/");
            _dataUrlMappings.Add("User", "/User/Details?userID=");

          
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
                            Data = AddClickability(f.Data),
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
            var incidentDO = uow.IncidentRepository.GetAll().OrderByDescending(x => x.CreateDate).Take(5);

            count = incidentDO.Count();
            return incidentDO.Select(
                        f => new
                        {
                            Id = f.Id,
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
            var strBookingRequestId = bookingRequestId.ToString(CultureInfo.InvariantCulture);
            return uow.FactRepository.GetQuery().Where(e => e.ObjectId == strBookingRequestId).AsEnumerable()
                .Union<IReportItemDO>(uow.IncidentRepository.GetQuery().Where(e => e.ObjectId == bookingRequestId).AsEnumerable())
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
                                SecondaryCategory = e.SecondaryCategory
                            })
                    .ToList();
        }

        private string AddClickability(string originalData)
        {
            if (originalData != null)
            {
                //This try-catch is to move on, even if something generates error, this is because right now we don't have consistency in our "Data" field
                //so in case of error it just return the original data.
                try
                {
                    string objectType = originalData.Split(' ')[0].ToString();
                    var splitedData = originalData.Split(':')[1];
                    string objectId = splitedData.Substring(0, splitedData.IndexOf(","));
                    string clickableLink = GetClickableLink(objectType, objectId);

                    originalData = originalData.Replace(objectId, clickableLink);
                }
                catch { }
            }
            return originalData;
        }

        private string GetClickableLink(string objectType, string objectId)
        {
            if (objectType == "Email")
            {
                string bookingRequestId = _email.FindEmailParentage(Convert.ToInt32(objectId));
                if (bookingRequestId != null)
                    return string.Format("<a style='padding-left:3px' target='_blank' href='{0}{1}'>{2}</a>", _dataUrlMappings[objectType], bookingRequestId, objectId);
            }
            if (objectType == "User")
            {
                string userId = _user.GetUserId(objectId);
                return string.Format("<a style='padding-left:3px' target='_blank' href='{0}{1}'>{2}</a>", _dataUrlMappings[objectType], userId, objectId);
            }
            return string.Format("<a style='padding-left:3px' target='_blank' href='{0}{1}'>{2}</a>", _dataUrlMappings[objectType], objectId, objectId);
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

        private object ShowBRResponsiveness(IUnitOfWork uow, DateRange dateRange, int start,
            int length, out int count)
        {
            Booker _booker = new Booker();
            var incidentDO = uow.IncidentRepository.GetAll().Where(e => e.CreateDate > dateRange.StartTime && e.CreateDate < dateRange.EndTime && e.PrimaryCategory ==
 "BookingRequest" && e.Activity == "Checkout").Skip(start).Take(length);
            count = incidentDO.Count();
            return incidentDO.Select(
            e => new
            {
                ObjectId = e.ObjectId,
                TimeToProcess = e.Data.Substring(e.Data.LastIndexOf(':') + 1),
            }

            ).ToList();

        }
    }
}
