using api_csharp_uplink.DB;
using api_csharp_uplink.Entities;
using api_csharp_uplink.Repository;
using Moq;

namespace test_api_csharp_uplink.Unitaire.Repository;

public class ItineraryRepositoryTest
{
    private const string MeasurementItinerary = "itinerary";
    private static readonly Station Station0 = new(0, 0, "Station0");
    private static readonly Station Station1 = new(1, 1, "Station1");
    private static readonly Station Station2 = new(2, 2, "Station2");

    private static readonly List<Connexion> Connexions5F =
    [
        new Connexion(5, "FORWARD", Station0, 5, 5.0),
        new Connexion(5, "FORWARD", Station1, 5, 5.0),
        new Connexion(5, "FORWARD", Station2, 5, 5.0)
    ];

    private static readonly List<ConnexionDb> ConnexionDbs5F =
    [
        ConvertConnexionToDb(Connexions5F[0], 2),
        ConvertConnexionToDb(Connexions5F[1], 1),
        ConvertConnexionToDb(Connexions5F[2], 0)
    ];

    private static readonly Itinerary Itinerary5F = new(5, "FORWARD", Connexions5F);

    private static readonly List<Connexion> Connexions4F =
    [
        new Connexion(4, "FORWARD", Station0, 5, 5.0),
        new Connexion(4, "FORWARD", Station1, 5, 5.0),
        new Connexion(4, "FORWARD", Station2, 5, 5.0)
    ];

    private static readonly List<ConnexionDb> ConnexionDbs4F =
    [
        ConvertConnexionToDb(Connexions4F[0], 2),
        ConvertConnexionToDb(Connexions4F[1], 1),
        ConvertConnexionToDb(Connexions4F[2], 0)
    ];

    private static readonly Itinerary Itinerary4F = new(4, "FORWARD", Connexions4F);
    
    private static ConnexionDb ConvertConnexionToDb(Connexion connexion, int timeToReduct) =>
        new()
        {
            lineNumber = connexion.lineNumber,
            orientation = connexion.orientation.ToString(),
            currentStation = connexion.stationCurrent.NameStation,
            timeToNextStation = connexion.timeToNextStation,
            distanceToNextStation = connexion.distanceToNextStation,
            Timestamp = DateTime.Now.Subtract(new TimeSpan(0, timeToReduct, 0))
        };

    [Fact]
    [Trait("Category", "Unit")]
    public async Task TestAddItinerary()
    {
        Mock<IGlobalInfluxDb> mock = new();
        mock.Setup(globalInfluxDb => globalInfluxDb.SaveAll(Connexions5F))
            .ReturnsAsync(Connexions5F);

        ItineraryRepository itineraryRepository = new(mock.Object);
        Itinerary itineraryActual = await itineraryRepository.AddItinerary(Itinerary5F);
        Assert.Equal(Itinerary5F, itineraryActual);

        mock.Setup(globalInfluxDb => globalInfluxDb.SaveAll(Connexions4F))
            .ReturnsAsync(Connexions4F);

        itineraryActual = await itineraryRepository.AddItinerary(Itinerary4F);
        Assert.Equal(Itinerary4F, itineraryActual);
    }

    private static string FillQueryFind(int lineNumber, string orientation) =>
        $"|> filter(fn: (r) => r.lineNumber == \"{lineNumber}\" and r.orientation == \"{orientation}\")\n" +
        "|> sort(columns: [\"_time\"], desc: true)";

