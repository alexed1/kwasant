using System;
using System.Net;
using System.Data;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using System.Collections.Generic;


using Data.Models;
using Data.DataAccessLayer.Repositories;
using Data.DataAccessLayer.Infrastructure;
using DayPilot.Web.Mvc;
using DayPilot.Web.Mvc.Data;
using DayPilot.Web.Mvc.Enums;
using DayPilot.Web.Mvc.Events.Calendar;
using DayPilot.Web.Mvc.Events.Common;
using DayPilot.Web.Mvc.Events.Navigator;
using DayPilot.Web.Mvc.Json;
using DBTools.Managers.APIManager.Packagers.Shnexy;
using BeforeCellRenderArgs = DayPilot.Web.Mvc.Events.Calendar.BeforeCellRenderArgs;
using TimeRangeSelectedArgs = DayPilot.Web.Mvc.Events.Calendar.TimeRangeSelectedArgs;


namespace Shnexy.Controllers
{
    [HandleError]
    public class CalendarController : Controller
    {

        #region "Member Variable"

        private ShnexyDbContext db = new ShnexyDbContext();

        private ShnexyPackager API;

        IEmailRepository emailRepository = new EmailRepository(new UnitOfWork(new ShnexyDbContext()));
        IEventFileRepository eventFileRepository = new EventFileRepository(new UnitOfWork(new ShnexyDbContext()));

        #endregion "Member Variable"

