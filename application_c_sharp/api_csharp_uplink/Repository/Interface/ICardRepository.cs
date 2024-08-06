using api_csharp_uplink.Entities;

namespace api_csharp_uplink.Interface;

public interface ICardRepository
{
    public Task<Card> Add(Card card);
    public Task<Card?> GetByDevEui(string devEuiCard);
    public Task<Card> Modify(Card card);
    public Task<List<Card>> GetAll();
}