    private static void FillMockStation(Mock<IGlobalInfluxDb> mock, List<(Station station, int number)> list)
    {
        Parallel.ForEach(list, tuple =>
        {
            StationDb stationDb = new() { NameStation = tuple.station.NameStation,
                Longitude = tuple.station.Position.Longitude, Latitude = tuple.station.Position.Latitude };

            var mockSetup = mock.Setup(globalInfluxDb => globalInfluxDb.Get<StationDb>("station",
                $"|> filter(fn: (r) => r.nameStation == \"{tuple.station.NameStation}\")"));

            Parallel.For(0, tuple.number, _ =>
                mockSetup.ReturnsAsync([stationDb])
            );
        });
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task TestFindItinerary()
    {
        Mock<IGlobalInfluxDb> mock = new();
        FillMockStation(mock, [(Station0, 1), (Station1, 1), (Station2, 1)]);
        
        mock.Setup(globalInfluxDb => globalInfluxDb.Get<ConnexionDb>(MeasurementItinerary,
                FillQueryFind(5, "FORWARD")))
            .ReturnsAsync(ConnexionDbs5F);
        ItineraryRepository itineraryRepository = new(mock.Object);

        Itinerary? itineraryActual = await itineraryRepository.FindItinerary(5, "FORWARD");
        Assert.Equal(Itinerary5F, itineraryActual);

        mock.Setup(globalInfluxDb => globalInfluxDb.Get<ConnexionDb>(MeasurementItinerary,
                FillQueryFind(4, "FORWARD")))
            .ReturnsAsync([]);

        itineraryActual = await itineraryRepository.FindItinerary(4, "FORWARD");
        Assert.Null(itineraryActual);
    }

    private static string FillQueryGetBetweenConnexion(int lineNumber, string orientation, string station1,
        string station2) =>
        $"|> filter(fn: (r) => r.lineNumber == \"{lineNumber}\" and r.orientation == \"{orientation}\")\n" +
        $"|> filter(fn: (r) => r.currentStation == \"{station1}\" or r.currentStation == \"{station2}\")\n" +
        $"|> sort(columns: [\"_time\"], desc: true)";

    private static string FillQueryGetListConnexion(int lineNumber, string orientation, DateTime timeStation1,
        DateTime timeStation2) =>
        "from(bucket: \"mybucket\")\n" +
        $"|> range(start: {timeStation1:yyyy-MM-ddTHH:mm:ssZ}, stop: {timeStation2:yyyy-MM-ddTHH:mm:ssZ})\n" +
        $"|> filter(fn: (r) => r.lineNumber == \"{lineNumber}\" and r.orientation == \"{orientation}\")\n" +
        "|> sort(columns: [\"_time\"], desc: true)";


    [Fact]
    [Trait("Category", "Unit")]
    public async Task TestGetItineraryBetweenStation()
    {
        Mock<IGlobalInfluxDb> mock = new();
        FillMockStation(mock, [(Station0, 3), (Station1, 3), (Station2,31)]);
        
        mock.SetupSequence(globalInfluxDb => globalInfluxDb.Get<ConnexionDb>(MeasurementItinerary,
                FillQueryGetBetweenConnexion(5, "FORWARD", "Station0", "Station2")))
            .ReturnsAsync([ConnexionDbs5F[0], ConnexionDbs5F[1]])
            .ReturnsAsync([]);

        mock.Setup(globalInfluxDb => globalInfluxDb.Get<ConnexionDb>(FillQueryGetListConnexion(5, "FORWARD",
                ConnexionDbs5F[0].Timestamp, ConnexionDbs5F[1].Timestamp.AddMinutes(1))))
            .ReturnsAsync(ConnexionDbs5F);

        ItineraryRepository itineraryRepository = new(mock.Object);
        Itinerary? itineraryActual =
            await itineraryRepository.FindItineraryBetweenStation(5, "FORWARD", "Station0", "Station2");
        Assert.Equal(Itinerary5F, itineraryActual);
        
        itineraryActual = await itineraryRepository.FindItineraryBetweenStation(5, "FORWARD", "Station0", "Station2");
        Assert.Null(itineraryActual);
    }

    private static string FillQueryGetIntoDelete(int lineNumber, string orientation) =>
        $"|> filter(fn: (r) => r.lineNumber == \"{lineNumber}\" and r.orientation == \"{orientation}\")";

    private static string FillQueryDelete(int lineNumber, string orientation) =>
        $"_measurement=\"itinerary\" and lineNumber=\"{lineNumber}\" and orientation=\"{orientation}\"";

    [Fact]
    [Trait("Category", "Unit")]
    public async Task TestDeleteItinerary()
    {
        Mock<IGlobalInfluxDb> mock = new();
        mock.Setup(globalInfluxDb => globalInfluxDb.Delete(FillQueryDelete(5, "FORWARD")));
        mock.Setup(globalInfluxDb => globalInfluxDb.Get<ConnexionDb>(MeasurementItinerary,
                FillQueryGetIntoDelete(5, "FORWARD")))
            .ReturnsAsync([]);

        ItineraryRepository itineraryRepository = new(mock.Object);
        bool result = await itineraryRepository.DeleteItinerary(5, "FORWARD");
        Assert.True(result);

        mock.Setup(globalInfluxDb => globalInfluxDb.Delete(FillQueryDelete(4, "FORWARD")));
        mock.Setup(globalInfluxDb => globalInfluxDb.Get<ConnexionDb>(MeasurementItinerary,
                FillQueryGetIntoDelete(4, "FORWARD")))
            .ReturnsAsync(ConnexionDbs5F);

        result = await itineraryRepository.DeleteItinerary(4, "FORWARD");
        Assert.False(result);
    }
    
    private static string GetQueryFindNextStation(int lineNumber, string orientation, string station) =>
        $"|> filter(fn: (r) => r.lineNumber == \"{lineNumber}\" and r.orientation == \"{orientation}\")\n" +
        $"|> filter(fn: (r) => r.currentStation == \"{station}\")";
    
    private static ConnexionWithTime ConvertConnexionWithTime(Connexion connexion, string stationNext) =>
        new(connexion.lineNumber, connexion.orientation.ToString(), connexion.stationCurrent.NameStation,
            stationNext, connexion.timeToNextStation, connexion.distanceToNextStation);
    
    [Fact]
    [Trait("Category", "Unit")]
    public async Task TestFindNextStation()
    {
        Mock<IGlobalInfluxDb> mock = new();
        
        mock.SetupSequence(globalInfluxDb => globalInfluxDb.Get<ConnexionDb>(MeasurementItinerary, 
                GetQueryFindNextStation(5, "FORWARD", "Station0")))
            .ReturnsAsync([ConnexionDbs5F[0]])
            .ReturnsAsync([]);

        mock.Setup(globalInfluxDb => globalInfluxDb.Get<ConnexionDb>(FillQueryGetListConnexionTime(5, "FORWARD",
                ConnexionDbs5F[0].Timestamp, ConnexionDbs5F[0].Timestamp.AddMinutes(2))))
            .ReturnsAsync(ConnexionDbs5F);

        ItineraryRepository itineraryRepository = new(mock.Object);
        ConnexionWithTime? connexionActual = await itineraryRepository.FindNextStation(5, "FORWARD", "Station0");
        Assert.Equal(ConvertConnexionWithTime(Connexions5F[0], "Station1"), connexionActual);
        
        connexionActual = await itineraryRepository.FindNextStation(5, "FORWARD", "Station0");
        Assert.Null(connexionActual);
        
        mock.Setup(globalInfluxDb => globalInfluxDb.Get<ConnexionDb>(MeasurementItinerary, 
                GetQueryFindNextStation(4, "FORWARD", "Station1")))
            .ReturnsAsync([ConnexionDbs4F[1]]);
        
        mock.Setup(globalInfluxDb => globalInfluxDb.Get<ConnexionDb>(FillQueryGetListConnexionTime(4, "FORWARD",
                ConnexionDbs4F[1].Timestamp, ConnexionDbs4F[1].Timestamp.AddMinutes(2))))
            .ReturnsAsync(ConnexionDbs4F[1..]);
        
        connexionActual = await itineraryRepository.FindNextStation(4, "FORWARD", "Station1");
        Assert.Equal(ConvertConnexionWithTime(Connexions4F[1], "Station2"), connexionActual);
    }
    
    private static string FillQueryFindConnexion(string stationName) =>
        $"|> filter(fn: (r) => r.currentStation == \"{stationName}\")";
    
    private static string FillQueryGetListConnexionTime(int lineNumber, string orientation, DateTime timeStation1, DateTime timeStation2) =>
        "from(bucket: \"mybucket\")\n" +
        $"|> range(start: {timeStation1:yyyy-MM-ddTHH:mm:ssZ}, stop: {timeStation2:yyyy-MM-ddTHH:mm:ssZ})\n" +
        "|> filter(fn: (r) => r._measurement == \"itinerary\")\n" +
        $"|> filter(fn: (r) => r.lineNumber == \"{lineNumber}\" and r.orientation == \"{orientation}\")\n" +
        "|> sort(columns: [\"_time\"], desc: true)";

    [Fact]
    [Trait("Category", "Unit")]
    public async Task TestFindConnexion()
    {
        Mock<IGlobalInfluxDb> mock = new();
        mock.SetupSequence(globalInfluxDb => globalInfluxDb.Get<ConnexionDb>(MeasurementItinerary, 
                FillQueryFindConnexion("Station0")))
            .ReturnsAsync([ConnexionDbs5F[0]])
            .ReturnsAsync([]);

        mock.Setup(globalInfluxDb => globalInfluxDb.Get<ConnexionDb>(FillQueryGetListConnexionTime(5, "FORWARD",
            ConnexionDbs5F[0].Timestamp, ConnexionDbs5F[0].Timestamp.AddMinutes(2))))
            .ReturnsAsync([ConnexionDbs5F[0], ConnexionDbs5F[1]]);
        
        ItineraryRepository itineraryRepository = new(mock.Object);
        List<ConnexionWithTime> connexionsActual = await itineraryRepository.FindConnexion("Station0");
        Assert.Equal([ConvertConnexionWithTime(Connexions5F[0], "Station1")], connexionsActual);
        
        mock.Setup(globalInfluxDb => globalInfluxDb.Get<ConnexionDb>(MeasurementItinerary, 
                FillQueryFindConnexion("Station1")))
            .ReturnsAsync([]);
        
        connexionsActual = await itineraryRepository.FindConnexion("Station1");
        Assert.Empty(connexionsActual);
    }
}