        #region "Action"
        public ActionResult Index(int id = 0)
        {
            if (id <= 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            PopulateCalender();

            #region "Get Email Related Information"

            if (emailRepository != null)
            {
                Email email = emailRepository.GetByKey(id);

                if (email != null)
                {
                    EventData._EmailId = email.Id;

                    List<EmailAddress> emailSenderList = new List<EmailAddress>();
                    StringBuilder sb = new StringBuilder("");

                    #region "Get To Addresses"

                    sb = new StringBuilder("");
                    sb.AppendFormat(" SELECT Id, Name, Email FROM dbo.EmailAddresses WHERE Email_Id2={0}", email.Id);

                    List<EmailAddress> emailToEmailAddressList = new List<EmailAddress>();

                    emailToEmailAddressList = db.EmailAddresses.SqlQuery(sb.ToString()).ToList();

                    StringBuilder sbToEmailAddressList = new StringBuilder("");

                    if (emailToEmailAddressList != null && emailToEmailAddressList.Count > 0)
                    {
                        email.To = emailToEmailAddressList;

                        foreach (EmailAddress emailAddress in emailToEmailAddressList)
                        {
                            if (sbToEmailAddressList.Length == 0)
                            {
                                sbToEmailAddressList.Append(emailAddress.Email);
                            }
                            else
                            {
                                sbToEmailAddressList.Append(",");
                                sbToEmailAddressList.Append(emailAddress.Email);
                            }
                        }
                    }

                    #endregion "Get To Addresses"

                    #region "Get CC Addresses"

                    sb = new StringBuilder("");
                    sb.AppendFormat(" SELECT Id, Name, Email FROM dbo.EmailAddresses WHERE Email_Id1={0}", email.Id);

                    List<EmailAddress> emailCCEmailAddressList = new List<EmailAddress>();

                    emailToEmailAddressList = db.EmailAddresses.SqlQuery(sb.ToString()).ToList();

                    StringBuilder sbCCEmailAddressList = new StringBuilder("");

                    if (emailCCEmailAddressList != null && emailCCEmailAddressList.Count > 0)
                    {
                        email.CC = emailCCEmailAddressList;

                        foreach (EmailAddress emailAddress in emailCCEmailAddressList)
                        {
                            if (sbCCEmailAddressList.Length == 0)
                            {
                                sbCCEmailAddressList.Append(emailAddress.Email);
                            }
                            else
                            {
                                sbCCEmailAddressList.Append(",");
                                sbCCEmailAddressList.Append(emailAddress.Email);
                            }
                        }
                    }

                    #endregion "Get CC Addresses"

                    #region "Get BCC Addresses"

                    sb = new StringBuilder("");
                    sb.AppendFormat(" SELECT Id, Name, Email FROM dbo.EmailAddresses WHERE Email_Id={0}", email.Id);

                    List<EmailAddress> emailBCCEmailAddressList = new List<EmailAddress>();

                    emailToEmailAddressList = db.EmailAddresses.SqlQuery(sb.ToString()).ToList();

                    StringBuilder sbBCCEmailAddressList = new StringBuilder("");

                    if (emailBCCEmailAddressList != null && emailBCCEmailAddressList.Count > 0)
                    {
                        email.Bcc = emailBCCEmailAddressList;

                        foreach (EmailAddress emailAddress in emailBCCEmailAddressList)
                        {
                            if (sbBCCEmailAddressList.Length == 0)
                            {
                                sbBCCEmailAddressList.Append(emailAddress.Email);
                            }
                            else
                            {
                                sbBCCEmailAddressList.Append(",");
                                sbBCCEmailAddressList.Append(emailAddress.Email);
                            }
                        }
                    }

                    #endregion "Get BCC Addresses"

                    #region "Set ViewData"

                    ViewData["ToEmailAddresses"] = sbToEmailAddressList.ToString();
                    ViewData["CCEmailAddresses"] = sbCCEmailAddressList.ToString();
                    ViewData["BCCEmailAddresses"] = sbBCCEmailAddressList.ToString();

                    #endregion "Set ViewData"

                    return View(email);
                }
                else
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
            }
            else
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            #endregion "Get Email Related Information"
        }

        public ActionResult Rtl()
        {
            return View();
        }
        public ActionResult Columns50()
        {
            return View();
        }
        public ActionResult Height100Pct()
        {
            return View();
        }

        public ActionResult Notify()
        {
            return View();
        }

        public ActionResult Crosshair()
        {
            return View();
        }

        public ActionResult ThemeBlue()
        {
            return View();
        }

        public ActionResult ThemeGreen()
        {
            return View();
        }

        public ActionResult ThemeWhite()
        {
            return View();
        }

        public ActionResult ThemeTraditional()
        {
            return View();
        }

        public ActionResult ThemeTransparent()
        {
            return View();
        }

        public ActionResult TimeHeaderCellDuration()
        {
            return View();
        }

        public ActionResult ActiveAreas()
        {
            return View();
        }

        public ActionResult JQuery()
        {
            return View();
        }

        public ActionResult HeaderAutoFit()
        {
            return View();
        }

        public ActionResult ExternalDragDrop()
        {
            return View();
        }

        public ActionResult Open()
        {
            return View();
        }

        public ActionResult EventArrangement()
        {
            return View();
        }

        public ActionResult AutoRefresh()
        {
            return View();
        }

        public ActionResult Today()
        {
            return View();
        }

        public ActionResult DaysResources()
        {
            return View();
        }

        public ActionResult Resources()
        {
            return View();
        }

        public ActionResult ContextMenu()
        {
            return View();
        }

        public ActionResult Message()
        {
            return View();
        }

        public ActionResult DayRange()
        {
            return View();
        }

        public ActionResult EventSelecting()
        {
            return View();
        }

        public ActionResult AutoHide()
        {
            return View();
        }

        public ActionResult GoogleLike()
        {
            return View();
        }


        public ActionResult RecurringEvents()
        {
            return View();
        }

        public ActionResult ThemeSilver()
        {
            return RedirectToAction("ThemeTraditional");
        }

        public ActionResult ThemeGreenWithBar()
        {
            return RedirectToAction("ThemeGreen");
        }

        public ActionResult Outlook2000()
        {
            return RedirectToAction("ThemeTraditional");
        }

        public ActionResult Backend()
        {
            return new Dpc().CallBack(this);
        }

        public ActionResult NavigatorBackend()
        {
            return new Dpn().CallBack(this);
        }

        public ActionResult New(string id)
        {
            ViewData["Id"] = id;

            return View(new EventManager.Event
            {
                Start = Convert.ToDateTime(Request.QueryString["start"]),
                End = Convert.ToDateTime(Request.QueryString["end"])
            });
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult New(FormCollection form)
        {

            DateTime dtFromDate;
            DateTime dtToDate;
            String strLocation = String.Empty;
            Boolean bIsAllDay = false;
            String strStatus = String.Empty;
            String strTransparency = String.Empty;
            String strClass = String.Empty;
            String strDescription = String.Empty;
            int intPriority = 0;
            int intSequence = 0;
            String strSummary = String.Empty;
            String strCategory = String.Empty;
            String strId = String.Empty;

            dtFromDate = form["FromDate"] != null ? Convert.ToDateTime(form["FromDate"]) : DateTime.MinValue;
            dtToDate = form["ToDate"] != null ? Convert.ToDateTime(form["ToDate"]) : DateTime.MinValue;
            strLocation = !String.IsNullOrEmpty(form["Location"]) ? form["Location"].ToString() : String.Empty;
            strStatus = !String.IsNullOrEmpty(form["Status"]) ? form["Status"].ToString() : String.Empty;
            strTransparency = !String.IsNullOrEmpty(form["TransparencyType"]) ? form["TransparencyType"].ToString() : String.Empty;
            strClass = !String.IsNullOrEmpty(form["Class"]) ? form["Class"].ToString() : String.Empty;
            strDescription = !String.IsNullOrEmpty(form["Description"]) ? form["Description"].ToString() : String.Empty;
            intPriority = !String.IsNullOrEmpty(form["Priority"]) ? Convert.ToInt32(form["Priority"]) : 0;
            intSequence = !String.IsNullOrEmpty(form["Sequence"]) ? Convert.ToInt32(form["Sequence"]) : 0;
            strSummary = !String.IsNullOrEmpty(form["Summary"]) ? form["Summary"].ToString() : String.Empty;
            strCategory = !String.IsNullOrEmpty(form["Category"]) ? form["Category"].ToString() : String.Empty;
            strId = !String.IsNullOrEmpty(form["Id"]) ? form["Id"].ToString() : String.Empty;

            #region Commmentted
            //if (!String.IsNullOrEmpty(form["chkIsAllDay"]))
            //{
            //    String strCheckAllDay = form["chkIsAllDay"].ToString();
            //    String strTemp = String.Empty;

            //    if (strCheckAllDay.IndexOf(',') > -1)
            //    {
            //        String[] arrCheckAllDay = strCheckAllDay.Split(',');
            //        strTemp = arrCheckAllDay[0];
            //    }
            //    else
            //    {
            //        strTemp = strCheckAllDay;
            //    }

            //    bIsAllDay = Convert.ToBoolean(strTemp);
            //}

            #endregion Commmentted

            if (intPriority == 0 || intSequence == 0)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);


            String strEventICSString = String.Empty;

            int intResult;
            Boolean blnResult;

            blnResult = Int32.TryParse(strId, out intResult);

            EventFile eventFile = new EventFile();

            (new EventManager(this)).EventDelete(strId);


            if (!blnResult)
            {
                strEventICSString = GetICSFormattedEventString(EventData._dtStartDate, EventData._dtEndDate, intSequence, strCategory, intPriority, strTransparency, strStatus, strClass, strSummary, strDescription, strLocation);
                eventFile.Body = strEventICSString;

                eventFileRepository.Add(eventFile);
                eventFileRepository.UnitOfWork.SaveChanges();

                #region "Update Email Status to Proccessed"

                if (EventData._EmailId > 0)
                {
                    Email existingEmail = emailRepository.GetByKey(EventData._EmailId);

                    if (existingEmail != null)
                    {
                        Email newEmail = new Email();

                        newEmail.Id = EventData._EmailId;
                        newEmail.Text = existingEmail.Text;
                        newEmail.Subject = existingEmail.Subject;
                        newEmail.FromEmail = existingEmail.FromEmail;
                        newEmail.Status = "Processed";

                        emailRepository.Update(newEmail, existingEmail);
                        emailRepository.UnitOfWork.SaveChanges();

                        newEmail.Id = 0;
                    }
                }

                #endregion "Update Email Status to Proccessed"

                new EventManager(this).EventCreate(EventData._dtStartDate, EventData._dtEndDate, strDescription, null, eventFile.Id.ToString());
            }
            else
            {
                int intEventFileId = Convert.ToInt32(strId);

                if (intEventFileId > 0)
                {
                    EventFile eventFileExist = eventFileRepository.GetByKey(intEventFileId);

                    if (eventFileExist != null)
                    {

                        String strDTSTART = String.Empty;
                        String strDTEND = String.Empty;

                        strDTSTART = !String.IsNullOrEmpty(form["start"]) ? form["start"].ToString() : String.Empty;
                        strDTEND = !String.IsNullOrEmpty(form["end"]) ? form["end"].ToString() : String.Empty;

                        if (!String.IsNullOrEmpty(strDTSTART) && !String.IsNullOrEmpty(strDTEND))
                        {
                            DateTime dtTempStartDate = ConvertLocalDateTime(strDTSTART);

                            String strDate = String.Format("{0}/{1}/{2}", dtTempStartDate.Month, dtTempStartDate.Day, dtTempStartDate.Year);

                            if (strDate != "1/1/1")
                            {

                                DateTime dtEditStartDate = ConvertLocalDateTime(strDTSTART);
                                DateTime dtEditEndDate = ConvertLocalDateTime(strDTEND);

                                EventFile newEventFile = new EventFile();
                                newEventFile.Id = intEventFileId;

                                strEventICSString = GetICSFormattedEventString(dtEditStartDate, dtEditEndDate, intSequence, strCategory, intPriority, strTransparency, strStatus, strClass, strSummary, strDescription, strLocation);
                                newEventFile.Body = strEventICSString;

                                eventFileRepository.Update(newEventFile, eventFileExist);
                                eventFileRepository.UnitOfWork.SaveChanges();

                                new EventManager(this).EventCreate(dtEditStartDate, dtEditEndDate, strDescription, null, intEventFileId.ToString());
                            }
                        }

                    }
                }
            }

            return JavaScript(SimpleJsonSerializer.Serialize("OK"));

        }

        #endregion "Action"

        #region "Method"

        private String GetICSFormattedEventString(DateTime FromDate, DateTime ToDate, int Sequence, String Category, int Priority, String Transp, String Status, String Class, String Summary, String Description, String Location)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("BEGIN:VCALENDAR\r\f");
            sb.Append("VERSION:2.0\r\f");
            sb.Append("PRODID:-//ince/events//NONSGML v1.0//EN\r\f");
            sb.Append("BEGIN:VEVENT\r\f");
            sb.Append(String.Format("DTSTART:{0}\r\f", GetFormatedDate(FromDate)));
            sb.Append(String.Format("DTEND:{0}\r\f", GetFormatedDate(ToDate)));
            sb.Append(String.Format("SEQUENCE:{0}\r\f", Sequence));
            sb.Append(String.Format("UID:{0}\r\f", new Guid()));
            sb.Append(String.Format("CATEGORIES:{0}\r\f", Category));
            sb.Append(String.Format("PRIORITY:{0}\r\f", Priority));
            sb.Append(String.Format("STATUS:{0}\r\f", Status));
            sb.Append(String.Format("TRANSP:{0}\r\f", Transp));
            sb.Append(String.Format("CLASS:{0}\r\f", Class));
            sb.Append(String.Format("SUMMARY:{0}\r\f", Summary));
            sb.Append(String.Format("DESCRIPTION:{0}\r\f", Description));
            sb.Append(String.Format("LOCATION:{0}\r\f", Location));
            sb.Append(String.Format("LOCATION:{0}\r\f", Location));
            sb.Append("END:VEVENT");
            sb.Append("END:VCALENDAR\r\f");
            return sb.ToString();
        }

