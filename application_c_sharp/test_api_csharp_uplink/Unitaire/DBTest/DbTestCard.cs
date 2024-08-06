using api_csharp_uplink.Entities;
using api_csharp_uplink.Interface;

namespace test_api_csharp_uplink.Unitaire.DBTest;

public class DbTestCard : ICardRepository
{
    private List<Card> _cards = [];
    
    public Task<Card> Add(Card card)
    {
        _cards.Add(card);
        return Task.FromResult(card);
    }

    public Task<Card?> GetByDevEui(string devEuiCard)
    {
        Card? cardFind = _cards.Find(card => card.DevEuiCard == devEuiCard);
        return Task.FromResult(cardFind);
    }

    public Task<Card> Modify(Card card)
    {
        _cards = _cards.Where(cardd => cardd.DevEuiCard != card.DevEuiCard).ToList();
        _cards.Add(card);
        return Task.FromResult(card);
    }

    public Task<List<Card>> GetAll()
    {
        return Task.FromResult(_cards);
    }
}