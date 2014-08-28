using KwasantCore.Managers.APIManager.Packagers;

namespace Daemons.EventExposers
{
    public sealed class GmailPackagerEventHandler : ExposedEvent
    {
        public static ExposedEvent EmailSent = new GmailPackagerEventHandler("EmailSent");
        public static ExposedEvent EmailRejected = new GmailPackagerEventHandler("EmailRejected");
        public static ExposedEvent EmailCriticalError = new GmailPackagerEventHandler("EmailCriticalError");
        private GmailPackagerEventHandler(string name)
            : base(name, typeof(GmailPackager))
        {
        }
    }
}
