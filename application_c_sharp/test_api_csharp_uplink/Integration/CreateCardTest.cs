using api_csharp_uplink.Dto;
using FluentAssertions;
using Newtonsoft.Json;
using System.Net;
using System.Text;
using test_api_csharp_uplink.Integration.DBTest;
using Xunit.Abstractions;

namespace test_api_csharp_uplink.Integration;

[Collection("NonParallel")]
public class CreateCardTest(ITestOutputHelper testOutputHelper) : IAsyncLifetime
{
    private readonly HttpClient _client = new();
    private readonly string _request = "http://api_csharp_uplink:8000/api/Card";
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
    public async Task TestGetCards()
    {
        await _influxDbTest.InitializeBucket();

        try
        {
            HttpResponseMessage response = await _client.GetAsync(_request);

            response.EnsureSuccessStatusCode();
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            string responseString = await response.Content.ReadAsStringAsync();
            responseString.Should().NotBeNullOrEmpty();
            responseString.Should().Be("[]");
        }
        catch (HttpRequestException e)
        {
            testOutputHelper.WriteLine($"Request error: {e.Message}");
            Assert.True(false);
        }
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task TestAddCardNormal()
    {
        CardDto card = new()
        {
            LineBus = 1,
            DevEuiCard = "0"
        };

        string json = JsonConvert.SerializeObject(card);
        StringContent content = new(json, Encoding.UTF8, "application/json");

        try
        {
            HttpResponseMessage response = await _client.PostAsync(_request, content);
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
    public async Task TestAddCardError()
    {
        CardDto card = new()
        {
            LineBus = 1,
            DevEuiCard = "1"
        };

        StringContent content = new(JsonConvert.SerializeObject(card), Encoding.UTF8, "application/json");

        try
        {
            HttpResponseMessage response = await _client.PostAsync(_request, content);
            response.EnsureSuccessStatusCode();
            response.StatusCode.Should().Be(HttpStatusCode.Created);

            response = await _client.PostAsync(_request, content);
            response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        }
        catch (HttpRequestException e)
        {
            testOutputHelper.WriteLine($"Request error: {e.Message}");
            Assert.True(false);
        }
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task TestModifyCard()
    {
        CardDto card = new()
        {
            LineBus = 1,
            DevEuiCard = "2"
        };

        string json12 = JsonConvert.SerializeObject(card);
        StringContent content12 = new(json12, Encoding.UTF8, "application/json");
        
        try
        {
            // Add card
            HttpResponseMessage response = await _client.PostAsync(_request, content12);
            response.EnsureSuccessStatusCode();
            response.StatusCode.Should().Be(HttpStatusCode.Created);

            // Get card by DevEUI
            response = await _client.GetAsync($"{_request}/devEuiCard/{card.DevEuiCard}");
            response.EnsureSuccessStatusCode();
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            string responseString = await response.Content.ReadAsStringAsync();
            responseString.Should().NotBeNullOrEmpty();
            responseString.Should().BeEquivalentTo(json12);
            
            // Modify card

            card = new CardDto{ LineBus = 2, DevEuiCard = card.DevEuiCard };
            string json22 = JsonConvert.SerializeObject(card);
            
            StringContent content22 = new StringContent(json22, Encoding.UTF8, "application/json");
            response = await _client.PutAsync(_request, content22);
            response.EnsureSuccessStatusCode();
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            responseString = await response.Content.ReadAsStringAsync();
            responseString.Should().NotBeNullOrEmpty();
            responseString.Should().BeEquivalentTo(json22);
        }
        catch (HttpRequestException e)
        {
            testOutputHelper.WriteLine($"Request error: {e.Message}");
            Assert.True(false);
        }
    }
    
    [Fact]
    [Trait("Category", "Integration")]
    public async Task TestModifyCardError()
    {
        CardDto card = new()
        {
            LineBus = 1,
            DevEuiCard = "2"
        };

        string json = JsonConvert.SerializeObject(card);
        StringContent content = new(json, Encoding.UTF8, "application/json");
        
        try
        {
            // Add card
            HttpResponseMessage response = await _client.PutAsync(_request, content);
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
    public async Task TestGetCard()
    {
        CardDto card = new()
        {
            LineBus = 2,
            DevEuiCard = "2"
        };

        string json = JsonConvert.SerializeObject(card);
        StringContent content = new(json, Encoding.UTF8, "application/json");
        try
        {
            // Add card
            HttpResponseMessage response = await _client.PostAsync(_request, content);
            response.EnsureSuccessStatusCode();
            response.StatusCode.Should().Be(HttpStatusCode.Created);

            // Get card by DevEUI
            response = await _client.GetAsync($"{_request}/devEuiCard/{card.DevEuiCard}");
            response.EnsureSuccessStatusCode();
            response.StatusCode.Should().Be(HttpStatusCode.OK);

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
    public async Task TestGetAllCard()
    {
        CardDto card = new()
        {
            LineBus = 2,
            DevEuiCard = "0"
        };
        try
        {
            await _client.PostAsync(_request, new StringContent(JsonConvert.SerializeObject(card), Encoding.UTF8, "application/json"));

            card.DevEuiCard = "1";
            await _client.PostAsync(_request, new StringContent(JsonConvert.SerializeObject(card), Encoding.UTF8, "application/json"));

            card.DevEuiCard = "2";
            await _client.PostAsync(_request, new StringContent(JsonConvert.SerializeObject(card), Encoding.UTF8, "application/json"));

            HttpResponseMessage response = await _client.GetAsync(_request);
            response.EnsureSuccessStatusCode();
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            string responseString = await response.Content.ReadAsStringAsync();
            responseString.Should().NotBeNullOrEmpty();

            List<CardDto>? buses = JsonConvert.DeserializeObject<List<CardDto>>(responseString);
            buses.Should().NotBeNull();
            buses.Should().HaveCount(3);
        }
        catch (HttpRequestException e)
        {
            testOutputHelper.WriteLine($"Request error: {e.Message}");
            Assert.True(false);
        }
    }
}
