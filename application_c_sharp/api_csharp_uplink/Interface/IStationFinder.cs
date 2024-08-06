using api_csharp_uplink.Entities;

namespace api_csharp_uplink.Interface;

public interface IStationFinder
{
    public Task<Station> GetStation(string nameStation);
    public Task<Station> GetStation(double latitude, double longitude);
}