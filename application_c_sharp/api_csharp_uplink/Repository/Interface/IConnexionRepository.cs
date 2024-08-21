using api_csharp_uplink.Entities;

namespace api_csharp_uplink.Repository.Interface;

public interface IConnexionRepository
{
    public Task<ConnexionWithTime?> FindNextStation(int lineNumber, string orientation, string station);
    public Task<List<ConnexionWithTime>> FindConnexion(string stationName);
}