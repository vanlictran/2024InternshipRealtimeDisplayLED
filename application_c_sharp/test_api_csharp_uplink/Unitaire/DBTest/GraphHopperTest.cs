using api_csharp_uplink.Connectors;
using api_csharp_uplink.Connectors.ExternalEntities;
using api_csharp_uplink.Entities;
using api_csharp_uplink.Interface;

namespace test_api_csharp_uplink.Unitaire.DBTest;

public class GraphHopperTest(bool useHaversine=false) : IGraphHelper
{
    public Task<TimeDistance> GetTimeAndDistance(Position position1, Position position2)
    {
        TimeDistance timeDistance = !useHaversine ? new TimeDistance(5, 5.0) : 
            new TimeDistance(5, GraphHelperService.DistanceHaversine(position1, position2));

        return Task.FromResult(timeDistance);
    }
}