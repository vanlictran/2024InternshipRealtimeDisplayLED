using api_csharp_uplink.Composant;
using api_csharp_uplink.Connectors;
using api_csharp_uplink.DirException;
using api_csharp_uplink.Entities;
using test_api_csharp_uplink.Unitaire.DBTest;

namespace test_api_csharp_uplink.Unitaire.Composant;

public class GraphComposantTest
{
    private static List<Connexion>? _connexionsForward;
    private static List<Connexion>? _connexionsBackward;
    private readonly GraphComposant _graphItinerary = new(new GraphHopperTest(true));

    public GraphComposantTest()
    {
        List<Station> stations = GenerateConnexion.AddStations();

        var (stationTimeDistanceForward, stationTimeDistanceBackward) = GenerateConnexion.GetStationTimeDistance(stations);

        _connexionsForward = GenerateConnexion.AddConnexions(stationTimeDistanceForward, "FORWARD");
        _connexionsBackward = GenerateConnexion.AddConnexions(stationTimeDistanceBackward, "BACKWARD");
    }
    
    [Fact]
    [Trait("Category", "Unit")]
    public void GetIndexesStation()
    {
        if (_connexionsForward == null)
            Assert.False(true);
        
        LinkedList<Connexion> connexions = new LinkedList<Connexion>(_connexionsForward);
        int[] indexes = GraphComposant.GetIndexesStation(connexions, "Station1", "StationF2");
        Assert.Equal([0, 1], indexes);
        
        indexes = GraphComposant.GetIndexesStation(connexions, "Station1", "Station5");
        Assert.Equal([0, 4], indexes);
        
        indexes = GraphComposant.GetIndexesStation(connexions, "Station5", "Station1");
        Assert.Equal([4, 0], indexes);
        
        indexes = GraphComposant.GetIndexesStation(connexions, "Station1", "StationB2");
        Assert.Equal([0, -1], indexes);
        
        indexes = GraphComposant.GetIndexesStation(connexions, "StationB2", "Station1");
        Assert.Equal([-1, 0], indexes);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void GetTimeItinerary()
    {
        if (_connexionsForward == null)
            Assert.False(true);
        
        LinkedList<Connexion> connexions = new LinkedList<Connexion>(_connexionsForward);
        int time = GraphComposant.GetTimeBetweenStations(connexions, 0, 1);
        Assert.Equal(5, time);
        
        time = GraphComposant.GetTimeBetweenStations(connexions, 0, 4);
        Assert.Equal(20, time);
        
        time = GraphComposant.GetTimeBetweenStations(connexions, 4, 0);
        Assert.Equal(0, time);
        
        time = GraphComposant.GetTimeBetweenStations(connexions, 0, 6);
        Assert.Equal(-1, time);
        
        time = GraphComposant.GetTimeBetweenStations(connexions, 0, -2);
        Assert.Equal(0, time);
        
        time = GraphComposant.GetTimeBetweenStations(null, 0, 1);
        Assert.Equal(-1, time);
        
        time = GraphComposant.GetTimeBetweenStations(connexions, 4, 4);
        Assert.Equal(0, time);
    }
    
    [Fact]
    [Trait("Category", "Unit")]
    public async Task AddItineraryCard()
    {
        if (_connexionsForward == null || _connexionsBackward == null)
            Assert.False(true);
        
        Itinerary itineraryForward = new Itinerary(1, "FORWARD", _connexionsForward);
        Itinerary itineraryBackward = new Itinerary(1, "BACKWARD", _connexionsBackward);
        await _graphItinerary.RegisterItineraryCard(itineraryForward);
        await _graphItinerary.RegisterItineraryCard(itineraryBackward);
        
        int timeBetweenStation = await _graphItinerary.GetItineraryTime(1, "Station1", "Station5");
        Assert.Equal(20, timeBetweenStation);
        
        timeBetweenStation = await _graphItinerary.GetItineraryTime(1, "Station5", "Station1");
        Assert.Equal(20, timeBetweenStation);
        
        timeBetweenStation = await _graphItinerary.GetItineraryTime(1, "StationF2", "StationF4");
        Assert.Equal(10, timeBetweenStation);
        
        timeBetweenStation = await _graphItinerary.GetItineraryTime(1, "StationF4", "StationF2");
        Assert.Equal(10, timeBetweenStation);
        
        timeBetweenStation = await _graphItinerary.GetItineraryTime(1, "StationF2", "StationB2");
        Assert.Equal(-1, timeBetweenStation);
        
        timeBetweenStation = await _graphItinerary.GetItineraryTime(1, "Station1", "StationB2");
        Assert.Equal(5, timeBetweenStation);
        
        timeBetweenStation = await _graphItinerary.GetItineraryTime(1, "StationB2", "Station1");
        Assert.Equal(5, timeBetweenStation);
        
        timeBetweenStation = await _graphItinerary.GetItineraryTime(1, "Station1", "Station1");
        Assert.Equal(-1, timeBetweenStation);
        
        await Assert.ThrowsAsync<NotFoundException>(() => _graphItinerary.GetItineraryTime(2, "Station1", "Station5"));
    }
    
    [Fact]
    [Trait("Category", "Unit")]
    public async Task DeleteItinerary()
    {
        if (_connexionsForward == null || _connexionsBackward == null)
            Assert.False(true);
        
        Itinerary itineraryForward = new Itinerary(1, "FORWARD", _connexionsForward);
        Itinerary itineraryBackward = new Itinerary(1, "BACKWARD", _connexionsBackward);
        await _graphItinerary.RegisterItineraryCard(itineraryForward);
        await _graphItinerary.RegisterItineraryCard(itineraryBackward);
        
        await _graphItinerary.RemoveItineraryCard(1, Orientation.FORWARD);
        int time = await _graphItinerary.GetItineraryTime(1, "Station1", "Station5");
        Assert.Equal(20, time); // The itinerary is still present in the backward orientation
        
        await _graphItinerary.RemoveItineraryCard(1, Orientation.BACKWARD);
        
        await Assert.ThrowsAsync<NotFoundException>(() => _graphItinerary.GetItineraryTime(1, "Station1", "Station5"));
        await Assert.ThrowsAsync<NotFoundException>(() => _graphItinerary.RemoveItineraryCard(1, Orientation.FORWARD));
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void GetMoreClosestStationTest()
    {
        if (_connexionsForward == null)
            Assert.False(true);

        Parallel.For(0, _connexionsForward.Count - 1, i =>
        {
            double distance = GraphHelperService.DistanceHaversine(_connexionsForward[i].stationCurrent.Position,
                _connexionsForward[i + 1].stationCurrent.Position);
            _connexionsForward[i].distanceToNextStation = distance;
        });
        
        LinkedList<Connexion> linkedList = new LinkedList<Connexion>(_connexionsForward);
        Position positionCard = new Position(-1, -1);
        (Station? station1, Station? station2) = _graphItinerary.GetMoreClosestStation(positionCard, linkedList);
        Assert.Equal("Station1", station2?.NameStation);
        Assert.Null(station1);
        
        positionCard = new Position(1.1, 2.2);
        (station1, station2) = _graphItinerary.GetMoreClosestStation(positionCard, linkedList);
        Assert.Equal("Station1", station1?.NameStation);
        Assert.Equal("StationF2", station2?.NameStation);
        
        positionCard = new Position(5.1, 10.2);
        (station1, station2) = _graphItinerary.GetMoreClosestStation(positionCard, linkedList);
        Assert.Equal("Station5", station1?.NameStation);
        Assert.Null(station2);
        
        positionCard = new Position(4.8, 9.5);
        (station1, station2) = _graphItinerary.GetMoreClosestStation(positionCard, linkedList);
        Assert.Equal("StationF4", station1?.NameStation);
        Assert.Equal("Station5", station2?.NameStation);
        
        positionCard = new Position(2.9, 5.9);
        (station1, station2) = _graphItinerary.GetMoreClosestStation(positionCard, linkedList);
        Assert.Equal("StationF2", station1?.NameStation);
        Assert.Equal("StationF3", station2?.NameStation);
        
        positionCard = new Position(3.1, 6.2);
        (station1, station2) = _graphItinerary.GetMoreClosestStation(positionCard, linkedList);
        Assert.Equal("StationF3", station1?.NameStation);
        Assert.Equal("StationF4", station2?.NameStation);
        
        positionCard = new Position(4.1, 8.1);
        (station1, station2) = _graphItinerary.GetMoreClosestStation(positionCard, linkedList);
        Assert.Equal("StationF4", station1?.NameStation);
        Assert.Equal("Station5", station2?.NameStation);
        
        Assert.NotNull(linkedList.First);
        positionCard = new Position(0.0, 15.0);
        (station1, station2) = _graphItinerary.GetMoreClosestStation(positionCard, 
            new LinkedList<Connexion>([linkedList.First.Value]));
        Assert.Null(station1);
        Assert.Equal("Station1", station2?.NameStation);
        
        Assert.Throws<NotFoundException>(() => _graphItinerary.GetMoreClosestStation(positionCard, null));
    }
    
    [Fact]
    [Trait("Category", "Unit")]
    public async Task GetTimeToNextOneStationTest()
    {
        int time = await _graphItinerary.GetTimeStation("Station1");
        Assert.Equal(-1, time);
        
        Station stationExpect = new Station(new Position(1.0, 2.0), "Station1");
        Station stationActual = await _graphItinerary.AddStationGraph(stationExpect);
        Assert.Equal(stationExpect, stationActual);
        
        time = await _graphItinerary.GetTimeStation("Station1");
        Assert.Equal(-1, time); // Car pas de position enregistrer
        
        await _graphItinerary.RegisterPositionOneStation(new Position(1.1, 2.1));
        time = await _graphItinerary.GetTimeStation("Station1");
        Assert.Equal(5, time);
    }
}