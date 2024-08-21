using api_csharp_uplink.Connectors.ExternalEntities;
using api_csharp_uplink.Entities;

namespace api_csharp_uplink.Interface;

public interface IGraphHelper
{
    public Task<TimeDistance> GetTimeAndDistance(Position position1, Position position2);
}