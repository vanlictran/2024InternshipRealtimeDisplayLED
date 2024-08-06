using api_csharp_uplink.DB;
using api_csharp_uplink.Entities;
using api_csharp_uplink.Repository;
using Moq;

namespace test_api_csharp_uplink.Unitaire.Repository;

public class StationRepositoryTest
{
    private const string MeasurementStation = "station";
    private readonly Station _station1 = new(new Position(1, 1), "station1");
    private readonly StationDb _stationDb1 = new()
    {
        NameStation = "station1",
        Longitude = 1,
        Latitude = 1
    };
    
    [Fact]
    [Trait("Category", "Unit")]
    public async Task TestAdd()
    {
        Mock<IGlobalInfluxDb> mock = new();
        mock.Setup(globalInfluxDb => globalInfluxDb.Save(It.IsAny<StationDb>()))
            .ReturnsAsync(_stationDb1);
        StationRepository stationRepository = new(mock.Object);
        
        Station result = await stationRepository.Add(_station1);
        Assert.Equal(_station1, result);
    }
    
    [Fact]
    [Trait("Category", "Unit")]
    public async Task TestGetStation()
    {
        Mock<IGlobalInfluxDb> mock = new();
        mock.SetupSequence(globalInfluxDb => globalInfluxDb.Get<StationDb>(MeasurementStation,
                $"|> filter(fn: (r) => r.nameStation == \"{_station1.NameStation}\")"))
            .ReturnsAsync([])
            .ReturnsAsync([_stationDb1]);
        
        StationRepository stationRepository = new(mock.Object);
        Station? result = await stationRepository.GetStation(_station1.NameStation);
        Assert.Null(result);
        
        result = await stationRepository.GetStation(_station1.NameStation);
        Assert.Equal(_station1, result);
    }
    
    [Fact]
    [Trait("Category", "Unit")]
    public async Task TestGetStationByPosition()
    {
        Mock<IGlobalInfluxDb> mock = new();
        mock.SetupSequence(globalInfluxDb => globalInfluxDb.Get<StationDb>(MeasurementStation,
                $"  |> filter(fn: (r) => r.longitude == \"{_station1.Position.Longitude}\" and r.latitude == \"{_station1.Position.Latitude}\")"))
            .ReturnsAsync([])
            .ReturnsAsync([_stationDb1]);
        
        StationRepository stationRepository = new(mock.Object);
        Station? result = await stationRepository.GetStation(_station1.Position);
        Assert.Null(result);
        
        result = await stationRepository.GetStation(_station1.Position);
        Assert.Equal(_station1, result);
    }
}