        private static string GetFormatedDate(DateTime date)
        {
            return String.Format("{0}{1}{2}T{3}{4}00Z", date.ToUniversalTime().Year, date.ToUniversalTime().Month.ToString("00"), date.ToUniversalTime().Day.ToString("00"), date.ToUniversalTime().Hour.ToString("00"), date.ToUniversalTime().Minute.ToString("00"));
        }

        private String CreateBody(String title, String description, String location, DateTime startTime, DateTime endTime)
        {
            const String successStyle = ".success{color: #333333;font-family: Century Gothic, Arial;font-size:1.4em;margin-top:5px;margin-bottom:5px;}";
            const String standardStyle = ".standard{color: #333333;font-family: Century Gothic, Arial;font-size:1.0em}";
            const String reminderStyle = ".reminderStyle{color: red;font-family: Century Gothic, Arial;font-size:1.0em;font-variant:italic;margin-top:2px;margin-bottom:2px;}";
            const String contentsStyle = ".contentsTable{color: #333333;font-family: Century Gothic, Arial;font-size:1.0em;border-collapse:collapse;border-spacing:0px;padding:0px;margin:0px;} .contentsTable > td {padding-right:5px;}";
            var b = new StringBuilder();
            b.Append(String.Format("<style>{0} {1} {2} {3}</style>", successStyle, standardStyle, contentsStyle, reminderStyle));
            b.Append(String.Format("<div class=\"standard\">You have successfully registered for the following event</div>"));
            b.Append(String.Format("<div class=\"success\">{0}</div>", title));
            b.Append(String.Format("<div class=\"reminderStyle\">Please click on the calendar invitation to add this appointment to your calendar.</div>"));
            b.Append(String.Format("<table class=\"contentsTable\">"));
            b.Append(String.Format("<tr class=\"standard\"><td>Time</td><td>{0} - {1}</td></tr>", startTime, endTime));
            b.Append(String.Format("<tr class=\"standard\"><td>Location</td><td>{0}</td></tr>", location));
            b.Append(String.Format("<tr class=\"standard\"><td>Description</td><td>{0}</td></tr>", description));
            b.Append(String.Format("</table>"));
            return b.ToString();
        }

