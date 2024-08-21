using api_csharp_uplink.Entities;

namespace api_csharp_uplink.Interface;

public interface IConnexionFinder
{
    public Task<ConnexionWithTime> FindConnexion(int lineNumber, string orientation, string station);
    public Task<List<ConnexionWithTime>> FindNextConnexion(string station);
}