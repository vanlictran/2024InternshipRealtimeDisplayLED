using System.Net;
using System.Text;
using api_csharp_uplink.Dto;
using FluentAssertions;
using Newtonsoft.Json;
using test_api_csharp_uplink.Integration.DBTest;
using Xunit.Abstractions;

namespace test_api_csharp_uplink.Integration;


[Collection("NonParallel")]
public class CreateItineraryTest(ITestOutputHelper testOutputHelper) : IAsyncLifetime
{
    private readonly HttpClient _client = new();
    private const string Request = "http://api_csharp_uplink:8000/api/Itinerary";
    private readonly InfluxDbTest _influxDbTest = new();
    private static readonly List<ConnexionDto> Connexions =
    [
        new ConnexionDto { CurrentNameStation = "Station3", NextNameStation = "Station4" },
        new ConnexionDto { CurrentNameStation = "Station2", NextNameStation = "Station3" },
        new ConnexionDto { CurrentNameStation = "Station0", NextNameStation = "Station1" },
        new ConnexionDto { CurrentNameStation = "Station1", NextNameStation = "Station2" },
        new ConnexionDto { CurrentNameStation = "Station4", NextNameStation = "Station3" }
    ];
    
    private static readonly List<ConnexionDto> ConnexionDtoSorted =
    [
        new ConnexionDto { CurrentNameStation = "Station0", NextNameStation = "Station1" },
        new ConnexionDto { CurrentNameStation = "Station1", NextNameStation = "Station2" },
        new ConnexionDto { CurrentNameStation = "Station2", NextNameStation = "Station3" },
        new ConnexionDto { CurrentNameStation = "Station3", NextNameStation = "Station4" },
    ];
    
    private ItineraryDto ConvertToItineraryDto(int lineNumber, string orientation, List<ConnexionDto> connexions) =>
        new() { LineNumber = lineNumber, Orientation = orientation, Connexions = connexions };
    

    private static StringContent CreateContent<T>(T contentDto) =>
        new(JsonConvert.SerializeObject(contentDto), Encoding.UTF8, "application/json");
    
