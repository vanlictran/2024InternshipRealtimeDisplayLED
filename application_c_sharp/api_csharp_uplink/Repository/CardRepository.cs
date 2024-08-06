using api_csharp_uplink.DB;
using api_csharp_uplink.Entities;
using api_csharp_uplink.Interface;

namespace api_csharp_uplink.Repository;

public class CardRepository(IGlobalInfluxDb globalInfluxDb) : ICardRepository
{
    private const string MeasurementCard = "card";

    public async Task<Card> Add(Card card) {
        CardDb cardDb = await globalInfluxDb.Save(ConvertCardToDb(card));
        return ConvertDbToCard(cardDb);        
    }
    

    public async Task<Card?> GetByDevEui(string devEuiCard)
    {
        string query = $"   |> filter(fn: (r) => r.devEuiCard == \"{devEuiCard}\")";
        List<CardDb> list = await globalInfluxDb.Get<CardDb>(MeasurementCard, query);
        return list.Count > 0 ? ConvertDbToCard(list[0]) : null;
    }
    
    public async Task<Card> Modify(Card card)
    {
        string predicate = $"devEuiCard=\"{card.DevEuiCard}\"";
        await globalInfluxDb.Delete(predicate);
        
        CardDb cardDb = await globalInfluxDb.Save(ConvertCardToDb(card));
        return ConvertDbToCard(cardDb);
    }

    public async Task<List<Card>> GetAll()
    {
        List<CardDb> cardDbs = await globalInfluxDb.GetAll<CardDb>(MeasurementCard);
        return cardDbs.Select(ConvertDbToCard).ToList();           
    }
    
    private static CardDb ConvertCardToDb(Card card)
    {
        return new CardDb
        {
            DevEuiCard = card.DevEuiCard,
            LineBus = card.LineBus,
            Timestamp = DateTime.Now
        };
    }
    
    private static Card ConvertDbToCard(CardDb cardDb)
    {
        return new Card(cardDb.DevEuiCard, cardDb.LineBus);
    }
}