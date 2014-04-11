using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web.Mvc;
using Data.Constants;
using Data.DataAccessLayer.Interfaces;
using Data.DataAccessLayer.Repositories;
using Data.Models;
using DayPilot.Web.Mvc.Recurrence;
using StructureMap;

namespace Shnexy.Controllers.Data
{
    /// <summary>
    /// Summary description for EventManager
    /// </summary>
    public class EventManager
    {
        private Controller controller;
        private readonly string _fromEmailAddress;
        private IUnitOfWork _uow;
        private InvitationRepository _invitationRepository;

        public EventManager(Controller controller, string fromEmailAddress)
        {
            this.controller = controller;
            _fromEmailAddress = fromEmailAddress;
            LoadData(_fromEmailAddress);
        }

        public List<Invitation> Data;

        //public DataTable FilteredData(DateTime start, DateTime end, string keyword)
        //{
        //    string where = String.Format("NOT (([end] <= '{0:s}') OR ([start] >= '{1:s}')) and [text] like '%{2}%'", start, end, keyword);
        //    DataRow[] rows = Data.Select(where);
        //    DataTable filtered = Data.Clone();

        //    foreach (DataRow r in rows)
        //    {
        //        filtered.ImportRow(r);
        //    }

        //    return filtered;
        //}

        private void LoadData(String fromEmailAddress)
        {
            _uow = ObjectFactory.GetInstance<IUnitOfWork>();
            _invitationRepository = new InvitationRepository(_uow);
            Data = _invitationRepository.GetQuery().Where(i => i.Emails.Any(e => e.From.Address == fromEmailAddress)).ToList();
        }

        public void Reload()
        {
            LoadData(_fromEmailAddress);
        }

        public void EventAdd(Invitation invitation)
        {
            Data.Add(invitation);
        }

        public void EventDelete(String idStr)
        {
            int id = Int32.Parse(idStr);
            var itemToDelete = Data.FirstOrDefault(inv => inv.InvitationID == id);
            if (itemToDelete != null)
            {
                Data.Remove(itemToDelete);
                var previouslyProcessedEmails = itemToDelete.Emails.Where(e => e.StatusID == EmailStatusConstants.PROCESSED).ToList();
                if (previouslyProcessedEmails.Any())
                {
                    foreach (var previouslyProcessedEmail in previouslyProcessedEmails)
                    {
                        previouslyProcessedEmail.StatusID = EmailStatusConstants.UNPROCESSED;
                    }
                }

                _invitationRepository.Attach(itemToDelete);
                _invitationRepository.Remove(itemToDelete);
                

                _uow.SaveChanges();
            }
        }

        public void EventMove(String idStr, DateTime newStart, DateTime newEnd)
        {
            int id = Int32.Parse(idStr);
            var itemToMove = Data.FirstOrDefault(inv => inv.InvitationID == id);
            if (itemToMove != null)
            {
                itemToMove.StartDate = newStart;
                itemToMove.EndDate = newEnd;
                _uow.SaveChanges();
            }
        }

        //public void EventEdit(string id, string name)
        //{
        //    DataRow dr = Data.Rows.Find(id);
        //    if (dr != null)
        //    {
        //        dr["text"] = name;
        //        Data.AcceptChanges();
        //    }
        //}

        //public void EventMove(string id, DateTime start, DateTime end, string resource)
        //{
        //    DataRow dr = Data.Rows.Find(id);
        //    if (dr != null)
        //    {
        //        dr["start"] = start;
        //        dr["end"] = end;
        //        dr["resource"] = resource;
        //        Data.AcceptChanges();
        //    }
        //}

        //public void EventMove(string id, DateTime start, DateTime end)
        //{
        //    DataRow dr = Data.Rows.Find(id);
        //    if (dr != null)
        //    {
        //        dr["start"] = start;
        //        dr["end"] = end;
        //        Data.AcceptChanges();
        //    }
        //    else // external drag&drop
        //    {
            
        //    }
        //}

        //public Event Get(string id)
        //{
        //    DataRow dr = Data.Rows.Find(id);
        //    if (dr == null)
        //    {
        //        //return new Event();
        //        return null;
        //    }
        //    return new Event()
        //    {
        //        Id = (string) dr["id"],
        //        Text = (string) dr["text"]
        //    };
        //}
        //internal void EventCreate(DateTime start, DateTime end, string text, string resource, string id)
        //{
        //    DataRow dr = Data.NewRow();

        //    dr["id"] = id;
        //    dr["start"] = start;
        //    dr["end"] = end;
        //    dr["text"] = text;
        //    dr["resource"] = resource;

        //    Data.Rows.Add(dr);
        //    Data.AcceptChanges();
        //}

        //internal void EventCreate(DateTime start, DateTime end, string text, string resource)
        //{
        //    EventCreate(start, end, text, resource, Guid.NewGuid().ToString());
        //}

        //public class Event
        //{
        //    public string Id { get; set; }
        //    public string Text { get; set; }
        //    public DateTime Start { get; set; }
        //    public DateTime End { get; set; }
        //}

        //public void EventDelete(string id)
        //{
        //    DataRow dr = Data.Rows.Find(id);
        //    if (dr != null)
        //    {
        //        Data.Rows.Remove(dr);
        //        Data.AcceptChanges();
        //    }
        //}
    }
}
