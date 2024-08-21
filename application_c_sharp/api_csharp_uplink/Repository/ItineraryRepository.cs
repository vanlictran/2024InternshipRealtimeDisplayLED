using api_csharp_uplink.DB;
using api_csharp_uplink.Entities;
using api_csharp_uplink.Repository.Interface;
namespace api_csharp_uplink.Repository;

public class ItineraryRepository(IGlobalInfluxDb globalInfluxDb) : IItineraryRepository, IConnexionRepository
{
    private const string MeasurementItinerary = "itinerary";
    private const string MeasurementStation = "station";
    public async Task<Itinerary> AddItinerary(Itinerary itinerary)
    {
        List<ConnexionDb> connexionDbs = [];
        for (int i = 0; i < itinerary.connexions.Count; i++)
            connexionDbs.Add(ConvertConnexionToDb(itinerary.connexions[i], itinerary.connexions.Count - i));
        
        await globalInfluxDb.SaveAll(connexionDbs);

        return itinerary;
    }

    public async Task<Itinerary?> FindItinerary(int lineNumber, string orientation)
    {
        string queryItinerary = $"|> filter(fn: (r) => r.lineNumber == \"{lineNumber}\" and r.orientation == \"{orientation}\")\n" +
                       $"|> sort(columns: [\"_time\"], desc: true)";
        List<ConnexionDb> listConnexions = await globalInfluxDb.Get<ConnexionDb>(MeasurementItinerary, queryItinerary);
        return listConnexions.Count == 0 ? null : new Itinerary(lineNumber, orientation, await GetAllConnexion(listConnexions));
    }

    public async Task<Itinerary?> FindItineraryBetweenStation(int lineNumber, string orientation, string station1, string station2)
    {
        string query = $"|> filter(fn: (r) => r.lineNumber == \"{lineNumber}\" and r.orientation == \"{orientation}\")\n" +
                       $"|> filter(fn: (r) => r.currentStation == \"{station1}\" or r.currentStation == \"{station2}\")\n" +
                       $"|> sort(columns: [\"_time\"], desc: true)";
        List<ConnexionDb> listConnexions = await globalInfluxDb.Get<ConnexionDb>(MeasurementItinerary, query);

        if (listConnexions.Count < 2)
            return null;
        
        DateTime timeStation1 = listConnexions[0].Timestamp;
        DateTime timeStation2 = listConnexions[1].Timestamp.AddMinutes(1);
        
        string queryItinerary = "from(bucket: \"mybucket\")\n" +
                                $"|> range(start: {timeStation1:yyyy-MM-ddTHH:mm:ssZ}, stop: {timeStation2:yyyy-MM-ddTHH:mm:ssZ})\n" +
                                $"|> filter(fn: (r) => r.lineNumber == \"{lineNumber}\" and r.orientation == \"{orientation}\")\n" +
                                "|> sort(columns: [\"_time\"], desc: true)";
        
        listConnexions = await globalInfluxDb.Get<ConnexionDb>(queryItinerary);
        return listConnexions.Count == 0 ? null : new Itinerary(lineNumber, orientation, await GetAllConnexion(listConnexions));
    }

    public async Task<bool> DeleteItinerary(int lineNumber, string orientation)
    {
        string predicateDelete =
            $"_measurement=\"itinerary\" and lineNumber=\"{lineNumber}\" and orientation=\"{orientation}\"";
        string predicate = $"|> filter(fn: (r) => r.lineNumber == \"{lineNumber}\" and r.orientation == \"{orientation}\")";
        await globalInfluxDb.Delete(predicateDelete);
        
        return (await globalInfluxDb.Get<ConnexionDb>(MeasurementItinerary, predicate)).Count == 0;
    }
    
    public async Task<ConnexionWithTime?> FindNextStation(int lineNumber, string orientation, string station)
    {
        string query =
            $"|> filter(fn: (r) => r.lineNumber == \"{lineNumber}\" and r.orientation == \"{orientation}\")\n" +
            $"|> filter(fn: (r) => r.currentStation == \"{station}\")";
        List<ConnexionDb> listConnexions = await globalInfluxDb.Get<ConnexionDb>(MeasurementItinerary, query);

        if (listConnexions.Count == 0)
            return null;
        
        return await FindConnexionWithTime(listConnexions[0].Timestamp, lineNumber, orientation);
    }

