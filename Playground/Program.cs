using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Text;
using Data.Entities;
using Data.Entities.Enumerations;
using Data.Infrastructure;
using Data.Interfaces;
using KwasantCore.Services;
using KwasantCore.StructureMap;
using KwasantICS.DDay.iCal;
using KwasantICS.DDay.iCal.DataTypes;
using KwasantICS.DDay.iCal.Serialization.iCalendar.Serializers;
using log4net.Config;
using StructureMap;
using UtilitiesLib.Logging;

namespace Playground
{
    public class Program
    {
        /// <summary>
        /// This is a sandbox for devs to use. Useful for directly calling some library without needing to launch the main application
        /// </summary>
        /// <param name="args"></param>
        private static void Main(string[] args)
        {
            StructureMapBootStrapper.ConfigureDependencies("dev"); //set to either "test" or "dev"

            KwasantDbContext db = new KwasantDbContext();
            db.Database.Initialize(true);

            IUnitOfWork unitOfWork = ObjectFactory.GetInstance<IUnitOfWork>();
            var evDO = new EventDO()
            {
            };
            evDO.Attendees = new List<AttendeeDO>
            {
                new AttendeeDO
                {
                    EmailAddress = "RJRudman@outlook.com",
                    EventID = evDO.EventID,
                    Event = evDO,
                    Name = "Robert Rudman",
               },
            };
            evDO.Location = "Office";
            evDO.Description = "Some desc";
            evDO.Summary = "Some Summary";
            evDO.StartDate = DateTime.Now;
            evDO.EndDate = DateTime.Now.AddHours(1);

            var email = DispatchEvent(unitOfWork, evDO);
            new Email(unitOfWork, email).Send();
        }

        public static EmailDO DispatchEvent(IUnitOfWork uow, EventDO eventDO)
        {
            if (eventDO.Attendees == null)
                eventDO.Attendees = new List<AttendeeDO>();

            string fromEmail = "rjrudman@gmail.com";
            string fromName = "Kwasant Scheduling Services";

            EmailDO outboundEmail = new EmailDO();
            outboundEmail.From = new EmailAddressDO { Address = fromEmail, Name = fromName };
            outboundEmail.To = eventDO.Attendees.Select(a => new EmailAddressDO { Address = a.EmailAddress, Name = a.Name }).ToList();
            outboundEmail.Subject = "Invitation via Kwasant: " + eventDO.Summary + "@ " + eventDO.StartDate;
            outboundEmail.Text = @"
<div style=""padding:15px;color:#D85E17"">
	<a style=""font-family:'Open Sans', Helvetica, Helvetica Nue, Arial, sans-serif;font-size:20px;color:#D85E17;text-decoration:none;"" href=""http://kwasant-test.azurewebsites.net/""><img style=""vertical-align:middle"" src=""http://kwasant-test.azurewebsites.net/Content/images/perfect-krawsant.png"">     This is a Kwasant Event Request. For more information, click here.</a>
</a>
</div>

<div style=""width:100%;max-width:729px;font-family:Arial,sans-serif;border-style:solid;border-color: rgb(170, 170, 170);border-width:1px 2px 2px 1px;background-color:rgb(255,255,255)"">
	<div style=""padding:15px;"">		
		<h3 style=""padding-bottom:6px;margin:0px;font-family:Arial,sans-serif;font-size:16px;font-weight:bold;color:rgb(34,34,34)"">Invite</h3>
		<table>		
			<tbody>
				<tr>
					<td style=""padding:0px 1em 10px 0px;font-family:Arial,sans-serif;font-size:13px;color:rgb(136,136,136);white-space:nowrap"" valign=""top"">
	                  <div><i style=""font-style:normal"">Who</i></div>
	               </td>
					<td style=""padding-bottom:10px;font-family:Arial,sans-serif;font-size:13px;color:rgb(34,34,34)"" valign=""top"">
	Tue, May 13, 3pm - 4pm <span style=""color:rgb(136,136,136)"">GMT+04:00</span></td>
				<tr/>
				<tr>
	               <td style=""padding:0px 1em 10px 0px;font-family:Arial,sans-serif;font-size:13px;color:rgb(136,136,136);white-space:nowrap"" valign=""top"">
	                  <div><i style=""font-style:normal"">Who</i></div>
	               </td>
	               <td style=""padding-bottom:10px;font-family:Arial,sans-serif;font-size:13px;color:rgb(34,34,34)"" valign=""top"">
	                  <span class=""HOEnZb""><font color=""#888888""></font></span><span class=""HOEnZb""><font color=""#888888""></font></span>
	                  <table cellspacing=""0"" cellpadding=""0"">
	                     <tbody>
	                        <tr>
	                           <td style=""padding-right:10px;font-family:Arial,sans-serif;font-size:13px;color:rgb(34,34,34)"">
	                              <div>
	                                 <div style=""margin:0px 0px 0.3em"">Robert Rudman</div>
	                              </div>
	                           </td>
	                        </tr>
	                        <tr>
	                           <td style=""padding-right:10px;font-family:Arial,sans-serif;font-size:13px;color:rgb(34,34,34)"">
	                              <div>
	                                 <div style=""margin:0px 0px 0.3em""><a href=""mailto:support@crowdscreener.com"">support@crowdscreener.com</a></div>
	                              </div>
	                           </td>
	                        </tr>
	                     </tbody>
	                  </table>
	               </td>
	            </tr>
			</tbody>
		</table>
	</div>
</div>
";
            outboundEmail.Status = EmailStatus.QUEUED;

            iCalendar ddayCalendar = new iCalendar();
            DDayEvent dDayEvent = new DDayEvent();
            if (eventDO.IsAllDay)
            {
                dDayEvent.IsAllDay = true;
            }
            else
            {
                dDayEvent.DTStart = new iCalDateTime(eventDO.StartDate);
                dDayEvent.DTEnd = new iCalDateTime(eventDO.EndDate);
            }
            dDayEvent.DTStamp = new iCalDateTime(DateTime.Now);
            dDayEvent.LastModified = new iCalDateTime(DateTime.Now);

            dDayEvent.Location = eventDO.Location;
            dDayEvent.Description = eventDO.Description;
            dDayEvent.Summary = eventDO.Summary;
            foreach (AttendeeDO attendee in eventDO.Attendees)
            {
                dDayEvent.Attendees.Add(new Attendee
                {
                    CommonName = attendee.Name,
                    Type = "INDIVIDUAL",
                    Role = "REQ-PARTICIPANT",
                    ParticipationStatus = ParticipationStatus.NeedsAction,
                    RSVP = true,
                    Value = new Uri("mailto:" + attendee.EmailAddress),
                });
                attendee.Event = eventDO;
            }
            dDayEvent.Organizer = new Organizer(fromEmail) { CommonName = fromName };

            ddayCalendar.Events.Add(dDayEvent);
            ddayCalendar.Method = CalendarMethods.Request;

            AttachCalendarToEmail(ddayCalendar, outboundEmail);

            if (eventDO.Emails == null)
                eventDO.Emails = new List<EmailDO>();
            eventDO.Emails.Add(outboundEmail);

            return outboundEmail;
        }

