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
        private readonly User _user;
        private readonly Email _email;
        private Dictionary<string, string> _dataUrlMappings;

        public Report() 
        {
            _user = new User();
            _email = new Email();
        }

        public object Generate(IUnitOfWork uow, DateRange dateRange, string type)
        {
            switch (type)
            {
                case "alllogs":
                    return ShowAllLogs(uow, dateRange);
                case "usage":
                    return GenerateUsageReport(uow, dateRange);
                case "incident":
                    return ShowAllIncidents(uow, dateRange);
                case "fiveRecentIncident":
                    return ShowMostRecent5Incidents(uow, dateRange);
            }
            return this;
        }

        private List<object> ShowAllLogs(IUnitOfWork uow, DateRange dateRange)
        {
            return uow.LogRepository.GetAll()
                .Where(e => e.Date > dateRange.StartTime && e.Date < dateRange.EndTime)
                .OrderByDescending(e => e.Date)
                .Select(l => (object)new
                {
                    Date = l.Date.ToString(DateStandardFormat),
                    l.Name,
                    l.Level,
                    l.Message
                }).ToList();
        }

        private object GenerateUsageReport(IUnitOfWork uow, DateRange dateRange)
        {
            _dataUrlMappings = new Dictionary<string, string>();
            _dataUrlMappings.Add("BookingRequest", "/Dashboard/Index/");
            _dataUrlMappings.Add("Email", "/Dashboard/Index/");
            _dataUrlMappings.Add("User", "/User/Details?userID=");

            return
                uow.FactRepository.GetAll()
                    .Where(e => e.CreateDate > dateRange.StartTime && e.CreateDate < dateRange.EndTime)
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

        private object ShowAllIncidents(IUnitOfWork uow, DateRange dateRange)
        {
            return uow.IncidentRepository.GetAll().Where(e => e.CreateDate > dateRange.StartTime && e.CreateDate < dateRange.EndTime).Select(
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

        private object ShowMostRecent5Incidents(IUnitOfWork uow, DateRange dateRange)
        {
            return uow.IncidentRepository.GetAll().OrderByDescending(x => x.CreateDate).Take(5).Select(
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
                                PrimaryCategory=e.PrimaryCategory,
                                Activity = e.Activity,
                                Status=e.Status,
                                Data=e.Data,
                                CreateDate = e.CreateDate.ToString(DateStandardFormat),
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
    }
}
