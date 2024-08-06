using System.Net;
using System.Text;
using api_csharp_uplink.Dto;
using FluentAssertions;
using Newtonsoft.Json;
using test_api_csharp_uplink.Integration.DBTest;
using Xunit.Abstractions;

namespace test_api_csharp_uplink.Integration;

[Collection("NonParallel")]
public class CreateStationTest(ITestOutputHelper testOutputHelper) : IAsyncLifetime
{
    private readonly HttpClient _client = new();

    private readonly StationDto _stationDtoStation1 = new(){ NameStation = "Station1", 
        Position = new PositionDto { Latitude = 15.01, Longitude = 14.01 } };
    private const string Request = "http://api_csharp_uplink:8000/api/Station";
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
    public async Task TestAddStationNormal()
    {
        string json = JsonConvert.SerializeObject(_stationDtoStation1);
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
    public async Task TestAddStationError()
    {
        try
        {
            StationDto stationDtoErrorLatitude = new()
            {
                NameStation = _stationDtoStation1.NameStation,
                Position = new PositionDto { Latitude = 91.0, Longitude = _stationDtoStation1.Position.Longitude }
            };
            
            StringContent content = new(JsonConvert.SerializeObject(stationDtoErrorLatitude), Encoding.UTF8,
                "application/json");

            HttpResponseMessage response = await _client.PostAsync(Request, content);
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            stationDtoErrorLatitude.Position.Latitude = -91.0;
            
            content = new StringContent(JsonConvert.SerializeObject(stationDtoErrorLatitude), Encoding.UTF8,
                "application/json");

            response = await _client.PostAsync(Request, content);
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);


            StationDto stationDtoErrorLongitude = new()
            {
                NameStation = _stationDtoStation1.NameStation,
                Position = new PositionDto { Latitude = _stationDtoStation1.Position.Latitude, Longitude = 180.01 }
            };
            
            content = new StringContent(JsonConvert.SerializeObject(stationDtoErrorLongitude), Encoding.UTF8,
                "application/json");

            response = await _client.PostAsync(Request, content);
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            stationDtoErrorLongitude.Position.Longitude = -180.01;
            
            content = new StringContent(JsonConvert.SerializeObject(stationDtoErrorLongitude), Encoding.UTF8,
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
    public async Task TestGetStationByName()
    {
        StationDto stationDtoStation2 = new(){ NameStation = "Station2", 
            Position = new PositionDto { Latitude = 15.01, Longitude = 14.01 } };

        string jsonStation1 = JsonConvert.SerializeObject(_stationDtoStation1);
        StringContent contentStation1 = new(jsonStation1, Encoding.UTF8, "application/json");
        
        string jsonStation2 = JsonConvert.SerializeObject(stationDtoStation2);
        StringContent contentStation2 = new(jsonStation2, Encoding.UTF8, "application/json");
        
        try
        {
            await _client.PostAsync(Request, contentStation1);
            
            HttpResponseMessage response = await _client.GetAsync($"{Request}?nameStation={_stationDtoStation1.NameStation}");
            response.EnsureSuccessStatusCode();
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            string responseString = await response.Content.ReadAsStringAsync();
            responseString.Should().NotBeNullOrEmpty();
            responseString.Should().BeEquivalentTo(jsonStation1);
            
            response = await _client.GetAsync(Request + $"?nameStation={stationDtoStation2.NameStation}");
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
            
            await _client.PostAsync(Request, contentStation2);
            
            response = await _client.GetAsync($"{Request}?nameStation={stationDtoStation2.NameStation}");
            response.EnsureSuccessStatusCode();
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            responseString = await response.Content.ReadAsStringAsync();
            responseString.Should().NotBeNullOrEmpty();
            responseString.Should().BeEquivalentTo(jsonStation2);
        }
        catch (HttpRequestException e)
        {
            testOutputHelper.WriteLine($"Request error: {e.Message}");
            Assert.True(false);
        }
    }
    
    [Fact]
    [Trait("Category", "Integration")]
    public async Task TestGetStationByNameError()
    {
        try
        {
            HttpResponseMessage response = await _client.GetAsync($"{Request}");
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
    public async Task TestGetStationByPosition()
    {
        StationDto stationDtoStation2 = new(){ NameStation = "Station2", 
            Position = new PositionDto { Latitude = 15.02, Longitude = 14.02 } };

        string jsonStation1 = JsonConvert.SerializeObject(_stationDtoStation1);
        StringContent contentStation1 = new(jsonStation1, Encoding.UTF8, "application/json");
        
        string jsonStation2 = JsonConvert.SerializeObject(stationDtoStation2);
        StringContent contentStation2 = new(jsonStation2, Encoding.UTF8, "application/json");
        
        try
        {
            await _client.PostAsync(Request, contentStation1);
            
            HttpResponseMessage response = await _client.GetAsync($"{Request}/{_stationDtoStation1.Position.Latitude}/{_stationDtoStation1.Position.Longitude}");
            response.EnsureSuccessStatusCode();
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            string responseString = await response.Content.ReadAsStringAsync();
            responseString.Should().NotBeNullOrEmpty();
            responseString.Should().BeEquivalentTo(jsonStation1);
            
            response = await _client.GetAsync(Request + $"/{stationDtoStation2.Position.Latitude}/{stationDtoStation2.Position.Longitude}");
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
            
            await _client.PostAsync(Request, contentStation2);
            
            response = await _client.GetAsync($"{Request}/{stationDtoStation2.Position.Latitude}/{stationDtoStation2.Position.Longitude}");
            response.EnsureSuccessStatusCode();
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            responseString = await response.Content.ReadAsStringAsync();
            responseString.Should().NotBeNullOrEmpty();
            responseString.Should().BeEquivalentTo(jsonStation2);
        }
        catch (HttpRequestException e)
        {
            testOutputHelper.WriteLine($"Request error: {e.Message}");
            Assert.True(false);
        }
    }
    
    [Fact]
    [Trait("Category", "Integration")]
    public async Task TestGetStationByPositionError()
    {
        try
        {
            HttpResponseMessage response = await _client.GetAsync($"{Request}/90.01/{_stationDtoStation1.Position.Longitude}");
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            
            response = await _client.GetAsync($"{Request}/-90.01/{_stationDtoStation1.Position.Longitude}");
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            
            response = await _client.GetAsync($"{Request}/{_stationDtoStation1.Position.Latitude}/180.01");
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            
            response = await _client.GetAsync($"{Request}/{_stationDtoStation1.Position.Latitude}/-180.01");
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }
        catch (HttpRequestException e)
        {
            testOutputHelper.WriteLine($"Request error: {e.Message}");
            Assert.True(false);
        }
    }
}