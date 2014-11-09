using Data.Entities;
using KwasantWeb.ViewModels;

namespace KwasantCore.Services
{
    internal interface INegotiationResponse
    {
        void Process(NegotiationVM curNegotiationVM, string userID);
    }
}