        private DateTime ConvertLocalDateTime(String UnversalDateTimeString)
        {
            String strLocalDateTime = UnversalDateTimeString.Insert(4, "-").Insert(7, "-").Replace("T", " ").Insert(13, ":").Insert(16, ":");

            try
            {
                return DateTime.Parse(strLocalDateTime);
            }
            catch (Exception e)
            {
                return new DateTime();
            }
        }

        private void PopulateCalender()
        {
            int day = (int)DateTime.Now.DayOfWeek;
            DateTime dtWeekStartDate = DateTime.Now.AddDays(-day);
            DateTime dtWeekEndDate = dtWeekStartDate.AddDays(6);

            List<EventFile> eventFileList = eventFileRepository.GetAll().ToList();

            int DTSTART = 4;
            int DTEND = 5;
            int DESCRIPTION = 14;

            foreach (EventFile eFile in eventFileList)
            {
                String[] arrICSBodyString = eFile.Body.Split(Environment.NewLine.ToCharArray());

                String[] arrDTSTART = arrICSBodyString[DTSTART].Split(':');

                String strDTSTART = String.Empty;
                strDTSTART = arrDTSTART[1];

                DateTime dtStart = ConvertLocalDateTime(strDTSTART);

                String[] arrDTEND = arrICSBodyString[DTEND].Split(':');

                String strDTEND = String.Empty;
                strDTEND = arrDTEND[1];

                DateTime dtEnd = ConvertLocalDateTime(strDTEND);

                String[] arrBody = arrICSBodyString[DESCRIPTION].Split(':');
                String strBody = String.Empty;

                strBody = arrBody[1];

                if (dtWeekStartDate >= dtStart || dtEnd <= dtWeekEndDate)
                {
                    try
                    {
                        new EventManager(this).EventCreate(dtStart, dtEnd, strBody, null, eFile.Id.ToString());
                    }
                    catch (Exception e)
                    {
                    }
                }
            }
        }

