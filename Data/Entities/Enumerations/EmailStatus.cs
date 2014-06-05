namespace Data.Entities.Enumerations
{
    public enum EmailStatus
    {
        QUEUED = 1,
        SENT = 2,
        UNPROCESSED = 3,
        PROCESSED = 4,
        DISPATCHED = 5,
        SEND_REJECTED = 6,
        SEND_CRITICAL_ERROR = 7,
        INVALID = 8
    }
    //public static class EmailStatus
    //{
    //    private static string _invalid = "INVALID";
    //    private static string _queued = "QUEUED";
    //    private static string _sent = "SENT";
    //    private static string _unprocessed = "UNPROCESSED";
    //    private static string _processed = "PROCESSED";
    //    private static string _dispatched = "DISPATCHED";
    //    private static string _sendreject = "SEND_REJECTED";
    //    private static string _senderror = "SEND_CRITICAL_ERROR";

    //    public static string INVALID
    //    { get { return _invalid; } }

    //    public static string QUEUED
    //    { get { return _queued; } }

    //    public static string SENT
    //    { get { return _sent; } }

    //    public static string UNPROCESSED
    //    { get { return _unprocessed; } }

    //    public static string PROCESSED
    //    { get { return _processed; } }

    //    public static string DISPATCHED
    //    { get { return _dispatched; } }

    //    public static string SEND_REJECTED
    //    { get { return _sendreject; } }

    //    public static string SEND_CRITICAL_ERROR
    //    { get { return _senderror; } }

    //}
}
