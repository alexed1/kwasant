//We rename .NET style "events" to "alerts" to avoid confusion with our business logic Alert concepts

using System;
using Data.Entities;
using Data.Interfaces;

namespace Data.Infrastructure
{
    //this class serves as both a registry of all of the defined alerts as well as a utility class.
    public static class AlertManager
    {
        public delegate void ResponseRecievedHandler(int bookingRequestId, String bookerID, String customerID);
        public static event ResponseRecievedHandler AlertResponseReceived;

        public delegate void BookingRequestNeedsProcessingHandler(int bookingRequestId);
        public static event BookingRequestNeedsProcessingHandler AlertBookingRequestNeedsProcessing;

        public delegate void TrackablePropertyUpdatedHandler(string name, string contextTable, object id, object status);
        public static event TrackablePropertyUpdatedHandler AlertTrackablePropertyUpdated;

        public delegate void TrackablePropertyCreatedHandler(string name, string contextTable, int id, object status);
        public static event TrackablePropertyCreatedHandler AlertTrackablePropertyCreated;

        public delegate void TrackablePropertyDeletedHandler(string name, string contextTable, int id, int parentId, object status);
        public static event TrackablePropertyDeletedHandler AlertTrackablePropertyDeleted;

        public delegate void ConversationMemberAddedHandler(int bookingRequestID);
        public static event ConversationMemberAddedHandler AlertConversationMemberAdded;
        
        public delegate void ConversationmatchedHandler(int emailID, string subject, int bookingRequestID);
        public static event ConversationmatchedHandler AlertConversationMatched;

        public delegate void ExplicitCustomerCreatedHandler(string curUserId);
        public static event ExplicitCustomerCreatedHandler AlertExplicitCustomerCreated;

        public delegate void PostResolutionNegotiationResponseReceivedHandler(int negotiationId);
        public static event PostResolutionNegotiationResponseReceivedHandler AlertPostResolutionNegotiationResponseReceived;

        public delegate void CustomerCreatedHandler(UserDO user);
        public static event CustomerCreatedHandler AlertCustomerCreated;

        public delegate void BookingRequestCreatedHandler(int bookingRequestId);
        public static event BookingRequestCreatedHandler AlertBookingRequestCreated;

        public delegate void EmailReceivedHandler(int emailId, string customerId);
        public static event EmailReceivedHandler AlertEmailReceived;

        public delegate void EventBookedHandler(int eventId, string customerId);
        public static event EventBookedHandler AlertEventBooked;

        public delegate void EmailSentHandler(int emailId, string customerId);
        public static event EmailSentHandler AlertEmailSent;

        public delegate void EmailProcessingHandler(string dateReceived, string errorMessage);
        public static event EmailProcessingHandler AlertEmailProcessingFailure;

        public delegate void BookingRequestStateChangeHandler(int bookingRequestId);
        public static event BookingRequestStateChangeHandler AlertBookingRequestStateChange;

        public delegate void BookingRequestTimeoutStateChangeHandler(int bookingRequestId, string bookerId);
        public static event BookingRequestTimeoutStateChangeHandler AlertBookingRequestProcessingTimeout;

        public delegate void BookingRequestReservedHandler(int bookingRequestId, string bookerId);
        public static event BookingRequestReservedHandler AlertBookingRequestReserved;

        public delegate void BookingRequestReservationTimeoutHandler(int bookingRequestId, string bookerId);
        public static event BookingRequestReservationTimeoutHandler AlertBookingRequestReservationTimeout;

        public delegate void UserRegistrationHandler(UserDO curUser);
        public static event UserRegistrationHandler AlertUserRegistration;

        public delegate void BookingRequestCheckedOutHandler(int bookingRequestId, string bookerId);
        public static event BookingRequestCheckedOutHandler AlertBookingRequestCheckedOut;

        public delegate void BookingRequestOwnershipChangeHandler(int bookingRequestId, string bookerId);
        public static event BookingRequestOwnershipChangeHandler AlertBookingRequestOwnershipChange;

        public delegate void Error_EmailSendFailureHandler(int emailId, string message);
        public static event Error_EmailSendFailureHandler AlertError_EmailSendFailure;

        public delegate void ErrorSyncingCalendarHandler(IRemoteCalendarAuthDataDO authData, IRemoteCalendarLinkDO calendarLink = null);
        public static event ErrorSyncingCalendarHandler AlertErrorSyncingCalendar;

        #region Method

        public static void ResponseReceived(int bookingRequestId, String bookerID, String customerID)
        {
            if (AlertResponseReceived != null)
                AlertResponseReceived(bookingRequestId, bookerID, customerID);
        }