        #endregion "Method"


        public static class EventData
        {
            internal static DateTime _dtStartDate;
            internal static DateTime _dtEndDate;

            internal static DateTime _dtEditStartDate;
            internal static DateTime _dtEditEndDate;

            internal static String _Id;

            internal static DateTime _dtCalenderEndDate;

            internal static int _EmailId;
        }

        public class Dpn : DayPilotNavigator
        {
            protected override void OnVisibleRangeChanged(VisibleRangeChangedArgs visibleRangeChangedArgs)
            {
                // this select is a really bad example, no where clause
                if (Id == "dpn_recurring")
                {
                    Events = new EventManager(Controller, "recurring").Data.AsEnumerable();
                    DataRecurrenceField = "recurrence";
                }
                else
                {
                    Events = new EventManager(Controller).Data.AsEnumerable();
                }

                DataStartField = "start";
                DataEndField = "end";
                DataIdField = "id";
            }
        }

        public class Dpc : DayPilotCalendar
        {
            IEventFileRepository eventFileRepository = new EventFileRepository(new UnitOfWork(new ShnexyDbContext()));

            protected override void OnTimeRangeSelected(TimeRangeSelectedArgs e)
            {
                new EventManager(Controller).EventCreate(e.Start, e.End, "Click To Open Form", e.Resource);

                EventData._dtStartDate = Convert.ToDateTime(e.Start);
                EventData._dtEndDate = Convert.ToDateTime(e.End);

                Update();
            }

