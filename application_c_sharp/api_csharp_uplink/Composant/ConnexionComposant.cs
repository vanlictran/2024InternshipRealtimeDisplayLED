using api_csharp_uplink.DirException;
using api_csharp_uplink.Entities;
using api_csharp_uplink.Interface;
using api_csharp_uplink.Repository.Interface;

namespace api_csharp_uplink.Composant;

public class ConnexionComposant(IConnexionRepository connexionRepository) : IConnexionFinder
{
    public async Task<ConnexionWithTime> FindConnexion(int lineNumber, string orientation, string station)
    {
        if (lineNumber < 0)
            throw new ArgumentOutOfRangeException(nameof(lineNumber), "Line number must be positive");
        Enum.Parse<Orientation>(orientation);
        if (string.IsNullOrWhiteSpace(station))
            throw new ArgumentNullException(nameof(station), "Station name cannot be empty or whitespace");
        
        return await connexionRepository.FindNextStation(lineNumber, orientation, station) ?? 
               throw new NotFoundException($"Connexion not found with line {lineNumber}, orientation {orientation} and station {station}");
    }

    public async Task<List<ConnexionWithTime>> FindNextConnexion(string station)
    {
        if (string.IsNullOrWhiteSpace(station))
            throw new ArgumentNullException(nameof(station), "Station name cannot be empty or whitespace");

        List<ConnexionWithTime> connexionWithTimes = await connexionRepository.FindConnexion(station);
        return connexionWithTimes;
    }
}