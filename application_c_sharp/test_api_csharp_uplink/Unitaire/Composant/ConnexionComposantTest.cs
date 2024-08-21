using api_csharp_uplink.Composant;
using api_csharp_uplink.DirException;
using api_csharp_uplink.Dto;
using api_csharp_uplink.Entities;
using api_csharp_uplink.Interface;
using test_api_csharp_uplink.Unitaire.DBTest;

namespace test_api_csharp_uplink.Unitaire.Composant;

public class ConnexionComposantTest
{
    private readonly IItineraryRegister _itineraryRegister;
    private readonly ConnexionComposant _connexionComposant;

    private readonly List<ConnexionDto> _connexions =
    [
        new ConnexionDto { CurrentNameStation = "Station3", NextNameStation = "Station4" },
        new ConnexionDto { CurrentNameStation = "Station2", NextNameStation = "Station3" },
        new ConnexionDto { CurrentNameStation = "Station0", NextNameStation = "Station1" },
        new ConnexionDto { CurrentNameStation = "Station1", NextNameStation = "Station2" },
        new ConnexionDto { CurrentNameStation = "Station4", NextNameStation = "Station3" }
    ];

    public ConnexionComposantTest()
    {
        IStationRepository stationRepository = new DbTestStation();
        StationComposant stationComposant = new StationComposant(stationRepository);

        for (int i = 0; i < 5; i++)
            _ = stationComposant.AddStation(i * 5.0, i * 4.0, $"Station{i}").Result;

        DbTestItinerary itineraryRepository = new DbTestItinerary();
        IGraphHelper graphHelper = new GraphHopperTest();
        IGraphItinerary graphItinerary = new GraphComposant(graphHelper);
        _itineraryRegister = new ItineraryComposant(itineraryRepository, stationComposant, graphHelper, graphItinerary);
        _connexionComposant = new ConnexionComposant(itineraryRepository);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task FindConnexionTest()
    {
        await _itineraryRegister.AddItinerary(5, "FORWARD", _connexions);

        ConnexionWithTime connexionFind = await _connexionComposant.FindConnexion(5, "FORWARD", "Station1");
        ConnexionWithTime connexionExpected =
            new ConnexionWithTime(5, "FORWARD", "Station1", "Station2", 5, 4.0);
        Assert.Equal(connexionExpected, connexionFind);

        connexionExpected =
            new ConnexionWithTime(5, "FORWARD", "Station3", "Station4", 5, 4.0);
        connexionFind = await _connexionComposant.FindConnexion(5, "FORWARD", "Station3");
        Assert.Equal(connexionExpected, connexionFind);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            _connexionComposant.FindConnexion(5, "FORWARD", "Station4"));
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _connexionComposant.FindConnexion(4, "FORWARD", "Station1"));
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
            _connexionComposant.FindConnexion(-1, "FORWARD", "Station1"));
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _connexionComposant.FindConnexion(5, "FORWAR", "Station1"));
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            _connexionComposant.FindConnexion(5, "FORWARD", ""));
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task FindNextConnexionTest()
    {
        await _itineraryRegister.AddItinerary(5, "FORWARD", _connexions);
        ConnexionWithTime connexionExpected5F =
            new ConnexionWithTime(5, "FORWARD", "Station1", "Station2", 5, 5.0);

        List<ConnexionWithTime> connexionsFind = await _connexionComposant.FindNextConnexion("Station1");
        Assert.Equal([connexionExpected5F], connexionsFind);

        await _itineraryRegister.AddItinerary(5, "BACKWARD", _connexions);
        await _itineraryRegister.AddItinerary(4, "FORWARD", _connexions);
        ConnexionWithTime connexionExpected5B =
            new ConnexionWithTime(5, "BACKWARD", "Station1", "Station2", 5, 5.0);
        ConnexionWithTime connexionExpected4F =
            new ConnexionWithTime(4, "FORWARD", "Station1", "Station2", 5, 5.0);

        connexionsFind = await _connexionComposant.FindNextConnexion("Station1");
        Assert.Equal([connexionExpected5F, connexionExpected5B, connexionExpected4F],
            connexionsFind);

        connexionsFind = await _connexionComposant.FindNextConnexion("Station5");
        Assert.Empty(connexionsFind);

        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            _connexionComposant.FindNextConnexion(""));
    }
}