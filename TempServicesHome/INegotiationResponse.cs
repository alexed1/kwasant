using KwasantWeb.ViewModels;

namespace KwasantWeb.TempServicesHome
{
    public interface INegotiationResponse
    {
        void Process(NegotiationVM curNegotiationVM, string userID);
    }
}