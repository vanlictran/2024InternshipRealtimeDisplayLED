using api_csharp_uplink.Entities;

namespace api_csharp_uplink.Interface;

public interface IStationRepository
{
    public Task<Station> Add(Station station);
    public Task<Station?> GetStation(string nameStation);
    public Task<Station?> GetStation(Position position);
}