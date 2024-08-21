using api_csharp_uplink.DirException;
using api_csharp_uplink.Dto;
using api_csharp_uplink.Entities;
using api_csharp_uplink.Interface;
using Microsoft.AspNetCore.Mvc;

namespace api_csharp_uplink.Controllers;

/// <summary>
/// Controller to manage position depending on DevEUI number.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class PositionController(IPositionRegister positionRegister) : ControllerBase
{
    
    /// <summary>
    /// Register a new position of card depending on DevEUI number and position
    /// </summary>
    /// <param name="positionCardDto">The schema position of the Card with Longitude, Latitude and DevEuiCard</param>
    /// <returns>A position of the card created</returns>
    /// <response code="200">Returns the position of the card.</response>
    /// <response code="400">if the position of card has not good value</response>
    /// <response code="500">If there is a server error.</response>
    [HttpPost]
    [ProducesResponseType(typeof(PositionCardDto), 201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> AddNewPosition([FromBody] PositionCardDto positionCardDto)
    {
        try
        {
           PositionCard positionCard = await positionRegister.AddPosition(positionCardDto.Position.Latitude,
                positionCardDto.Position.Longitude,
                positionCardDto.DevEuiNumber);
            
            return Created($"api/devEuiNumber/{positionCardDto.DevEuiNumber}", ConvertPositionCardIntoDto(positionCard));
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            return ErrorManager.HandleError(e);
        }
    }
    
    /// <summary>
    /// Get a position of card depending on DevEUI number
    /// </summary>
    /// <param name="devEuiCard">The devEui number of card</param>
    /// <returns>A response with position of card</returns>
    /// <response code="200">Returns the position of the card.</response>
    /// <response code="404">If a bus with the specified DevEUI number is not found.</response>
    /// <response code="500">If there is a server error.</response>
    [HttpGet("devEuiCard/{devEuiCard}")]
    [ProducesResponseType(typeof(PositionCardDto), 200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> GetLastPositionByDevEuiNumber(string devEuiCard)
    {
        try
        {
            PositionCard positionCard = await positionRegister.GetLastPosition(devEuiCard);
            return Ok(ConvertPositionCardIntoDto(positionCard));
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            return ErrorManager.HandleError(e);
        }
    }
    
    

    [HttpGet("{nameStation}")]
    [ProducesResponseType(typeof(string), 200)]
    public IActionResult TimeBusToNextStation(string nameStation)
    {
        if (string.IsNullOrEmpty(nameStation))
            return BadRequest();

        Random rnd = new Random();
        int time = rnd.Next(1, 90);
        Console.WriteLine("Time to station : " + time + " mn");
        return Ok(time + " mn");
    }

    /**
     * Convert a PositionCard into a PositionCardDto
     * @param positionCard The positionCard to convert
     */
    private static PositionCardDto ConvertPositionCardIntoDto(PositionCard positionCard)
    {
        return new PositionCardDto
        {
            DevEuiNumber = positionCard.DevEuiCard,
            Position = new PositionDto
            {
                Latitude = positionCard.Position.Latitude,
                Longitude = positionCard.Position.Longitude
            }
        };
    }
}
