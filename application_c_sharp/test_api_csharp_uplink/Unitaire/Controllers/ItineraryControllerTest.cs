using api_csharp_uplink.Composant;
using api_csharp_uplink.Controllers;
using api_csharp_uplink.Dto;
using api_csharp_uplink.Interface;
using api_csharp_uplink.Repository.Interface;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using test_api_csharp_uplink.Unitaire.DBTest;

namespace test_api_csharp_uplink.Unitaire.Controllers;

public class ItineraryControllerTest
{
    private readonly ItineraryController _itineraryController;

    private static readonly List<ConnexionDto> Connexions =
    [
        new ConnexionDto { CurrentNameStation = "Station3", NextNameStation = "Station4" },
        new ConnexionDto { CurrentNameStation = "Station2", NextNameStation = "Station3" },
        new ConnexionDto { CurrentNameStation = "Station0", NextNameStation = "Station1" },
        new ConnexionDto { CurrentNameStation = "Station1", NextNameStation = "Station2" },
        new ConnexionDto { CurrentNameStation = "Station4", NextNameStation = "Station3" }
    ];

    private static readonly List<ConnexionDto> ConnexionsSorted =
    [
        new ConnexionDto { CurrentNameStation = "Station0", NextNameStation = "Station1" },
        new ConnexionDto { CurrentNameStation = "Station1", NextNameStation = "Station2" },
        new ConnexionDto { CurrentNameStation = "Station2", NextNameStation = "Station3" },
        new ConnexionDto { CurrentNameStation = "Station3", NextNameStation = "Station4" },
    ];

    private static readonly ItineraryDto ItineraryDto5F = new()
    {
        LineNumber = 5, Orientation = "FORWARD",
        Connexions = Connexions
    };

    public ItineraryControllerTest()
    {
        IStationRepository stationRepository = new DbTestStation();
        StationComposant stationComposant = new StationComposant(stationRepository);

        for (int i = 0; i < 5; i++)
            _ = stationComposant.AddStation(i * 5.0, i * 4.0, $"Station{i}").Result;

        IItineraryRepository itineraryRepository = new DbTestItinerary();
        IGraphHelper graphHelper = new GraphHopperTest();
        IGraphItinerary graphItinerary = new GraphComposant(graphHelper);
        ItineraryComposant itineraryComposant = new ItineraryComposant(itineraryRepository, stationComposant, graphHelper, graphItinerary);
        _itineraryController = new ItineraryController(itineraryComposant, itineraryComposant);
    }

    private static void VerifyObjectResult<T>(ItineraryDto scheduleExpected, IActionResult result)
        where T : ObjectResult
    {
        result.Should().BeOfType<T>();
        T okObjectResult = (T)result;
        okObjectResult.Should().NotBeNull();
        okObjectResult.Value.Should().BeEquivalentTo(scheduleExpected);
    }

    private static ItineraryDto
        ConvertToItineraryDto(int lineNumber, string orientation, List<ConnexionDto> connexions) =>
        new() { LineNumber = lineNumber, Orientation = orientation, Connexions = connexions };

