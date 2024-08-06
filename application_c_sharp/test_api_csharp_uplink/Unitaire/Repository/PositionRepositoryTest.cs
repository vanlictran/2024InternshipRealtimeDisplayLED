using api_csharp_uplink.DB;
using api_csharp_uplink.Entities;
using api_csharp_uplink.Repository;
using Moq;

namespace test_api_csharp_uplink.Unitaire.Repository;

public class PositionRepositoryTest
{
    private const string MeasurementPosition = "positionCard";
    private readonly PositionCard _positionCard0 = new(new Position(1.0, 2.0), "0");
    private readonly PositionCardDb _positionCardDb0 = new()
    {
        DevEuiCard = "0",
        Latitude = 1.0,
        Longitude = 2.0
    };
    
    [Fact]
    [Trait("Category", "Unit")]
    public async Task TestAdd()
    {
        Mock<IGlobalInfluxDb> mock = new();
        mock.Setup(globalInfluxDb => globalInfluxDb.Save(It.IsAny<PositionCardDb>()))
            .ReturnsAsync(_positionCardDb0);
        PositionRepository positionRepository = new(mock.Object);
        
        PositionCard result = await positionRepository.Add(_positionCard0);
        Assert.Equal(_positionCard0, result);
    }
    
    [Fact]
    [Trait("Category", "Unit")]
    public async Task TestGetLast()
    {
        string query = "from(bucket: \"mybucket\")\n  |> range(start: -15m)\n  "
                       + $"|> filter(fn: (r) => r._measurement == \"{MeasurementPosition}\" and r.devEuiCard == \"{_positionCard0.DevEuiCard}\")\n  " +
                       "|> last()";
        Mock<IGlobalInfluxDb> mock = new();
        mock.Setup(globalInfluxDb => globalInfluxDb.Get<PositionCardDb>(query))
            .ReturnsAsync([_positionCardDb0]);
        PositionRepository positionRepository = new(mock.Object);
        
        PositionCard? result = await positionRepository.GetLast(_positionCard0.DevEuiCard);
        Assert.Equal(_positionCard0, result);
    }
}