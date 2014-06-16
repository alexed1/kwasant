namespace Data.Entities.Enumerations
{

    //DO NOT USE THIS for anything other than Emails. Don't use it for things that inherit from email. 
    public enum EmailStatus
    {
        QUEUED = 1,
        SENT = 2,
        UNPROCESSED = 3,
        PROCESSED = 4,
        DISPATCHED = 5,
        SEND_REJECTED = 6,
        SEND_CRITICAL_ERROR = 7,
        INVALID = 8,
        UNSTARTED = 9
    }
}
