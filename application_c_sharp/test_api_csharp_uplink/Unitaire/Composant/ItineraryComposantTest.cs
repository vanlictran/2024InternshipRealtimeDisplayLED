using api_csharp_uplink.Composant;
using api_csharp_uplink.DirException;
using api_csharp_uplink.Dto;
using api_csharp_uplink.Entities;
using api_csharp_uplink.Interface;
using api_csharp_uplink.Repository.Interface;
using test_api_csharp_uplink.Unitaire.DBTest;

namespace test_api_csharp_uplink.Unitaire.Composant;

public class ItineraryComposantTest
{
    private readonly ItineraryComposant _itineraryComposant;
    private readonly List<ConnexionDto> _connexions =
    [
        new ConnexionDto { CurrentNameStation = "Station3", NextNameStation = "Station4" },
        new ConnexionDto { CurrentNameStation = "Station2", NextNameStation = "Station3" },
        new ConnexionDto { CurrentNameStation = "Station0", NextNameStation = "Station1" },
        new ConnexionDto { CurrentNameStation = "Station1", NextNameStation = "Station2" },
        new ConnexionDto { CurrentNameStation = "Station4", NextNameStation = "Station3" }
    ]; 

    public ItineraryComposantTest()
    {
        IStationRepository stationRepository = new DbTestStation();
        StationComposant stationComposant = new StationComposant(stationRepository);

        for (int i = 0; i < 5; i++)
            _ = stationComposant.AddStation(i * 5.0, i * 4.0, $"Station{i}").Result;

        IItineraryRepository itineraryRepository = new DbTestItinerary();
        IGraphHelper graphHelper = new GraphHopperTest();        
        IGraphItinerary graphItinerary = new GraphComposant(graphHelper);
        _itineraryComposant = new ItineraryComposant(itineraryRepository, stationComposant, graphHelper, graphItinerary);
    }
    
