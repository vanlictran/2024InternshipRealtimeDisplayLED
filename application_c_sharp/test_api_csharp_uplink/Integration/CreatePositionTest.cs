using System.Net;
using System.Text;
using api_csharp_uplink.Dto;
using FluentAssertions;
using Newtonsoft.Json;
using test_api_csharp_uplink.Integration.DBTest;
using Xunit.Abstractions;

namespace test_api_csharp_uplink.Integration;

[Collection("NonParallel")]
public class CreatePositionTest(ITestOutputHelper testOutputHelper) : IAsyncLifetime
{
    private readonly HttpClient _client = new();

    private readonly PositionCardDto _positionCardDto15140 = new(){ DevEuiNumber = "0", 
        Position = new PositionDto { Latitude = 15.01, Longitude = 14.01 } };
    private const string Request = "http://api_csharp_uplink:8000/api/Position";
    private readonly InfluxDbTest _influxDbTest = new();

    public async Task InitializeAsync()
    {
        await _influxDbTest.InitializeBucket();
    }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task TestGetPosition()
    {
        try
        {
            HttpResponseMessage response = await _client.GetAsync(Request + "/devEuiNumber/0");
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }
        catch (HttpRequestException e)
        {
            testOutputHelper.WriteLine($"Request error: {e.Message}");
            Assert.True(false);
        }
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task TestAddPositionNormal()
    {
        string json = JsonConvert.SerializeObject(_positionCardDto15140);
        StringContent content = new(json, Encoding.UTF8, "application/json");

        try
        {
            HttpResponseMessage response = await _client.PostAsync(Request, content);
            response.EnsureSuccessStatusCode();
            response.StatusCode.Should().Be(HttpStatusCode.Created);

            string responseString = await response.Content.ReadAsStringAsync();
            responseString.Should().NotBeNullOrEmpty();
            responseString.Should().BeEquivalentTo(json);
        }
        catch (HttpRequestException e)
        {
            testOutputHelper.WriteLine($"Request error: {e.Message}");
            Assert.True(false);
        }
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task TestAddPositionError()
    {
        try
        {
            PositionCardDto positionCardDtoErrorLatitude = new()
            {
                DevEuiNumber = _positionCardDto15140.DevEuiNumber,
                Position = new PositionDto { Latitude = 91.0, Longitude = _positionCardDto15140.Position.Longitude }
            };
            
            StringContent content = new(JsonConvert.SerializeObject(positionCardDtoErrorLatitude), Encoding.UTF8,
                "application/json");

            HttpResponseMessage response = await _client.PostAsync(Request, content);
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            positionCardDtoErrorLatitude.Position.Latitude = -91.0;
            
            content = new StringContent(JsonConvert.SerializeObject(positionCardDtoErrorLatitude), Encoding.UTF8,
                "application/json");

            response = await _client.PostAsync(Request, content);
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);


            PositionCardDto positionCardDtoErrorLongitude = new()
            {
                DevEuiNumber = _positionCardDto15140.DevEuiNumber,
                Position = new PositionDto { Latitude = _positionCardDto15140.Position.Latitude, Longitude = 180.01 }
            };
            
            content = new StringContent(JsonConvert.SerializeObject(positionCardDtoErrorLongitude), Encoding.UTF8,
                "application/json");

            response = await _client.PostAsync(Request, content);
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            positionCardDtoErrorLongitude.Position.Longitude = -180.01;
            
            content = new StringContent(JsonConvert.SerializeObject(positionCardDtoErrorLongitude), Encoding.UTF8,
                "application/json");

            response = await _client.PostAsync(Request, content);
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }
        catch (HttpRequestException e)
        {
            testOutputHelper.WriteLine($"Request error: {e.Message}");
            Assert.True(false);
        }
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task TestGetPositions()
    {
        PositionCardDto positionCardDto15141 = new(){ DevEuiNumber = "1", 
            Position = new PositionDto { Latitude = 15.01, Longitude = 14.01 } };
        PositionCardDto positionCardDto14140 = new(){ DevEuiNumber = "0", 
            Position = new PositionDto { Latitude = 14.5, Longitude = 14.01 } };

        string json15140 = JsonConvert.SerializeObject(_positionCardDto15140);
        StringContent content15140 = new(json15140, Encoding.UTF8, "application/json");
        
        string json15141 = JsonConvert.SerializeObject(positionCardDto15141);
        StringContent content15141 = new(json15141, Encoding.UTF8, "application/json");
        
        string json14140 = JsonConvert.SerializeObject(positionCardDto14140);
        StringContent content14140 = new(json14140, Encoding.UTF8, "application/json");
        
        try
        {
            await _client.PostAsync(Request, content15140);
            await _client.PostAsync(Request, content15141);
            HttpResponseMessage response = await _client.GetAsync($"{Request}/devEuiCard/{_positionCardDto15140.DevEuiNumber}");
            response.EnsureSuccessStatusCode();
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            string responseString = await response.Content.ReadAsStringAsync();
            responseString.Should().NotBeNullOrEmpty();
            responseString.Should().BeEquivalentTo(json15140);
            
            response = await _client.GetAsync($"{Request}/devEuiCard/{positionCardDto15141.DevEuiNumber}");
            response.EnsureSuccessStatusCode();
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            responseString = await response.Content.ReadAsStringAsync();
            responseString.Should().NotBeNullOrEmpty();
            responseString.Should().BeEquivalentTo(json15141);
            
            await _client.PostAsync(Request, content14140);
            
            response = await _client.GetAsync($"{Request}/devEuiCard/{_positionCardDto15140.DevEuiNumber}");
            response.EnsureSuccessStatusCode();
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            responseString = await response.Content.ReadAsStringAsync();
            responseString.Should().NotBeNullOrEmpty();
            responseString.Should().BeEquivalentTo(json14140);
            
            response = await _client.GetAsync($"{Request}/devEuiCard/2");
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        }
        catch (HttpRequestException e)
        {
            testOutputHelper.WriteLine($"Request error: {e.Message}");
            Assert.True(false);
        }
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task TestGetPositionsError()
    {
        try
        {
            HttpResponseMessage response = await _client.GetAsync($"{Request}/devEuiCard/-1");
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }
        catch (HttpRequestException e)
        {
            testOutputHelper.WriteLine($"Request error: {e.Message}");
            Assert.True(false);
        }
    }
}