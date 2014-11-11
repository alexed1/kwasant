using KwasantWeb.ViewModels;

namespace KwasantCore.Interfaces
{
    public interface INegotiationResponse
    {
        void Process(NegotiationVM curNegotiationVM, string userID);
    }
}