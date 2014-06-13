﻿using System;
using System.Data.Entity;
using Data.Infrastructure;
using Data.Repositories;

namespace Data.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        #region Method

        AttachmentRepository AttachmentRepository { get; }
        AttendeeRepository AttendeeRepository { get; }
        EmailAddressRepository EmailAddressRepository { get; }
        RecipientRepository RecipientRepository { get; }
        BookingRequestRepository BookingRequestRepository { get; }
        CalendarRepository CalendarRepository { get; }
        ClarificationRequestRepository ClarificationRequestRepository { get; }
        CommunicationConfigurationRepository CommunicationConfigurationRepository { get; }
        EmailRepository EmailRepository { get; }
        EnvelopeRepository EnvelopeRepository { get; }
        EventRepository EventRepository { get; }
        InstructionRepository InstructionRepository { get; }
        StoredFileRepository StoredFileRepository { get; }
        TrackingStatusRepository TrackingStatusRepository { get; }
        UserRepository UserRepository { get; }
        AspNetUserRolesRepository AspNetUserRolesRepository { get; }
        AspNetRolesRepository AspNetRolesRepository { get; }

        /// <summary>
        /// Call this to commit the unit of work
        /// </summary>
        void Commit();

        /// <summary>
        /// Return the database reference for this UOW
        /// </summary>
        IDBContext Db { get; }

        /// <summary>
        /// Starts a transaction on this unit of work
        /// </summary>
        void StartTransaction();

        /// <summary>
        /// The save changes.
        /// </summary>
        void SaveChanges();

        /// <summary>
        /// The save changes.
        /// </summary>
        // void SaveChanges(SaveOptions saveOptions);
        #endregion
    }
}