        private static void AttachCalendarToEmail(iCalendar iCal, EmailDO emailDO)
        {
            iCalendarSerializer serializer = new iCalendarSerializer(iCal);
            string fileToAttach = serializer.Serialize(iCal);

//            fileToAttach =
//@"BEGIN:VCALENDAR
//PRODID:-//Google Inc//Google Calendar 70.9054//EN
//VERSION:2.0
//CALSCALE:GREGORIAN
//METHOD:REQUEST
//BEGIN:VEVENT
//DTSTART:20140519T113000Z
//DTEND:20140519T123000Z
//DTSTAMP:20140519T112437Z
//ORGANIZER;CN=Robert Rudman:mailto:rjrudman@gmail.com
//UID:2p56n0mrobf0222796a8h6tqs4@google.com
//ATTENDEE;CUTYPE=INDIVIDUAL;ROLE=REQ-PARTICIPANT;PARTSTAT=NEEDS-ACTION;RSVP=
// TRUE;CN=rjrudman@outlook.com;X-NUM-GUESTS=0:mailto:rjrudman@outlook.com
//ATTENDEE;CUTYPE=INDIVIDUAL;ROLE=REQ-PARTICIPANT;PARTSTAT=ACCEPTED;RSVP=TRUE
// ;CN=Robert Rudman;X-NUM-GUESTS=0:mailto:rjrudman@gmail.com
//CREATED:20140519T112437Z
//DESCRIPTION:SOMEDESC\nView your event at http://www.google.com/calendar/eve
// nt?action=VIEW&ueid=2p56n0mrobf0222796a8h6tqs4.
//LAST-MODIFIED:20140519T112437Z
//LOCATION:SOMEWHERE
//SEQUENCE:0
//STATUS:CONFIRMED
//SUMMARY:SOMETHING
//TRANSP:OPAQUE
//END:VEVENT
//END:VCALENDAR";

            AttachmentDO attachmentDO = GetBase64Event(fileToAttach);

            if (emailDO.Attachments == null)
                emailDO.Attachments = new List<AttachmentDO>();

            attachmentDO.Email = emailDO;
            emailDO.Attachments.Add(GetBase64Event(fileToAttach));
            //emailDO.Attachments.Add(GetTextEvent(fileToAttach));
        }

        private static AttachmentDO GetTextEvent(string fileToAttach)
        {
            var ct = new ContentType {MediaType = "text/calendar", Name = "invite.ics", CharSet = "UTF-8"};
            ct.Parameters.Add("method", "REQUEST");

            var attachmentDO = Email.CreateNewAttachment(
                new System.Net.Mail.Attachment(
                    new MemoryStream(Encoding.UTF8.GetBytes(fileToAttach)),
                    ct
                    ) {TransferEncoding = TransferEncoding.SevenBit});
            return attachmentDO;
        }

        private static AttachmentDO GetBase64Event(string fileToAttach)
        {
            return Email.CreateNewAttachment(
                new System.Net.Mail.Attachment(
                    new MemoryStream(Encoding.UTF8.GetBytes(fileToAttach)),
                    new ContentType {MediaType = "application/calendar", Name = "invite.ics"}
                    ) {TransferEncoding = TransferEncoding.Base64});
        }
    }
}
