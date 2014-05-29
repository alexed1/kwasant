using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Linq;
using System.Transactions;
using Data.Interfaces;
using Data.Repositories;

namespace Data.Infrastructure
{
    public class UnitOfWork : IUnitOfWork
    {
        private TransactionScope _transaction;
        private readonly IDBContext _context;

        internal UnitOfWork(IDBContext context)
        {
            context.UnitOfWork = this;
            _context = context;
        }

        private AttachmentRepository _attachmentRepository;

        public AttachmentRepository AttachmentRepository
        {
            get
            {
                return _attachmentRepository ?? (_attachmentRepository = new AttachmentRepository(_context));
            }
        }

        private AttendeeRepository _attendeeRepository;

        public AttendeeRepository AttendeeRepository
        {
            get
            {
                return _attendeeRepository ?? (_attendeeRepository = new AttendeeRepository(_context));
            }
        }

        private EmailAddressRepository _emailAddressRepository;

        public EmailAddressRepository EmailAddressRepository
        {
            get
            {
                return _emailAddressRepository ?? (_emailAddressRepository = new EmailAddressRepository(_context));
            }
        }

        private RecipientRepository _recipientRepository;
        public RecipientRepository RecipientRepository
        {
            get
            {
                return _recipientRepository ?? (_recipientRepository = new RecipientRepository(_context));
            }
        }

        private BookingRequestRepository _bookingRequestRepository;

        public BookingRequestRepository BookingRequestRepository
        {
            get
            {
                return _bookingRequestRepository ?? (_bookingRequestRepository = new BookingRequestRepository(_context));
            }
        }

        private CalendarRepository _calendarRepository;

        public CalendarRepository CalendarRepository
        {
            get
            {
                return _calendarRepository ?? (_calendarRepository = new CalendarRepository(_context));
            }
        }

        private CommunicationConfigurationRepository _communicationConfigurationRepository;

        public CommunicationConfigurationRepository CommunicationConfigurationRepository
        {
            get
            {
                return _communicationConfigurationRepository ??
                       (_communicationConfigurationRepository = new CommunicationConfigurationRepository(_context));
            }
        }

        private EmailRepository _emailRepository;

        public EmailRepository EmailRepository
        {
            get
            {
                return _emailRepository ?? (_emailRepository = new EmailRepository(_context));
            }
        }

        private EventRepository _eventRepository;

        public EventRepository EventRepository
        {
            get
            {
                return _eventRepository ?? (_eventRepository = new EventRepository(_context));
            }
        }

        private InstructionRepository _instructionRepository;

        public InstructionRepository InstructionRepository
        {
            get
            {
                return _instructionRepository ?? (_instructionRepository = new InstructionRepository(_context));
            }
        }
        
        private StoredFileRepository _storedFileRepository;

        public StoredFileRepository StoredFileRepository
        {
            get
            {
                return _storedFileRepository ?? (_storedFileRepository = new StoredFileRepository(_context));
            }
        }

        private TrackingStatusRepository _trackingStatusRepository;

        public TrackingStatusRepository TrackingStatusRepository
        {
            get
            {
                return _trackingStatusRepository ?? (_trackingStatusRepository = new TrackingStatusRepository(_context));
            }
        }

        private UserRepository _userRepository;

        public UserRepository UserRepository
        {
            get
            {
                return _userRepository ?? (_userRepository = new UserRepository(_context));
            }
        }

        public void Save()
        {
            _context.SaveChanges();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_transaction != null)
                _transaction.Dispose();
            _context.Dispose();
        }

        public void StartTransaction()
        {
            _transaction = new TransactionScope();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void Commit()
        {
            _context.SaveChanges();
            _transaction.Complete();
            _transaction.Dispose();
        }

        public void SaveChanges()
        {
            try
            {
                _context.SaveChanges();
            }
            catch (DbEntityValidationException e)
            {
                string errorFormat = @"Validation failed for entity [{0}]. Validation errors:" + Environment.NewLine + @"{1}";
                var errorList = new List<String>();
                foreach (var entityValidationError in e.EntityValidationErrors)
                {
                    var entityName = entityValidationError.Entry.Entity.GetType().Name;
                    var errors = String.Join(Environment.NewLine, entityValidationError.ValidationErrors.Select(a => a.PropertyName + ": " + a.ErrorMessage));
                    errorList.Add(String.Format(errorFormat, entityName, errors));
                }
                throw new Exception(String.Join(Environment.NewLine + Environment.NewLine, errorList) + Environment.NewLine, e);
            }
            
        }

        public IDBContext Db
        {
            get { return _context; }
        }
    }
}
