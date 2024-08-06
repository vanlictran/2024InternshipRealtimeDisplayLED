using api_csharp_uplink.Entities;

namespace api_csharp_uplink.Interface;

public interface ICardRegistration
{
    Task<Card> CreateCard(int lineNumber, string devEuiCard);
    
    Task<Card> ModifyCard(int lineNumber, string devEuiCard);
}