using api_csharp_uplink.Entities;

namespace api_csharp_uplink.Interface;

public interface IPositionRepository
{
    public Task<PositionCard> Add(PositionCard positionCard);
    public Task<PositionCard?> GetLast(string devEuiCard);
}