    [Fact]
    [Trait("Category", "Unit")]
    public async Task AddItineraryTest()
    {
        ItineraryDto itineraryExpected5F = ConvertToItineraryDto(5, "FORWARD", ConnexionsSorted);
        IActionResult itineraryAdd = await _itineraryController.AddItinerary(ItineraryDto5F);
        VerifyObjectResult<CreatedResult>(itineraryExpected5F, itineraryAdd);

        IActionResult itineraryFind = await _itineraryController.FindItinerary(5, "FORWARD");
        VerifyObjectResult<OkObjectResult>(itineraryExpected5F, itineraryFind);


        ItineraryDto itineraryDto5B = ConvertToItineraryDto(5, "BACKWARD", Connexions);
        ItineraryDto itineraryDto5BExpected = ConvertToItineraryDto(5, "BACKWARD", ConnexionsSorted);
        itineraryAdd = await _itineraryController.AddItinerary(itineraryDto5B);
        VerifyObjectResult<CreatedResult>(itineraryDto5BExpected, itineraryAdd);

        itineraryFind = await _itineraryController.FindItinerary(5, "BACKWARD");
        VerifyObjectResult<OkObjectResult>(itineraryDto5BExpected, itineraryFind);

        ItineraryDto itineraryDto4F = ConvertToItineraryDto(4, "FORWARD", Connexions);
        ItineraryDto itineraryDto4FExpected = ConvertToItineraryDto(4, "FORWARD", ConnexionsSorted);
        itineraryAdd = await _itineraryController.AddItinerary(itineraryDto4F);
        VerifyObjectResult<CreatedResult>(itineraryDto4FExpected, itineraryAdd);

        itineraryFind = await _itineraryController.FindItinerary(4, "FORWARD");
        VerifyObjectResult<OkObjectResult>(itineraryDto4FExpected, itineraryFind);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task AddItineraryTestError()
    {
        IActionResult itineraryAdd = await _itineraryController.AddItinerary(
            ConvertToItineraryDto(-1, "FORWARD", Connexions));
        itineraryAdd.Should().BeOfType<BadRequestObjectResult>();

        itineraryAdd = await _itineraryController.AddItinerary(
            ConvertToItineraryDto(5, "FORWAR", Connexions));
        itineraryAdd.Should().BeOfType<BadRequestObjectResult>();

        itineraryAdd = await _itineraryController.AddItinerary(
            ConvertToItineraryDto(5, "FORWARD", []));
        itineraryAdd.Should().BeOfType<BadRequestObjectResult>();

        itineraryAdd = await _itineraryController.AddItinerary(
            ConvertToItineraryDto(5, "FORWARD",
                [new ConnexionDto { CurrentNameStation = "Station0", NextNameStation = "Station0" }]));
        itineraryAdd.Should().BeOfType<BadRequestObjectResult>();

        itineraryAdd = await _itineraryController.AddItinerary(
            ConvertToItineraryDto(5, "FORWARD",
                [Connexions[0], new ConnexionDto { CurrentNameStation = "Station6", NextNameStation = "Station7" }]));
        itineraryAdd.Should().BeOfType<NotFoundObjectResult>();

        await _itineraryController.AddItinerary(ItineraryDto5F);
        itineraryAdd = await _itineraryController.AddItinerary(ItineraryDto5F);
        itineraryAdd.Should().BeOfType<ConflictObjectResult>();
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task GetItineraryTest()
    {
        await _itineraryController.AddItinerary(ItineraryDto5F);
        ItineraryDto itineraryExpected5F = ConvertToItineraryDto(5, "FORWARD", ConnexionsSorted);

        IActionResult itineraryFind = await _itineraryController.FindItinerary(5, "FORWARD");
        VerifyObjectResult<OkObjectResult>(itineraryExpected5F, itineraryFind);

        IActionResult itineraryFindError = await _itineraryController.FindItinerary(5, "BACKWARD");
        itineraryFindError.Should().BeOfType<NotFoundObjectResult>();

        itineraryFindError = await _itineraryController.FindItinerary(4, "FORWARD");
        itineraryFindError.Should().BeOfType<NotFoundObjectResult>();

        itineraryFindError = await _itineraryController.FindItinerary(-1, "FORWARD");
        itineraryFindError.Should().BeOfType<BadRequestObjectResult>();

        itineraryFindError = await _itineraryController.FindItinerary(5, "FORWAR");
        itineraryFindError.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task GetItineraryBetweenTest()
    {
        await _itineraryController.AddItinerary(ItineraryDto5F);

        ItineraryDto itineraryExpected5F = ConvertToItineraryDto(5, "FORWARD", ConnexionsSorted);
        IActionResult itineraryFind =
            await _itineraryController.FindItineraryBetweenStation(5, "FORWARD", "Station0", "Station4");
        VerifyObjectResult<OkObjectResult>(itineraryExpected5F, itineraryFind);

        itineraryFind = await _itineraryController.FindItineraryBetweenStation(5, "FORWARD", "Station1", "Station3");
        VerifyObjectResult<OkObjectResult>(ConvertToItineraryDto(5, "FORWARD", ConnexionsSorted.GetRange(1, 2)),
            itineraryFind);

        IActionResult itineraryFindError =
            await _itineraryController.FindItineraryBetweenStation(5, "FORWARD", "Station5", "Station0");
        itineraryFindError.Should().BeOfType<BadRequestObjectResult>();

        itineraryFindError =
            await _itineraryController.FindItineraryBetweenStation(4, "FORWARD", "Station0", "Station5");
        itineraryFindError.Should().BeOfType<NotFoundObjectResult>();

        itineraryFindError =
            await _itineraryController.FindItineraryBetweenStation(5, "BACKWARD", "Station0", "Station5");
        itineraryFindError.Should().BeOfType<NotFoundObjectResult>();

        itineraryFindError =
            await _itineraryController.FindItineraryBetweenStation(-1, "FORWARD", "Station0", "Station5");
        itineraryFindError.Should().BeOfType<BadRequestObjectResult>();

        itineraryFindError =
            await _itineraryController.FindItineraryBetweenStation(5, "FORWAR", "Station0", "Station5");
        itineraryFindError.Should().BeOfType<BadRequestObjectResult>();

        itineraryFindError = await _itineraryController.FindItineraryBetweenStation(5, "FORWARD", "", "Station5");
        itineraryFindError.Should().BeOfType<BadRequestObjectResult>();

        itineraryFindError = await _itineraryController.FindItineraryBetweenStation(5, "FORWARD", "Station0", "");
        itineraryFindError.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task DeleteItineraryTest()
    {
        await _itineraryController.AddItinerary(ItineraryDto5F);
        await _itineraryController.DeleteItinerary(5, "FORWARD");

        IActionResult itineraryFindError = await _itineraryController.FindItinerary(5, "FORWARD");
        itineraryFindError.Should().BeOfType<NotFoundObjectResult>();

        itineraryFindError =
            await _itineraryController.FindItineraryBetweenStation(5, "FORWARD", "Station0", "Station4");
        itineraryFindError.Should().BeOfType<NotFoundObjectResult>();

        itineraryFindError = await _itineraryController.DeleteItinerary(-1, "FORWARD");
        itineraryFindError.Should().BeOfType<BadRequestObjectResult>();

        itineraryFindError = await _itineraryController.DeleteItinerary(5, "FORWAR");
        itineraryFindError.Should().BeOfType<BadRequestObjectResult>();
    }
}