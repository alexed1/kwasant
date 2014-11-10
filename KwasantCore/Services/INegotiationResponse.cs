using Data.Entities;
using KwasantWeb.ViewModels;

namespace KwasantCore.Services
{
    public interface INegotiationResponse
    {
        void Process(NegotiationVM curNegotiationVM, string userID);
    }
}