            protected override void OnEventMove(DayPilot.Web.Mvc.Events.Calendar.EventMoveArgs e)
            {
                if (new EventManager(Controller).Get(e.Id) != null)
                {
                    new EventManager(Controller).EventMove(e.Id, e.NewStart, e.NewEnd);
                    MoveUpdateEventFile(e.Id, e.NewStart, e.NewEnd);
                }
                else // external drag&drop
                {
                    new EventManager(Controller).EventCreate(e.NewStart, e.NewEnd, e.Text, e.NewResource, e.Id);
                    MoveUpdateEventFile(e.Id, e.NewStart, e.NewEnd);
                }

                Update();
            }

            private static string GetFormatedDate(DateTime date)
            {
                return String.Format("{0}{1}{2}T{3}{4}00Z", date.ToUniversalTime().Year, date.ToUniversalTime().Month.ToString("00"), date.ToUniversalTime().Day.ToString("00"), date.ToUniversalTime().Hour.ToString("00"), date.ToUniversalTime().Minute.ToString("00"));
            }

            private void MoveUpdateEventFile(String Id, DateTime StartDate, DateTime EndDate)
            {

                int DTSTART = 4;
                int DTEND = 5;

                EventFile evtFile = new EventFile();

                if (eventFileRepository != null)
                {
                    int intResultId;
                    Boolean blnResult;

                    blnResult = Int32.TryParse(Id, out intResultId);

                    if (blnResult)
                    {
                        evtFile = eventFileRepository.GetByKey(intResultId);

                        StringBuilder sb = new StringBuilder("");

                        if (evtFile != null)
                        {
                            String strICSMessageBody = String.Empty;
                            strICSMessageBody = evtFile.Body;

                            String[] arrICSString = strICSMessageBody.Split(Environment.NewLine.ToCharArray());

                            String strStartDate = String.Format("\fDTSTART:{0}", GetFormatedDate(StartDate));
                            String strEndDate = String.Format("\fDTEND:{0}", GetFormatedDate(EndDate));

                            arrICSString[4] = strStartDate;
                            arrICSString[5] = strEndDate;

                            foreach (String strTemp in arrICSString)
                            {
                                sb.Append(strTemp);
                                sb.Append("\r");
                            }
                        }

                        EventFile newEventFile = new EventFile();

                        newEventFile.Id = intResultId;
                        newEventFile.Body = sb.ToString();

                        eventFileRepository.Update(newEventFile, evtFile);
                        eventFileRepository.UnitOfWork.SaveChanges();
                    }
                }
            }


