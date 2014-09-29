using System;

namespace KwasantCore.Managers.APIManagers.Packagers.Mandrill.APIStructures
{
    public class MandrillResponse
    {
        public const String MandrilSent = "sent";
        public const String MandrilQueued = "queued";
        public const String MandrilScheduled = "scheduled";
        public const String MandrilRejected = "rejected";
        public const String MandrilInvalid = "invalid";
        public const String MandrilError = "error";

        public String Email;
        public String Status;
        public String RejectReason;
        public String _ID;
        public int Code;
        public String Name;
        public String Message;
    }
}