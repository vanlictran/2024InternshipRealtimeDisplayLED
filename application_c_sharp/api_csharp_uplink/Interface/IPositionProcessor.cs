using api_csharp_uplink.Entities;

namespace api_csharp_uplink.Interface;

public interface IPositionProcessor
{
    public Task<int> RegisterPositionCard(PositionCard positionCard);
    public Task<int> RegisterPositionOneStation(Position position);
}