using api_csharp_uplink.Entities;
using api_csharp_uplink.Interface;

namespace test_api_csharp_uplink.Unitaire.DBTest;

public class DbTestStation : IStationRepository
{
    
    private readonly List<Station> _stations = [];
    
    public Task<Station> Add(Station station)
    {
        _stations.Add(station);
        return Task.FromResult(station);
    }

    public Task<Station?> GetStation(string nameStation)
    {
        Station? station = _stations.Find(station => station.NameStation.Equals(nameStation));
        return Task.FromResult(station);
    }

    public Task<Station?> GetStation(Position position)
    {
        Station? station = _stations.Find(station => station.Position.Equals(position));
        return Task.FromResult(station);
    }
    
}