            protected override void OnEventClick(EventClickArgs e)
            {
                EventData._Id = e.Id;

                //UpdateWithMessage("Event clicked: " + e.Text);
                //Redirect("http://www.daypilot.org/");
            }

            protected override void OnEventDelete(EventDeleteArgs e)
            {
                new EventManager(Controller).EventDelete(e.Id);
                Update();
            }

            protected override void OnEventResize(DayPilot.Web.Mvc.Events.Calendar.EventResizeArgs e)
            {
                new EventManager(Controller).EventMove(e.Id, e.NewStart, e.NewEnd);
                Update();
            }

            protected override void OnEventBubble(EventBubbleArgs e)
            {
                EventData._dtEditStartDate = e.Start;
                EventData._dtEditEndDate = e.End;

                EventData._Id = e.Id;
                e.BubbleHtml = "This is an event bubble for id: " + e.Id;
                //EventData._dtStartDate = Convert.ToDateTime(e.Start);
                //EventData._dtEndDate = Convert.ToDateTime(e.End);
            }

            protected override void OnEventMenuClick(EventMenuClickArgs e)
            {
                switch (e.Command)
                {
                    case "Delete":

                        int intResultId;
                        Boolean blnResult;

                        blnResult = Int32.TryParse(e.Id, out intResultId);

                        if (blnResult)
                        {
                            EventFile deleteEventFile;

                            deleteEventFile = eventFileRepository.GetByKey(intResultId);

                            if (deleteEventFile != null)
                            {
                                eventFileRepository.Remove(deleteEventFile);
                                eventFileRepository.UnitOfWork.SaveChanges();
                            }
                        }

                        new EventManager(Controller).EventDelete(e.Id);
                        Update();
                        break;
                }
            }

            protected override void OnCommand(CommandArgs e)
            {
                switch (e.Command)
                {
                    case "navigate":
                        StartDate = (DateTime)e.Data["start"];
                        Update(CallBackUpdateType.Full);
                        break;

                    case "refresh":
                        UpdateWithMessage("Refreshed");
                        break;

                    case "selected":
                        if (SelectedEvents.Count > 0)
                        {
                            EventInfo ei = SelectedEvents[0];
                            SelectedEvents.RemoveAt(0);
                            UpdateWithMessage("Event removed from selection: " + ei.Text);
                        }

                        break;

                    case "delete":
                        string id = (string)e.Data["id"];
                        new EventManager(Controller).EventDelete(id);
                        Update(CallBackUpdateType.EventsOnly);
                        break;

                }
            }

            protected override void OnBeforeCellRender(BeforeCellRenderArgs e)
            {
                if (Id == "dpc_today")
                {
                    if (e.Start.Date == DateTime.Today)
                    {
                        if (e.IsBusiness)
                        {
                            e.BackgroundColor = "#ffaaaa";
                        }
                        else
                        {
                            e.BackgroundColor = "#ff6666";
                        }
                    }
                }

            }

