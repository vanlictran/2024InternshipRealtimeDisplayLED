namespace test_api_csharp_uplink.Unitaire.DBTest;

using api_csharp_uplink.Entities;
using api_csharp_uplink.Interface;

public class DbTestPosition : IPositionRepository
{
    private readonly List<PositionCard> _positionCards = [];

    public Task<PositionCard> Add(PositionCard positionCard)
    {
        _positionCards.Add(positionCard);
        return Task.FromResult(positionCard);
    }

    public Task<PositionCard?> GetLast(string devEuiCard)
    {
        List<PositionCard> list = _positionCards.FindAll(position => position.DevEuiCard.Equals(devEuiCard));
        return Task.FromResult(list.Count > 0 ? list[^1] : null);
    }
}