    public async Task<List<ConnexionWithTime>> FindConnexion(string stationName)
    {
        string query = $"|> filter(fn: (r) => r.currentStation == \"{stationName}\")";
        List<ConnexionDb> listConnexions = await globalInfluxDb.Get<ConnexionDb>(MeasurementItinerary, query);
        
        if (listConnexions.Count == 0)
            return [];

        return await GetAllConnexionTime(listConnexions);
    }

    private async Task<List<ConnexionWithTime>> GetAllConnexionTime(List<ConnexionDb> connexionDbs)
    {
        List<ConnexionWithTime> connexionWithTimes = [..new ConnexionWithTime[connexionDbs.Count]];
        
        await Parallel.ForAsync(0, connexionDbs.Count, async (i, _) =>
        {
            ConnexionWithTime? connexionWithTime = await FindConnexionWithTime(connexionDbs[i].Timestamp, connexionDbs[i].lineNumber, connexionDbs[i].orientation);

            if (connexionWithTime != null) 
                connexionWithTimes[i] = connexionWithTime;
        });
        
        return connexionWithTimes;
    }
    
    private async Task<ConnexionWithTime?> FindConnexionWithTime(DateTime timeStation, int lineNumber, string orientation)
    {
        DateTime timeStation2 = timeStation.AddMinutes(2);
            
        string queryConnexion = "from(bucket: \"mybucket\")\n" +
                                $"|> range(start: {timeStation:yyyy-MM-ddTHH:mm:ssZ}, stop: {timeStation2:yyyy-MM-ddTHH:mm:ssZ})\n" +
                                "|> filter(fn: (r) => r._measurement == \"itinerary\")\n" +
                                $"|> filter(fn: (r) => r.lineNumber == \"{lineNumber}\" and r.orientation == \"{orientation}\")\n" +
                                "|> sort(columns: [\"_time\"], desc: true)";
        List<ConnexionDb> listTemp = await globalInfluxDb.Get<ConnexionDb>(queryConnexion);
            
        return listTemp.Count >= 2 ? ConvertConnexionTimeFromDb(listTemp[0], listTemp[1].currentStation) : null;
    }
    
    private async Task<List<Connexion>> GetAllConnexion(List<ConnexionDb> connexionsDbs)
    {
        List<Connexion> connexions = [..new Connexion[connexionsDbs.Count]];

        await Parallel.ForAsync(0, connexionsDbs.Count, async (i, _) =>
        {
            string queryStation = $"|> filter(fn: (r) => r.nameStation == \"{connexionsDbs[i].currentStation}\")";
            List<StationDb> listStations = await globalInfluxDb.Get<StationDb>(MeasurementStation, queryStation);
            
            if (listStations.Count != 0)
            {
                Station station = ConvertDbToStation(listStations[0]);
                connexions[i] = ConvertConnexionFromDb(connexionsDbs[i], station);
            }
        });

        return connexions;
    }
    
    private static ConnexionDb ConvertConnexionToDb(Connexion connexion, int timeToReduct)
    {
        return new ConnexionDb
        {
            lineNumber = connexion.lineNumber,
            orientation = connexion.orientation.ToString(),
            currentStation = connexion.stationCurrent.NameStation,
            timeToNextStation = connexion.timeToNextStation,
            distanceToNextStation = connexion.distanceToNextStation,
            Timestamp = DateTime.Now.Subtract(new TimeSpan(0, timeToReduct, 0))
        };
    }
    
    private static ConnexionWithTime ConvertConnexionTimeFromDb(ConnexionDb connexionDb, string nextStation)
    {
        return new ConnexionWithTime(connexionDb.lineNumber, connexionDb.orientation, connexionDb.currentStation, 
            nextStation, connexionDb.timeToNextStation, connexionDb.distanceToNextStation);
    }
    
    private static Connexion ConvertConnexionFromDb(ConnexionDb connexionDb, Station stationCurrent)
    {
        return new Connexion(connexionDb.lineNumber, connexionDb.orientation, stationCurrent,
            connexionDb.timeToNextStation, connexionDb.distanceToNextStation);
    }
    
    private static Station ConvertDbToStation(StationDb stationDb)
    {
        return new Station(new Position(stationDb.Latitude, stationDb.Longitude), stationDb.NameStation);
    }
}