            protected override void OnBeforeEventRender(BeforeEventRenderArgs e)
            {

                if (Id == "dpcg")  // Calendar/GoogleLike
                {
                    if (e.Id == "6")
                    {
                        e.BorderColor = "#1AAFE0";
                        e.BackgroundColor = "#90D8F2";
                    }
                    if (e.Id == "8")
                    {
                        e.BorderColor = "#068c14";
                        e.BackgroundColor = "#08b81b";
                    }
                    if (e.Id == "2")
                    {
                        e.BorderColor = "#990607";
                        e.BackgroundColor = "#f60e13";
                    }
                }
                else if (Id == "dpc_menu")  // Calendar/ContextMenu
                {
                    if (e.Id == "7")
                    {
                        e.ContextMenuClientName = "menu2";
                    }
                }
                else if (Id == "dpc_areas")  // Calendar/ActiveAreas
                {
                    e.CssClass = "calendar_white_event_withheader";

                    e.Areas.Add(new Area().Right(3).Top(3).Width(15).Height(15).CssClass("event_action_delete").JavaScript("dpc_areas.eventDeleteCallBack(e);"));
                    e.Areas.Add(new Area().Right(20).Top(3).Width(15).Height(15).CssClass("event_action_menu").JavaScript("dpc_areas.bubble.showEvent(e, true);"));
                    e.Areas.Add(new Area().Left(0).Bottom(5).Right(0).Height(5).CssClass("event_action_bottomdrag").ResizeEnd());
                    e.Areas.Add(new Area().Left(15).Top(1).Right(46).Height(11).CssClass("event_action_move").Move());
                }

                if (e.Id == "7")
                {
                    e.DurationBarColor = "red";
                }

                if (e.Recurrent)
                {
                    e.InnerHtml += " (R)";
                }
            }

            protected override void OnInit(InitArgs initArgs)
            {
                //Thread.Sleep(5000);

                UpdateWithMessage("Welcome!", CallBackUpdateType.Full);

                if (Id == "days_resources")
                {
                    Columns.Clear();
                    Column today = new Column(DateTime.Today.ToShortDateString(), DateTime.Today.ToString("s"));
                    today.Children.Add("A", "a", DateTime.Today);
                    today.Children.Add("B", "b", DateTime.Today);
                    Columns.Add(today);

                    Column tomorrow = new Column(DateTime.Today.AddDays(1).ToShortDateString(), DateTime.Today.AddDays(1).ToString("s"));
                    tomorrow.Children.Add("A", "a", DateTime.Today.AddDays(1));
                    tomorrow.Children.Add("B", "b", DateTime.Today.AddDays(1));
                    Columns.Add(tomorrow);

                }
                else if (Id == "resources")
                {
                    Columns.Clear();
                    Columns.Add("A", "A");
                    Columns.Add("B", "B");
                    Columns.Add("C", "C");
                }
            }

            protected override void OnBeforeHeaderRender(BeforeHeaderRenderArgs e)
            {
                if (Id == "dpc_areas")
                {
                    e.Areas.Add(new Area().Right(1).Top(0).Width(17).Bottom(1).CssClass("resource_action_menu").Html("<div><div></div></div>").JavaScript("alert(e.date);"));
                }
                if (Id == "dpc_autofit")
                {
                    e.InnerHtml += " adding some longer text so the autofit can be tested";
                }

                EventData._dtCalenderEndDate = e.Date;
            }
            protected override void OnBeforeTimeHeaderRender(BeforeTimeHeaderRenderArgs e)
            {
            }

            protected override void OnFinish()
            {
                // only load the data if an update was requested by an Update() call
                if (UpdateType == CallBackUpdateType.None)
                {
                    return;
                }

                // this select is a really bad example, no where clause
                if (Id == "dpc_recurring")
                {
                    Events = new EventManager(Controller, "recurring").Data.AsEnumerable();
                    DataRecurrenceField = "recurrence";
                }
                else
                {
                    Events = new EventManager(Controller).Data.AsEnumerable();
                }

                DataStartField = "start";
                DataEndField = "end";
                DataTextField = "text";
                DataIdField = "id";
                DataResourceField = "resource";

                DataAllDayField = "allday";
                //DataTagFields = "id, name";
            }
        }
    }
}