    [Fact]
    [Trait("Category", "Unit")]
    public async Task AddItineraryTest()
    {
        Itinerary itineraryAdd = await _itineraryComposant.AddItinerary(5, "FORWARD", _connexions);

        Assert.Equal(5, itineraryAdd.lineNumber);
        Assert.Equal(Orientation.FORWARD, itineraryAdd.orientation);
        Assert.Equal(5, itineraryAdd.connexions.Count);

        var add = itineraryAdd;
        Parallel.For(0, itineraryAdd.connexions.Count, i =>
                Assert.Equal("Station" + i, add.connexions[i].stationCurrent.NameStation
            ));

        Itinerary itineraryFind = await _itineraryComposant.FindItinerary(5, "FORWARD");
        Assert.Equal(itineraryAdd, itineraryFind);
        Assert.Equal(itineraryFind.connexions, itineraryAdd.connexions);
        
        itineraryAdd = await _itineraryComposant.AddItinerary(5, "BACKWARD", _connexions);
        Assert.Equal(5, itineraryAdd.lineNumber);
        Assert.Equal(Orientation.BACKWARD, itineraryAdd.orientation);
        Assert.NotEqual(itineraryFind, itineraryAdd);
        Assert.NotEqual(itineraryFind.connexions, itineraryAdd.connexions);
        
        Parallel.ForEach(itineraryAdd.connexions, connexion =>
            Assert.Equal(Orientation.BACKWARD, connexion.orientation)
        );
        
        itineraryFind = await _itineraryComposant.FindItinerary(5, "BACKWARD");
        Assert.Equal(itineraryAdd, itineraryFind);
        Assert.Equal(itineraryAdd.connexions, itineraryFind.connexions);

        itineraryAdd = await _itineraryComposant.AddItinerary(4, "FORWARD", _connexions);
        Assert.Equal(4, itineraryAdd.lineNumber);
        Assert.Equal(Orientation.FORWARD, itineraryAdd.orientation);
        Assert.NotEqual(itineraryFind, itineraryAdd);
        Assert.NotEqual(itineraryFind.connexions, itineraryAdd.connexions);
        
        itineraryFind = await _itineraryComposant.FindItinerary(4, "FORWARD");
        Assert.Equal(itineraryAdd, itineraryFind);
        Assert.Equal(itineraryAdd.connexions, itineraryFind.connexions);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task AddItineraryTestError()
    {
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => 
            _itineraryComposant.AddItinerary(-1, "FORWARD", _connexions));
        await Assert.ThrowsAsync<ArgumentException>(() => 
            _itineraryComposant.AddItinerary(5, "FORWAR", _connexions));
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
            _itineraryComposant.AddItinerary(5, "FORWARD", []));
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => 
            _itineraryComposant.AddItinerary(5, "FORWARD", [
                new ConnexionDto{CurrentNameStation = "Station0", NextNameStation = "Station0"}]));
        await Assert.ThrowsAsync<NotFoundException>(() => 
            _itineraryComposant.AddItinerary(5, "FORWARD",
                [_connexions[0], new ConnexionDto{CurrentNameStation = "Station6", NextNameStation = "Station7"}]));

        await _itineraryComposant.AddItinerary(5, "FORWARD", _connexions);
        await Assert.ThrowsAsync<AlreadyCreateException>(() => 
            _itineraryComposant.AddItinerary(5, "FORWARD", _connexions));
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task GetItineraryTest()
    {
        await _itineraryComposant.AddItinerary(5, "FORWARD", _connexions);
        
        Itinerary itineraryFind = await _itineraryComposant.FindItinerary(5, "FORWARD");
        Assert.Equal(5, itineraryFind.lineNumber);
        Assert.Equal(Orientation.FORWARD, itineraryFind.orientation);
        Assert.Equal(5, itineraryFind.connexions.Count);
        
        Parallel.For(0, itineraryFind.connexions.Count, i =>
            Assert.Equal("Station" + i, itineraryFind.connexions[i].stationCurrent.NameStation
            ));
        Parallel.ForEach(itineraryFind.connexions, connexion => Assert.Equal(Orientation.FORWARD, connexion.orientation));
        Parallel.ForEach(itineraryFind.connexions, connexion => Assert.Equal(5, connexion.lineNumber));
        
        await Assert.ThrowsAsync<NotFoundException>(() => _itineraryComposant.FindItinerary(5, "BACKWARD"));
        await Assert.ThrowsAsync<NotFoundException>(() => _itineraryComposant.FindItinerary(4, "FORWARD"));
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => _itineraryComposant.FindItinerary(-1, "FORWARD"));
        await Assert.ThrowsAsync<ArgumentException>(() => _itineraryComposant.FindItinerary(5, "FORWAR"));
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task GetItineraryBetweenTest()
    {
        await _itineraryComposant.AddItinerary(5, "FORWARD", _connexions);
        
        Itinerary itineraryFind = await _itineraryComposant.FindItineraryBetweenStation(5, "FORWARD", "Station0", "Station4");
        Assert.Equal(5, itineraryFind.lineNumber);
        Assert.Equal(Orientation.FORWARD, itineraryFind.orientation);
        Assert.Equal(5, itineraryFind.connexions.Count);
        
        Parallel.For(0, itineraryFind.connexions.Count, i =>
            Assert.Equal("Station" + i, itineraryFind.connexions[i].stationCurrent.NameStation
            ));
        
        Itinerary itineraryFind2 = await _itineraryComposant.FindItineraryBetweenStation(5, "FORWARD", "Station1", "Station3");
        Assert.Equal(5, itineraryFind2.lineNumber);
        Assert.Equal(Orientation.FORWARD, itineraryFind2.orientation);
        Assert.Equal(3, itineraryFind2.connexions.Count);
        
        Parallel.For(1, 4, i =>
            Assert.Equal("Station" + i, itineraryFind2.connexions[i - 1].stationCurrent.NameStation
            ));
        
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => 
            _itineraryComposant.FindItineraryBetweenStation(5, "FORWARD", "Station5", "Station0"));
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _itineraryComposant.FindItineraryBetweenStation(4, "FORWARD", "Station0", "Station5"));
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _itineraryComposant.FindItineraryBetweenStation(5, "BACKWARD", "Station0", "Station5"));
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
            _itineraryComposant.FindItineraryBetweenStation(-1, "FORWARD", "Station0", "Station5"));
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _itineraryComposant.FindItineraryBetweenStation(5, "FORWAR", "Station0", "Station5"));
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            _itineraryComposant.FindItineraryBetweenStation(5, "FORWARD", "", "Station5"));
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            _itineraryComposant.FindItineraryBetweenStation(5, "FORWARD", "Station0", ""));
    }
    
    [Fact]
    [Trait("Category", "Unit")]
    public async Task DeleteItineraryTest()
    {
        await _itineraryComposant.AddItinerary(5, "FORWARD", _connexions);
        await _itineraryComposant.DeleteItinerary(5, "FORWARD");
        
        await Assert.ThrowsAsync<NotFoundException>(() => _itineraryComposant.FindItinerary(5, "FORWARD"));
        await Assert.ThrowsAsync<NotFoundException>(() => _itineraryComposant.FindItineraryBetweenStation(5, "FORWARD", "Station0", "Station4"));
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => _itineraryComposant.DeleteItinerary(-1, "FORWARD"));
        await Assert.ThrowsAsync<ArgumentException>(() => _itineraryComposant.DeleteItinerary(5, "FORWAR"));
    }
}