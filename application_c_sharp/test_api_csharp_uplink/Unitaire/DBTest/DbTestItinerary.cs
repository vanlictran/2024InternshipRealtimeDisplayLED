using api_csharp_uplink.Entities;
using api_csharp_uplink.Repository.Interface;

namespace test_api_csharp_uplink.Unitaire.DBTest;

public class DbTestItinerary : IItineraryRepository, IConnexionRepository
{
    private List<Itinerary> _itineraries = [];

    public Task<Itinerary> AddItinerary(Itinerary itinerary)
    {
        _itineraries.Add(itinerary);
        return Task.FromResult(itinerary);
    }

    public Task<Itinerary?> FindItinerary(int lineNumber, string orientation)
    {
        Itinerary? itinerary = _itineraries.Find(i => i.lineNumber == lineNumber
                                                      && i.orientation.ToString() == orientation);

        return Task.FromResult(itinerary);
    }

    public Task<Itinerary?> FindItineraryBetweenStation(int lineNumber, string orientation, string station1,
        string station2)
    {
        Itinerary? itinerary = _itineraries.Find(i => i.lineNumber == lineNumber
                                                      && i.orientation.ToString() == orientation);
        if (itinerary == null)
            return Task.FromResult<Itinerary?>(null);

        List<Connexion> connexions = [];
        bool findStation1 = false;

        foreach (var connexion in itinerary.connexions)
        {
            if (findStation1)
            {
                connexions.Add(connexion);

                if (connexion.stationCurrent.NameStation == station2)
                    break;
            }
            else if (connexion.stationCurrent.NameStation == station1)
            {
                connexions.Add(connexion);
                findStation1 = true;
            }
        }

        Itinerary newItinerary = new Itinerary(itinerary.lineNumber, itinerary.orientation.ToString(), connexions);
        return Task.FromResult<Itinerary?>(newItinerary);
    }

    public Task<bool> DeleteItinerary(int lineNumber, string orientation)
    {
        _itineraries = _itineraries.Where(itinerary =>
            itinerary.lineNumber != lineNumber && itinerary.orientation.ToString() != orientation).ToList();

        return Task.FromResult(true);
    }

    public Task<ConnexionWithTime?> FindNextStation(int lineNumber, string orientation, string station)
    {
        Itinerary? itinerary = _itineraries.Find(itinerary1 =>
            itinerary1.lineNumber == lineNumber && itinerary1.orientation.ToString() == orientation);

        if (itinerary == null)
            return Task.FromResult<ConnexionWithTime?>(null);

        int indexConnexion =
            itinerary.connexions.FindIndex(connexion => connexion.stationCurrent.NameStation == station);

        if (indexConnexion == -1 || indexConnexion == itinerary.connexions.Count - 1)
            return Task.FromResult<ConnexionWithTime?>(null);

        Connexion connexionCurrent = itinerary.connexions[indexConnexion];
        Station stationCurrent = connexionCurrent.stationCurrent;
        Station stationNext = itinerary.connexions[indexConnexion + 1].stationCurrent;

        ConnexionWithTime connexionWithTime = new ConnexionWithTime(lineNumber, orientation, stationCurrent.NameStation,
            stationNext.NameStation, connexionCurrent.timeToNextStation, connexionCurrent.distanceToNextStation);

        return Task.FromResult<ConnexionWithTime?>(connexionWithTime);
    }

    public Task<List<ConnexionWithTime>> FindConnexion(string stationName)
    {
        List<ConnexionWithTime> connexions = [];

        foreach (var itinerary in _itineraries)
        {
            for (int i = 0; i < itinerary.connexions.Count - 1; i++)
            {
                Connexion connexion = itinerary.connexions[i];
                Station stationCurrent = connexion.stationCurrent;

                if (stationCurrent.NameStation == stationName)
                {
                    ConnexionWithTime connexionWithTime = new ConnexionWithTime(connexion.lineNumber,
                        connexion.orientation.ToString(),
                        stationCurrent.NameStation, itinerary.connexions[i + 1].stationCurrent.NameStation,
                        connexion.timeToNextStation, connexion.distanceToNextStation);

                    connexions.Add(connexionWithTime);
                }
            }
        }

        return Task.FromResult(connexions);
    }
}