using System;
using System.Collections.Generic;
using System.Linq;
using Data.Constants;
using Data.DataAccessLayer.Interfaces;
using Data.DataAccessLayer.Repositories;
using Data.Models;
using UtilitiesLib;

namespace Shnexy.Controllers.Models
{
    /// <summary>
    /// Summary description for EventManager
    /// </summary>
    public class Calendar
    {
        private readonly CustomerDO _customer;
        private readonly IUnitOfWork _uow;
        private InvitationRepository _invitationRepo;

        public Calendar(IUnitOfWork uow, CustomerDO customer)
        {
            _uow = uow;
            _customer = customer;
            LoadData(_customer);
        }

        public List<EventDO> EventsList;
        
        private void LoadData(CustomerDO customer)
        {
            _invitationRepo = new InvitationRepository(_uow);
            EventsList = _invitationRepo.GetQuery().Where(i => i.BookingRequest.Customer.CustomerID == customer.CustomerID).ToList();
        }

        public void Reload()
        {
            LoadData(_customer);
        }

        public void AddEvent(EventDO eventDo)
        {
            EventsList.Add(eventDo);
        }

        public void DeleteEvent(String idStr)
        {
            int id = idStr.ToInt();
            EventDO eventToDelete = EventsList.FirstOrDefault(inv => inv.EventID == id);
            if (eventToDelete != null)
            {
                EventsList.Remove(eventToDelete);
                /* To be confirmed. Currently, if an event is deleted, the booking requests (emails) are set back to unprocessed. We may be immediately dispatching an event, 
                * which means this code is invalid. */
                List<EmailDO> previouslyProcessedEmails = eventToDelete.Emails.Where(e => e.StatusID == EmailStatusConstants.PROCESSED).ToList();
                if (previouslyProcessedEmails.Any())
                {
                    foreach (EmailDO previouslyProcessedEmail in previouslyProcessedEmails)
                    {
                        previouslyProcessedEmail.StatusID = EmailStatusConstants.UNPROCESSED;
                    }
                }

                _invitationRepo.Remove(eventToDelete);
                
                _uow.SaveChanges();
            }
        }

        public void MoveEvent(String idStr, DateTime newStart, DateTime newEnd)
        {
            int id = Int32.Parse(idStr);
            EventDO itemToMove = EventsList.FirstOrDefault(inv => inv.EventID == id);
            if (itemToMove != null)
            {
                itemToMove.StartDate = newStart;
                itemToMove.EndDate = newEnd;
                _uow.SaveChanges();
            }
        }
    }
}
