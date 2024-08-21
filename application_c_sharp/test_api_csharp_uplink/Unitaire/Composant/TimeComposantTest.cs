using api_csharp_uplink.Composant;
using api_csharp_uplink.DirException;
using api_csharp_uplink.Dto;
using api_csharp_uplink.Entities;
using api_csharp_uplink.Interface;
using api_csharp_uplink.Repository.Interface;
using test_api_csharp_uplink.Unitaire.DBTest;

namespace test_api_csharp_uplink.Unitaire.Composant;

public class TimeComposantTest : IAsyncLifetime
{
    private TimeComposant? _timeProcessor;
    private IItineraryRegister? _itineraryRegister;

    public async Task InitializeAsync()
    {
        IStationRepository stationRepository = new DbTestStation();
        StationComposant stationComposant = new StationComposant(stationRepository);
        
        List<Station> stations = GenerateConnexion.AddStations();

        await Parallel.ForEachAsync(stations, async (station, _) =>
        {
            await stationComposant.AddStation(station.Position.Latitude, station.Position.Longitude,
                station.NameStation);
        });
        
        var (stationTimeDistanceForward, stationTimeDistanceBackward) = GenerateConnexion.GetStationTimeDistance(stations);

        List<Connexion> connexionsForward = GenerateConnexion.AddConnexions(stationTimeDistanceForward, "FORWARD");
        List<Connexion> connexionsBackward = GenerateConnexion.AddConnexions(stationTimeDistanceBackward, "BACKWARD");
        
        List<ConnexionDto> connexionDtosForward = GenerateConnexion.ConvertConnexionToDto(connexionsForward);
        List<ConnexionDto> connexionDtosBackward = GenerateConnexion.ConvertConnexionToDto(connexionsBackward);
        
        IItineraryRepository itineraryRepository = new DbTestItinerary();
        IGraphHelper graphHopperTest = new GraphHopperTest();
        GraphComposant graphComposant = new GraphComposant(graphHopperTest);
        _itineraryRegister = new ItineraryComposant(itineraryRepository, stationComposant, graphHopperTest, graphComposant);
        CardComposant cardComposant = new CardComposant(new DbTestCard());
        _timeProcessor = new TimeComposant(graphComposant, graphComposant, cardComposant);
        
        await _itineraryRegister.AddItinerary(1, "FORWARD", connexionDtosForward);
        await _itineraryRegister.AddItinerary(1, "BACKWARD", connexionDtosBackward);
    }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }
    
    [Fact]
    [Trait("Category", "ToDevelop")]
    public async Task GetTimeItinerary()
    {
        if (_timeProcessor == null) 
            Assert.False(true);
        
        int time = await _timeProcessor.GetTimeBetweenStations(1, "Station1", "Station5");
        Assert.Equal(20, time);
        
        time = await _timeProcessor.GetTimeBetweenStations(1, "Station5", "Station1");
        Assert.Equal(20, time);
        
        time = await _timeProcessor.GetTimeBetweenStations(1, "StationF2", "StationF4");
        Assert.Equal(10, time);
                
        time = await _timeProcessor.GetTimeBetweenStations(1, "StationF4", "StationF2");
        Assert.Equal(10, time);
        
        time = await _timeProcessor.GetTimeBetweenStations(1, "Station1", "StationB2");
        Assert.Equal(5, time);
        
        time = await _timeProcessor.GetTimeBetweenStations(1, "StationB2", "Station1");
        Assert.Equal(5, time);
        
        time = await _timeProcessor.GetTimeBetweenStations(1, "Station1", "Station1");
        Assert.Equal(0, time);
    }

    [Fact]
    [Trait("Category", "ToDevelop")]
    public async Task GetTimeItineraryError()
    {
        if (_timeProcessor == null) 
            Assert.False(true);
        
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _timeProcessor.GetTimeBetweenStations(1, "StationF2", "StationB2"));
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _timeProcessor.GetTimeBetweenStations(2, "Station1", "Station5"));
        await Assert.ThrowsAsync<ArgumentNullException>(() => _timeProcessor.GetTimeBetweenStations(1, "", "Station5"));
        await Assert.ThrowsAsync<ArgumentNullException>(() => _timeProcessor.GetTimeBetweenStations(1, "Station1", ""));
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
            _timeProcessor.GetTimeBetweenStations(-1, "Station1", "Station5"));
    }
    
    [Fact]
    [Trait("Category", "Unit")]
    public async Task GetTimeStation()
    {
        if (_timeProcessor == null || _itineraryRegister == null)
            Assert.False(true);

        await Assert.ThrowsAsync<NotFoundException>(() => _timeProcessor.GetTimeToNextStation("Station1"));
        
        await _itineraryRegister.AddOneStation("Station1");
        await Assert.ThrowsAsync<NotFoundException>(() => _timeProcessor.GetTimeToNextStation("Station1"));

        await _timeProcessor.RegisterPositionOneStation(new Position(1.1, 2.1));
        int timeToNextStation = await _timeProcessor.GetTimeToNextStation("Station1");
        Assert.Equal(5, timeToNextStation);
        
        await Assert.ThrowsAsync<NotFoundException>(() => _timeProcessor.GetTimeToNextStation("Station2"));

        await Assert.ThrowsAsync<ArgumentNullException>(() => _timeProcessor.GetTimeToNextStation(""));
    }
    
}