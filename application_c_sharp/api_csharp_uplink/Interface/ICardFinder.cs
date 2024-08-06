using api_csharp_uplink.Entities;

namespace api_csharp_uplink.Interface;

public interface ICardFinder
{
    Task<List<Card>> GetCards();
    Task<Card> GetCardByDevEuiCard(string devEuiCard);
}