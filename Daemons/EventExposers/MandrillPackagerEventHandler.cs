using KwasantCore.Managers.APIManager.Packagers.Mandrill;

namespace Daemons.EventExposers
{
    public sealed class MandrillPackagerEventHandler : ExposedEvent
    {
        public static ExposedEvent EmailSent = new MandrillPackagerEventHandler("EmailSent");
        public static ExposedEvent EmailRejected = new MandrillPackagerEventHandler("EmailRejected");
        public static ExposedEvent EmailCriticalError = new MandrillPackagerEventHandler("EmailCriticalError");
        private MandrillPackagerEventHandler(string name)
            : base(name, typeof(MandrillPackager))
        {
        }
    }
}
