using System.Net;
using System.Text;
using api_csharp_uplink.Dto;
using FluentAssertions;
using Newtonsoft.Json;
using test_api_csharp_uplink.Integration.DBTest;
using Xunit.Abstractions;

namespace test_api_csharp_uplink.Integration;

[Collection("NonParallel")]
public class GetConnexionTest(ITestOutputHelper testOutputHelper) : IAsyncLifetime
{
    private readonly HttpClient _client = new();
    private const string Request = "http://api_csharp_uplink:8000/api/Connexion";
    private readonly InfluxDbTest _influxDbTest = new();

    private static readonly List<ConnexionDto> Connexions =
    [
        new ConnexionDto { CurrentNameStation = "Station3", NextNameStation = "Station4" },
        new ConnexionDto { CurrentNameStation = "Station2", NextNameStation = "Station3" },
        new ConnexionDto { CurrentNameStation = "Station0", NextNameStation = "Station1" },
        new ConnexionDto { CurrentNameStation = "Station1", NextNameStation = "Station2" },
        new ConnexionDto { CurrentNameStation = "Station4", NextNameStation = "Station3" }
    ];

    private static readonly List<ConnexionDtoWithTime> ConnexionsExpected =
    [
        ConvertToConnexionDto("Station1", "Station2", 5, "FORWARD"),
        ConvertToConnexionDto("Station1", "Station2", 4, "FORWARD"),
        ConvertToConnexionDto("Station1", "Station2", 5, "BACKWARD")
    ];

    private static ItineraryDto
        ConvertToItineraryDto(int lineNumber, string orientation, List<ConnexionDto> connexions) =>
        new() { LineNumber = lineNumber, Orientation = orientation, Connexions = connexions };

    private static ConnexionDtoWithTime ConvertToConnexionDto(string currentNameStation, string nextNameStation,
        int lineNumber, string orientation) =>
        new()
        {
            CurrentNameStation = currentNameStation, NextNameStation = nextNameStation,
            DistanceToNextStation = 5, TimeToNextStation = 5,
            LineNumber = lineNumber, Orientation = orientation
        };

    private static StringContent CreateContent<T>(T contentDto) =>
        new(JsonConvert.SerializeObject(contentDto), Encoding.UTF8, "application/json");
    
    private static void EqualConnexionDto(ConnexionDtoWithTime expect, ConnexionDtoWithTime actual)
    {
        Assert.Equal(expect.CurrentNameStation, actual.CurrentNameStation);
        Assert.Equal(expect.NextNameStation, actual.NextNameStation);
        Assert.Equal(expect.TimeToNextStation, actual.TimeToNextStation);
        Assert.Equal(expect.LineNumber, actual.LineNumber);
        Assert.Equal(expect.Orientation, actual.Orientation);
    }


    public async Task InitializeAsync()
    {
        await _influxDbTest.InitializeBucket();

        await Parallel.ForAsync(0, 5, async (i, token) =>
        {
            StringContent content = CreateContent(new StationDto
            {
                NameStation = $"Station{i}",
                Position = { Latitude = i * 5.0, Longitude = i * 4.0 }
            });
            await _client.PostAsync("http://api_csharp_uplink:8000/api/Station", content, token);
        });

        ItineraryDto itineraryDto5F = ConvertToItineraryDto(5, "FORWARD", Connexions);
        StringContent content = CreateContent(itineraryDto5F);
        await _client.PostAsync("http://api_csharp_uplink:8000/api/Itinerary", content);
    }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }
    

    private async Task VerifyFindConnexionDtoSuccess(ConnexionDtoWithTime connexionDtoExpect)
    {
        HttpResponseMessage response =
            await _client.GetAsync(
                $"{Request}/{connexionDtoExpect.LineNumber}/{connexionDtoExpect.Orientation}/{connexionDtoExpect.CurrentNameStation}");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        ConnexionDtoWithTime connexionDtoActual = JsonConvert.DeserializeObject<ConnexionDtoWithTime>(
            await response.Content.ReadAsStringAsync()) ?? throw new InvalidOperationException();
        
        EqualConnexionDto(connexionDtoExpect, connexionDtoActual);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task FindConnexionTest()
    {
        try
        {
            await VerifyFindConnexionDtoSuccess(ConnexionsExpected[0]);

            await VerifyFindConnexionDtoSuccess(ConvertToConnexionDto("Station3", "Station4", 5, "FORWARD"));

            HttpResponseMessage response = await _client.GetAsync($"{Request}/5/FORWARD/Station4");
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);

            response = await _client.GetAsync($"{Request}/4/FORWARD/Station1");
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);

            response = await _client.GetAsync($"{Request}/-1/FORWARD/Station1");
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            response = await _client.GetAsync($"{Request}/5/FORWAR/Station1");
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            response = await _client.GetAsync($"{Request}/5/FORWARD/");
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }
        catch (HttpRequestException e)
        {
            testOutputHelper.WriteLine($"Request error: {e.Message}");
            Assert.True(false);
        }
    }
    
    private async Task VerifyFindNextConnexionDtoSuccess(List<ConnexionDtoWithTime> connexionDtoWithTimesExpect,
        string stationName)
    {
        HttpResponseMessage responseMessage = await _client.GetAsync($"{Request}/{stationName}");
        string responseString = await responseMessage.Content.ReadAsStringAsync();
        List<ConnexionDtoWithTime> connexions = JsonConvert.DeserializeObject<List<ConnexionDtoWithTime>>(responseString) 
                                                ?? throw new InvalidOperationException();
        
        Parallel.For(0, connexions.Count, i => EqualConnexionDto(connexionDtoWithTimesExpect[i], connexions[i]));
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task FindNextConnexionTest()
    {
        try
        {
            await VerifyFindNextConnexionDtoSuccess([ConnexionsExpected[0]], "Station1");

            ItineraryDto itineraryDto5B = ConvertToItineraryDto(5, "BACKWARD", Connexions);
            ItineraryDto itineraryDto4F = ConvertToItineraryDto(4, "FORWARD", Connexions);
            StringContent content5B = CreateContent(itineraryDto5B);
            StringContent content4F = CreateContent(itineraryDto4F);
            await _client.PostAsync("http://api_csharp_uplink:8000/api/Itinerary", content4F);
            await _client.PostAsync("http://api_csharp_uplink:8000/api/Itinerary", content5B);

            await VerifyFindNextConnexionDtoSuccess([ConnexionsExpected[1], ConnexionsExpected[2], ConnexionsExpected[0]]
                , "Station1");
            
            await VerifyFindNextConnexionDtoSuccess([], "Station5");
            
            HttpResponseMessage response = await _client.GetAsync($"{Request}/");
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }
        catch (HttpRequestException e)
        {
            testOutputHelper.WriteLine($"Request error: {e.Message}");
            Assert.True(false);
        }
    }
}