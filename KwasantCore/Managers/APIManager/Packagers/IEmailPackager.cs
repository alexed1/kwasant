using Data.Entities;

namespace KwasantCore.Managers.APIManager.Packagers
{
    public interface IEmailPackager
    {
        void Send(EmailDO email);
    }
}