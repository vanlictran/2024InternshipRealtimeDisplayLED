using api_csharp_uplink.Dto;
using api_csharp_uplink.Entities;

namespace test_api_csharp_uplink.Unitaire.Composant;

public static class GenerateConnexion
{
    public static List<Station> AddStations()
    {
        List<Station> stations = [];

        for (int i = 1; i < 6; i++)
        {
            if (i is 1 or 5)
                stations.Add(new Station(i, i * 2, "Station" + i));
            else
            {
                stations.Add(new Station(i, i * 2, "StationF" + i));
                stations.Add(new Station(i, i * 2, "StationB" + i));
            }
        }

        return stations;
    }

    public static (List<(Station station, int time, double distance)> stationTimeDistanceForward,
        List<(Station station, int time, double distance)> stationTimeDistanceBackward)
        GetStationTimeDistance(List<Station> stations)
    {
        List<(Station station, int time, double distance)> stationTimeDistanceForward = [];
        List<(Station station, int time, double distance)> stationTimeDistanceBackward = [];

        for (int i = 0; i < stations.Count; i++)
        {
            if (i == 0 || i == stations.Count - 1)
            {
                stationTimeDistanceForward.Add((stations[i], 5, 7));
                stationTimeDistanceBackward.Add((stations[i], 5, 7));
            }
            else if (i % 2 == 1)
                stationTimeDistanceForward.Add((stations[i], 5, 7));
            else
                stationTimeDistanceBackward.Add((stations[i], 5, 7));
        }

        return (stationTimeDistanceForward, stationTimeDistanceBackward);
    }

    public static List<Connexion> AddConnexions(List<(Station station, int time, double distance)> stationTimeDistance,
        string orientation)
    {
        List<Connexion> connexions = [];

        for (int i = 0; i < stationTimeDistance.Count - 1; i++)
            connexions.Add(new Connexion(5, orientation, stationTimeDistance[i].station, stationTimeDistance[i].time,
                stationTimeDistance[i].distance));
        
        connexions.Add(new Connexion(5, orientation, stationTimeDistance[^1].station, 0, 0));

        return connexions;
    }
    
    public static List<ConnexionDto> ConvertConnexionToDto(List<Connexion> connexion)
    {
        List<ConnexionDto> connexionDtos = [..new ConnexionDto[connexion.Count - 1]];
        
        Parallel.For(0, connexion.Count - 1,i =>
        {
            Connexion connexion1 = connexion[i];
            Connexion connexion2 = connexion[i + 1];
            
            connexionDtos[i] = new ConnexionDto {CurrentNameStation = connexion1.stationCurrent.NameStation, 
                NextNameStation = connexion2.stationCurrent.NameStation};
        });

        return connexionDtos;
    }
}