        public static void TrackablePropertyUpdated(string entityName, string propertyName, object id, object value)
        {
            if (AlertTrackablePropertyUpdated != null)
                AlertTrackablePropertyUpdated(entityName, propertyName, id, value);
        }

        public static void TrackablePropertyCreated(string name, string contextTable, int id, object status)
        {
            if (AlertTrackablePropertyCreated != null)
                AlertTrackablePropertyCreated(name, contextTable, id, status);
        }

        public static void TrackablePropertyDeleted(string name, string contextTable, int id, int parentID, object status)
        {
            if (AlertTrackablePropertyDeleted != null)
                AlertTrackablePropertyDeleted(name, contextTable, id, parentID, status);
        }

        public static void ConversationMemberAdded(int bookingRequestID)
        {
            if (AlertConversationMemberAdded != null)
                AlertConversationMemberAdded(bookingRequestID);
        }

        public static void ConversationMatched(int emailID, string subject, int bookingRequestID)
        {
            if (AlertConversationMatched != null)
                AlertConversationMatched(emailID, subject, bookingRequestID);
        }

        /// <summary>
        /// Publish Customer Created event
        /// </summary>
        public static void ExplicitCustomerCreated(string curUserId)
        {
            if (AlertExplicitCustomerCreated != null)
                AlertExplicitCustomerCreated(curUserId);
        }

        public static void PostResolutionNegotiationResponseReceived(int negotiationDO)
        {
            if (AlertPostResolutionNegotiationResponseReceived != null)
                AlertPostResolutionNegotiationResponseReceived(negotiationDO);
        }

        public static void CustomerCreated(UserDO user)
        {
            if (AlertCustomerCreated != null)
                AlertCustomerCreated(user);
        }

        public static void BookingRequestCreated(int bookingRequestId)
        {
            if (AlertBookingRequestCreated != null)
                AlertBookingRequestCreated(bookingRequestId);
        }
            
        public static void EmailReceived(int emailId, string customerId)
        {
            if (AlertEmailReceived != null)
                AlertEmailReceived(emailId, customerId);
        }
        public static void EventBooked(int eventId, string customerId)
        {
            if (AlertEventBooked != null)
                AlertEventBooked(eventId, customerId);
        }
        public static void EmailSent(int emailId, string customerId)
        {
            if (AlertEmailSent != null)
                AlertEmailSent(emailId, customerId);
        }
                
        public static void EmailProcessingFailure(string dateReceived, string errorMessage)
        {
            if (AlertEmailProcessingFailure != null)
                AlertEmailProcessingFailure(dateReceived, errorMessage);
        }

        public static void BookingRequestStateChange(int bookingRequestId)
        {
            if (AlertBookingRequestStateChange != null)
                AlertBookingRequestStateChange(bookingRequestId);
        }
        public static void BookingRequestProcessingTimeout(int bookingRequestId, string bookerId)
        {
            if (AlertBookingRequestProcessingTimeout != null)
                AlertBookingRequestProcessingTimeout(bookingRequestId, bookerId);
        }

        public static void BookingRequestReserved(int bookingRequestId, string bookerId)
        {
            BookingRequestReservedHandler handler = AlertBookingRequestReserved;
            if (handler != null) handler(bookingRequestId, bookerId);
        }

        public static void BookingRequestReservationTimeout(int bookingRequestId, string bookerId)
        {
            BookingRequestReservationTimeoutHandler handler = AlertBookingRequestReservationTimeout;
            if (handler != null) handler(bookingRequestId, bookerId);
        }

        public static void UserRegistration(UserDO curUser)
        {
            if (AlertUserRegistration != null)
                AlertUserRegistration(curUser);
        }

        public static void BookingRequestCheckedOut(int bookingRequestId, string bookerId)
        {
            if (AlertBookingRequestStateChange != null)
                AlertBookingRequestCheckedOut(bookingRequestId, bookerId);
        }

        public static void BookingRequestBookerChange(int bookingRequestId, string bookerId)
        {
            if (AlertBookingRequestStateChange != null)
                AlertBookingRequestOwnershipChange(bookingRequestId, bookerId);
        }

        public static void Error_EmailSendFailure(int emailId, string message)
        {
            if (AlertError_EmailSendFailure != null)
                AlertError_EmailSendFailure(emailId, message);
        }

        public static void ErrorSyncingCalendar(IRemoteCalendarAuthDataDO authData, IRemoteCalendarLinkDO calendarLink = null)
        {
            var handler = AlertErrorSyncingCalendar;
            if (handler != null)
                handler(authData, calendarLink);
        }

        public static void BookingRequestNeedsProcessing(int bookingRequestId)
        {
            var handler = AlertBookingRequestNeedsProcessing;
            if (handler != null)
                handler(bookingRequestId);
        }

        #endregion
    }
}