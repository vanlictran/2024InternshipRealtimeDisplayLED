using api_csharp_uplink.Composant;
using api_csharp_uplink.DirException;
using api_csharp_uplink.Entities;
using api_csharp_uplink.Interface;
using test_api_csharp_uplink.Unitaire.DBTest;

namespace test_api_csharp_uplink.Unitaire.Composant;

public class PositionComposantTest
{
    private readonly PositionComposant _positionComposant;
    private readonly PositionCard _positionBus15140 = new(new Position(15.0, 14.0), "0");
    
    public PositionComposantTest()
    {
        IPositionRepository positionRepository = new DbTestPosition();
        CardComposant cardComposant = new(new DbTestCard());
        _ = cardComposant.CreateCard(5, "0").Result;
        _ = cardComposant.CreateCard(5, "1").Result;
        
        GraphComposant graphComposant = new GraphComposant(new GraphHopperTest());
        IPositionProcessor positionProcessor = new TimeComposant(graphComposant, graphComposant, cardComposant);
        
        _positionComposant = new PositionComposant(positionRepository, positionProcessor);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task AddPositionTest()
    {
        PositionCard positionCardActual = await _positionComposant.AddPosition(_positionBus15140.Position.Latitude, 
            _positionBus15140.Position.Longitude, _positionBus15140.DevEuiCard);
        Assert.NotNull(positionCardActual);
        Assert.Equal(_positionBus15140, positionCardActual);
        
        PositionCard positionCardActual2 = await _positionComposant.GetLastPosition("0");
        Assert.NotNull(positionCardActual2);
        Assert.Equal(_positionBus15140, positionCardActual2);
    }
    
    [Fact]
    [Trait("Category", "Unit")]
    public async Task AddErrorPositionTest()
    {
        await Assert.ThrowsAsync<ValueNotCorrectException>(() => _positionComposant.AddPosition(90.01,
            _positionBus15140.Position.Longitude, _positionBus15140.DevEuiCard));
        
        await Assert.ThrowsAsync<ValueNotCorrectException>(() => _positionComposant.AddPosition(-90.01,
            _positionBus15140.Position.Longitude, _positionBus15140.DevEuiCard));
        
        await Assert.ThrowsAsync<ValueNotCorrectException>(() => _positionComposant.AddPosition(_positionBus15140.Position.Latitude,
            180.01, _positionBus15140.DevEuiCard));
        
        await Assert.ThrowsAsync<ValueNotCorrectException>(() => _positionComposant.AddPosition(_positionBus15140.Position.Latitude,
            -180.01, _positionBus15140.DevEuiCard));
    }
    
    [Fact]
    [Trait("Category", "Unit")]
    public async Task GetPositionTest()
    {
        PositionCard positionBus15141 = new PositionCard(new Position(15.0, 14.0), "1");
        PositionCard positionBus14140 = new PositionCard(new Position(14.5, 14.0), "0");
        
        await _positionComposant.AddPosition(_positionBus15140.Position.Latitude, 
            _positionBus15140.Position.Longitude, _positionBus15140.DevEuiCard);
        await _positionComposant.AddPosition(positionBus15141.Position.Latitude, 
            positionBus15141.Position.Longitude, positionBus15141.DevEuiCard);
        
        PositionCard positionActual = await _positionComposant.GetLastPosition("0");
        Assert.NotNull(positionActual);
        Assert.Equal(_positionBus15140, positionActual);
        
        positionActual = await _positionComposant.GetLastPosition("1");
        Assert.NotNull(positionActual);
        Assert.Equal(positionBus15141, positionActual);

        await _positionComposant.AddPosition(positionBus14140.Position.Latitude,
            positionBus14140.Position.Longitude, positionBus14140.DevEuiCard);
        
        positionActual = await _positionComposant.GetLastPosition("0");
        Assert.NotNull(positionActual);
        Assert.Equal(positionBus14140, positionActual);
        
        positionActual = await _positionComposant.GetLastPosition("1");
        Assert.NotNull(positionActual);
        Assert.Equal(positionBus15141, positionActual);
    }
    
    [Fact]
    [Trait("Category", "Unit")]
    public async Task GetErrorPositionTest()
    {
        await Assert.ThrowsAsync<NotFoundException>(() => _positionComposant.GetLastPosition("-1"));
    }
}