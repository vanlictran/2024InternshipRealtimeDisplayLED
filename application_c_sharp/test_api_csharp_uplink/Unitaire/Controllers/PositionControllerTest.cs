using api_csharp_uplink.Composant;
using api_csharp_uplink.Connectors;
using api_csharp_uplink.Controllers;
using api_csharp_uplink.Dto;
using api_csharp_uplink.Interface;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using test_api_csharp_uplink.Unitaire.DBTest;

namespace test_api_csharp_uplink.Unitaire.Controllers;

public class PositionControllerTest
{
    private readonly PositionController _positionController;
    private readonly PositionCardDto _positionCardDto15140 = new(){ DevEuiNumber = "0", 
        Position = new PositionDto { Latitude = 15.0, Longitude = 14.0 } };
    
    public PositionControllerTest()
    {
        IPositionRepository positionRepository = new DbTestPosition();
        CardComposant cardComposant = new(new DbTestCard());
        _ = cardComposant.CreateCard(5, "0").Result;
        _ = cardComposant.CreateCard(5, "1").Result;
        
        IGraphHelper graphHelper = new GraphHopperTest();
        GraphComposant graphComposant = new GraphComposant(graphHelper);
        IPositionProcessor positionProcessor = new TimeComposant(graphComposant, graphComposant, cardComposant);
        PositionComposant positionRegister = new(positionRepository, positionProcessor);
        _positionController = new PositionController(positionRegister);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task AddPositionTest()
    {
        IActionResult actionResult = await _positionController.AddNewPosition(_positionCardDto15140);
        actionResult.Should().BeOfType<CreatedResult>();
        CreatedResult createdResult = (CreatedResult) actionResult;
        createdResult.Should().NotBeNull();
        createdResult.Value.Should().BeEquivalentTo(_positionCardDto15140);
    }
    
    [Fact]
    [Trait("Category", "Unit")]
    public async Task AddPositionErrorTest()
    {
        PositionCardDto positionCardDtoErrorLatitude = new() { DevEuiNumber = _positionCardDto15140.DevEuiNumber, 
            Position = new PositionDto { Latitude = 91.0, Longitude = _positionCardDto15140.Position.Longitude  } };
        
        IActionResult actionResult = await _positionController.AddNewPosition(positionCardDtoErrorLatitude);
        actionResult.Should().BeOfType<BadRequestObjectResult>();
        
        positionCardDtoErrorLatitude.Position.Latitude = -91.0;
        actionResult = await _positionController.AddNewPosition(positionCardDtoErrorLatitude);
        actionResult.Should().BeOfType<BadRequestObjectResult>();
        
        PositionCardDto positionCardDtoErrorLongitude = new() { DevEuiNumber = _positionCardDto15140.DevEuiNumber, 
            Position = new PositionDto { Latitude = _positionCardDto15140.Position.Latitude, Longitude = 180.01  } };
        
        actionResult = await _positionController.AddNewPosition(positionCardDtoErrorLongitude);
        actionResult.Should().BeOfType<BadRequestObjectResult>();
        
        positionCardDtoErrorLongitude.Position.Longitude = -180.01;
        actionResult = await _positionController.AddNewPosition(positionCardDtoErrorLongitude);
        actionResult.Should().BeOfType<BadRequestObjectResult>();
    }
    
    [Fact]
    [Trait("Category", "Unit")]
    public async Task GetPositionTest()
    {
        PositionCardDto positionCardDto15141 = new(){ DevEuiNumber = "1", 
            Position = new PositionDto { Latitude = 15.0, Longitude = 14.0 } };
        PositionCardDto positionCardDto14140 = new(){ DevEuiNumber = "0", 
            Position = new PositionDto { Latitude = 14.5, Longitude = 14.0 } };
        
        await _positionController.AddNewPosition(_positionCardDto15140);
        await _positionController.AddNewPosition(positionCardDto15141);
        
        IActionResult actionResult = await _positionController.GetLastPositionByDevEuiNumber(_positionCardDto15140.DevEuiNumber);
        actionResult.Should().BeOfType<OkObjectResult>();
        
        OkObjectResult okObjectResult = (OkObjectResult) actionResult;
        okObjectResult.Should().NotBeNull();
        okObjectResult.Value.Should().BeEquivalentTo(_positionCardDto15140);
        
        actionResult = await _positionController.GetLastPositionByDevEuiNumber(positionCardDto15141.DevEuiNumber);
        actionResult.Should().BeOfType<OkObjectResult>();
        
        okObjectResult = (OkObjectResult) actionResult;
        okObjectResult.Should().NotBeNull();
        okObjectResult.Value.Should().BeEquivalentTo(positionCardDto15141);
        
        await _positionController.AddNewPosition(positionCardDto14140);
        
        actionResult = await _positionController.GetLastPositionByDevEuiNumber(positionCardDto14140.DevEuiNumber);
        actionResult.Should().BeOfType<OkObjectResult>();
        
        okObjectResult = (OkObjectResult) actionResult;
        okObjectResult.Should().NotBeNull();
        okObjectResult.Value.Should().BeEquivalentTo(positionCardDto14140);
        
        actionResult = await _positionController.GetLastPositionByDevEuiNumber("2");
        actionResult.Should().BeOfType<NotFoundObjectResult>();
    }
}