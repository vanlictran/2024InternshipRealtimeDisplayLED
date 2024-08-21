using api_csharp_uplink.Composant;
using api_csharp_uplink.Controllers;
using api_csharp_uplink.Dto;
using api_csharp_uplink.Interface;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using test_api_csharp_uplink.Unitaire.DBTest;

namespace test_api_csharp_uplink.Unitaire.Controllers;

public class ConnexionControllerTest
{
    private readonly ItineraryController _itineraryController;
    private readonly ConnexionController _connexionController;

    private static readonly List<ConnexionDto> Connexions =
    [
        new ConnexionDto { CurrentNameStation = "Station3", NextNameStation = "Station4" },
        new ConnexionDto { CurrentNameStation = "Station2", NextNameStation = "Station3" },
        new ConnexionDto { CurrentNameStation = "Station0", NextNameStation = "Station1" },
        new ConnexionDto { CurrentNameStation = "Station1", NextNameStation = "Station2" },
        new ConnexionDto { CurrentNameStation = "Station4", NextNameStation = "Station3" }
    ];

    private static readonly ItineraryDto ItineraryDto5F = new()
    {
        LineNumber = 5, Orientation = "FORWARD",
        Connexions = Connexions
    };

    private static ConnexionDtoWithTime ConvertToConnexionDtoWithTime(int lineNumber, string orientation,
        string currentNameStation, string nextNameStation, int timeToNextStation, double distanceToNextStation) =>
        new()
        {
            LineNumber = lineNumber, Orientation = orientation, CurrentNameStation = currentNameStation,
            NextNameStation = nextNameStation, TimeToNextStation = timeToNextStation,
            DistanceToNextStation = distanceToNextStation
        };

    private static void VerifyObjectResult<T>(ConnexionDtoWithTime connexionExpected, IActionResult result)
        where T : ObjectResult
    {
        result.Should().BeOfType<T>();
        T okObjectResult = (T)result;
        okObjectResult.Should().NotBeNull();
        okObjectResult.Value.Should().BeEquivalentTo(connexionExpected);
    }
    
    private static void VerifyObjectResult<T>(List<ConnexionDtoWithTime> connexionExpected, IActionResult result)
        where T : ObjectResult
    {
        result.Should().BeOfType<T>();
        T okObjectResult = (T)result;
        okObjectResult.Should().NotBeNull();
        okObjectResult.Value.Should().BeEquivalentTo(connexionExpected);
    }


    public ConnexionControllerTest()
    {
        IStationRepository stationRepository = new DbTestStation();
        StationComposant stationComposant = new StationComposant(stationRepository);

        for (int i = 0; i < 5; i++)
            _ = stationComposant.AddStation(i * 5.0, i * 5.0, $"Station{i}").Result;

        DbTestItinerary itineraryRepository = new DbTestItinerary();
        IGraphHelper graphHelper = new GraphHopperTest();
        IGraphItinerary graphItinerary = new GraphComposant(graphHelper);
        ItineraryComposant itineraryComposant = new ItineraryComposant(itineraryRepository, stationComposant, graphHelper, graphItinerary);
        ConnexionComposant connexionComposant = new ConnexionComposant(itineraryRepository);
        _itineraryController = new ItineraryController(itineraryComposant, itineraryComposant);
        _connexionController = new ConnexionController(connexionComposant);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task FindConnexionTest()
    {
        await _itineraryController.AddItinerary(ItineraryDto5F);

        IActionResult result = await _connexionController.FindNextStation(5, "FORWARD", "Station1");
        ConnexionDtoWithTime connexionExpected =
            ConvertToConnexionDtoWithTime(5, "FORWARD", "Station1", "Station2", 5, 5.0);
        VerifyObjectResult<OkObjectResult>(connexionExpected, result);

        result = await _connexionController.FindNextStation(5, "FORWARD", "Station3");
        connexionExpected = ConvertToConnexionDtoWithTime(5, "FORWARD", "Station3", "Station4", 5, 5.0);
        VerifyObjectResult<OkObjectResult>(connexionExpected, result);

        result = await _connexionController.FindNextStation(5, "FORWARD", "Station4");
        result.Should().BeOfType<NotFoundObjectResult>();

        result = await _connexionController.FindNextStation(4, "FORWARD", "Station1");
        result.Should().BeOfType<NotFoundObjectResult>();

        result = await _connexionController.FindNextStation(5, "FORWAR", "Station1");
        result.Should().BeOfType<BadRequestObjectResult>();

        result = await _connexionController.FindNextStation(5, "FORWARD", "");
        result.Should().BeOfType<BadRequestObjectResult>();

        result = await _connexionController.FindNextStation(-1, "FORWARD", "Station1");
        result.Should().BeOfType<BadRequestObjectResult>();
    }
    
    
    [Fact]
    [Trait("Category", "Unit")]
    public async Task FindNextConnexionTest()
    {
        await _itineraryController.AddItinerary(ItineraryDto5F);

        ConnexionDtoWithTime connexionExpected =
            ConvertToConnexionDtoWithTime(5, "FORWARD", "Station1", "Station2", 5, 5.0);
        IActionResult result = await _connexionController.FindConnexion("Station1");
        VerifyObjectResult<OkObjectResult>([connexionExpected], result);

        ItineraryDto itineraryDto5B = new()
        {
            LineNumber = 5, Orientation = "BACKWARD",
            Connexions = Connexions
        };

        ItineraryDto itineraryDto4F = new()
        {
            LineNumber = 4, Orientation = "FORWARD",
            Connexions = Connexions
        };

        ConnexionDtoWithTime connexionExpected5B =
            ConvertToConnexionDtoWithTime(5, "BACKWARD", "Station1", "Station2", 5, 5.0);
        ConnexionDtoWithTime connexionExpected4F =
            ConvertToConnexionDtoWithTime(4, "FORWARD", "Station1", "Station2", 5, 5.0);

        await _itineraryController.AddItinerary(itineraryDto5B);
        await _itineraryController.AddItinerary(itineraryDto4F);

        result = await _connexionController.FindConnexion("Station1");
        VerifyObjectResult<OkObjectResult>([connexionExpected, connexionExpected5B, connexionExpected4F], result);

        result = await _connexionController.FindConnexion("");
        result.Should().BeOfType<BadRequestObjectResult>();
    }
}