    public async Task InitializeAsync()
    {
        await _influxDbTest.InitializeBucket();

        await Parallel.ForAsync(0, 5,  async (i, token) =>
        {
            StringContent content = CreateContent(new StationDto
            { NameStation = $"Station{i}", 
                Position = { Latitude = i * 5.0, Longitude = i * 4.0 } });
            await _client.PostAsync("http://api_csharp_uplink:8000/api/Station", content, token);
        });
    }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }
    
    private static async Task VerifyResponseSuccess(HttpResponseMessage response, HttpStatusCode statusCode, string? jsonExpected)
    {
        response.EnsureSuccessStatusCode();
        response.StatusCode.Should().Be(statusCode);

        if (jsonExpected == null) 
            return;
        
        string responseString = await response.Content.ReadAsStringAsync();
        responseString.Should().NotBeNullOrEmpty();
        responseString.Should().BeEquivalentTo(jsonExpected);
    }

    private async Task VerifyCreateItineraryTest(int lineNumber, string orientation)
    {
        ItineraryDto itineraryDto = ConvertToItineraryDto(lineNumber, orientation, Connexions);
        ItineraryDto itineraryDtoExpected = ConvertToItineraryDto(lineNumber, orientation, ConnexionDtoSorted);
        
        StringContent content = CreateContent(itineraryDto);
        string jsonExpected = JsonConvert.SerializeObject(itineraryDtoExpected);
        HttpResponseMessage response = await _client.PostAsync(Request, content);
        await VerifyResponseSuccess(response, HttpStatusCode.Created, jsonExpected);
        
        response = await _client.GetAsync($"{Request}/{lineNumber}/{orientation}");
        await VerifyResponseSuccess(response, HttpStatusCode.OK, jsonExpected);
    }
    
    [Fact]
    [Trait("Category", "Integration")]
    public async Task CreateItineraryTestWithoutError()
    {
        try
        {
            await VerifyCreateItineraryTest(5, "FORWARD");
        
            await VerifyCreateItineraryTest(5, "BACKWARD");
        
            await VerifyCreateItineraryTest(4, "FORWARD");
        }
        catch (HttpRequestException e)
        {
            testOutputHelper.WriteLine($"Request error: {e.Message}");
            Assert.True(false);
        }
    }

    private async Task VerifyCreateItineraryTestError(int lineNumber, string orientation, List<ConnexionDto> connexions, HttpStatusCode statusCode)
    {
        ItineraryDto itineraryDto = ConvertToItineraryDto(lineNumber, orientation, connexions);
        StringContent content = CreateContent(itineraryDto);
        HttpResponseMessage response = await _client.PostAsync(Request, content);
        Assert.Equal(statusCode, response.StatusCode);
    }
    
    [Fact]
    [Trait("Category", "Integration")]
    public async Task CreateItineraryTestError()
    {
        try
        {
            await VerifyCreateItineraryTestError(-1, "FORWARD", Connexions, HttpStatusCode.BadRequest);
            
            await VerifyCreateItineraryTestError(5, "FORWAR", Connexions, HttpStatusCode.BadRequest);

            await VerifyCreateItineraryTestError(5, "FORWARD", [], HttpStatusCode.BadRequest);
            
            await VerifyCreateItineraryTestError(5, "FORWARD",
                [new ConnexionDto{ CurrentNameStation = "Station0", NextNameStation = "Station0" }], HttpStatusCode.BadRequest);
            
            await VerifyCreateItineraryTestError(5, "FORWARD",
                [ Connexions[0], new ConnexionDto{ CurrentNameStation = "Station6", NextNameStation = "Station7"}], HttpStatusCode.NotFound);
            
            StringContent content = CreateContent(ConvertToItineraryDto(5, "FORWARD", Connexions));
            await _client.PostAsync(Request, content);
            
            await VerifyCreateItineraryTestError(5, "FORWARD", Connexions, HttpStatusCode.Conflict);
        }
        catch (HttpRequestException e)
        {
            testOutputHelper.WriteLine($"Request error: {e.Message}");
            Assert.True(false);
        }
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task GetItineraryTest()
    {
        StringContent content = CreateContent(ConvertToItineraryDto(5, "FORWARD", Connexions));
        await _client.PostAsync(Request, content);
        
        HttpResponseMessage response = await _client.GetAsync($"{Request}/5/FORWARD");
        await VerifyResponseSuccess(response, HttpStatusCode.OK, JsonConvert.SerializeObject(ConvertToItineraryDto(5, "FORWARD", ConnexionDtoSorted)));
        
        response = await _client.GetAsync($"{Request}/5/BACKWARD");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        
        response = await _client.GetAsync($"{Request}/4/FORWARD");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        
        response = await _client.GetAsync($"{Request}/-1/FORWARD");
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        
        response = await _client.GetAsync($"{Request}/5/FORWAR");
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task FindItineraryBetweenStationTest()
    {
        StringContent content = CreateContent(ConvertToItineraryDto(5, "FORWARD", Connexions));
        await _client.PostAsync(Request, content);

        string connexionsJson = JsonConvert.SerializeObject(ConvertToItineraryDto(5, "FORWARD", ConnexionDtoSorted));
        HttpResponseMessage response = await _client.GetAsync($"{Request}/5/FORWARD/Station0/Station4");
        await VerifyResponseSuccess(response, HttpStatusCode.OK, connexionsJson);
        
        connexionsJson = JsonConvert.SerializeObject(ConvertToItineraryDto(5, "FORWARD", ConnexionDtoSorted.GetRange(1, 2)));
        response = await _client.GetAsync($"{Request}/5/FORWARD/Station1/Station3");
        await VerifyResponseSuccess(response, HttpStatusCode.OK, connexionsJson);

        string jsonExpect = JsonConvert.SerializeObject(ConvertToItineraryDto(5, "FORWARD", ConnexionDtoSorted));
        response = await _client.GetAsync($"{Request}/5/FORWARD/Station4/Station0");
        await VerifyResponseSuccess(response, HttpStatusCode.OK, jsonExpect);
        
        response = await _client.GetAsync($"{Request}/4/FORWARD/Station0/Station4");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        
        response = await _client.GetAsync($"{Request}/5/BACKWARD/Station0/Station4");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        
        response = await _client.GetAsync($"{Request}/-1/FORWARD/Station0/Station4");
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        
        response = await _client.GetAsync($"{Request}/5/FORWAR/Station0/Station4");
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        
        response = await _client.GetAsync($"{Request}/5/FORWARD/Station0/");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        
        response = await _client.GetAsync($"{Request}/5/FORWARD//Station4");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
    
    [Fact]
    [Trait("Category", "Integration")]
    public async Task DeleteItineraryTest()
    {
        StringContent content = CreateContent(ConvertToItineraryDto(5, "FORWARD", Connexions));
        await _client.PostAsync(Request, content);
        
        HttpResponseMessage response = await _client.DeleteAsync($"{Request}/5/FORWARD");
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        
        response = await _client.GetAsync($"{Request}/5/FORWARD");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        
        response = await _client.GetAsync($"{Request}/-1/FORWARD");
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        
        response = await _client.DeleteAsync($"{Request}/5/FORWAR");
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}