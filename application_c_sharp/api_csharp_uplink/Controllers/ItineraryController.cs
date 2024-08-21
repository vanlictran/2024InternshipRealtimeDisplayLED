using api_csharp_uplink.DirException;
using api_csharp_uplink.Dto;
using api_csharp_uplink.Entities;
using api_csharp_uplink.Interface;
using Microsoft.AspNetCore.Mvc;

namespace api_csharp_uplink.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ItineraryController(IItineraryRegister itineraryRegister, IItineraryFinder itineraryFinder) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(ItineraryDto), 201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> AddItinerary(ItineraryDto itineraryDto)
    {
        try
        {
            Itinerary itinerary =
                await itineraryRegister.AddItinerary(itineraryDto.LineNumber, itineraryDto.Orientation, itineraryDto.Connexions);
            return Created($"{itinerary.lineNumber}/{itinerary.orientation.ToString()}", ConvertItineraryToDto(itinerary));
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            return ErrorManager.HandleError(e);
        }
    }
    
    [HttpPost("addOneStation")]
    [ProducesResponseType(typeof(StationDto), 201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> AddOneStation(string nameStation)
    {
        try
        {
            Station station = await itineraryRegister.AddOneStation(nameStation);
            StationDto stationDtoGraph = new StationDto
            {
                NameStation = station.NameStation,
                Position = new PositionDto
                {
                    Longitude = station.Position.Longitude,
                    Latitude = station.Position.Latitude
                }
            };
            return Created("", stationDtoGraph);
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            return ErrorManager.HandleError(e);
        }
    }
    
    [HttpGet("{lineNumber:int}/{orientation}")]
    [ProducesResponseType(typeof(ItineraryDto), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> FindItinerary(int lineNumber, string orientation)
    {
        try
        {
            Itinerary itinerary = await itineraryFinder.FindItinerary(lineNumber, orientation);
            return Ok(ConvertItineraryToDto(itinerary));
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            return ErrorManager.HandleError(e);
        }
    }
    
    [HttpGet("{lineNumber:int}/{orientation}/{station1}/{station2}")]
    [ProducesResponseType(typeof(ItineraryDto), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> FindItineraryBetweenStation(int lineNumber, string orientation, 
        string station1, string station2)
    {
        try
        {
            Itinerary itinerary = await itineraryFinder.FindItineraryBetweenStation(lineNumber, orientation, station1, station2);
            return Ok(ConvertItineraryToDto(itinerary));
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            return ErrorManager.HandleError(e);
        }
    }
    
    [HttpDelete("{lineNumber:int}/{orientation}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(400)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> DeleteItinerary(int lineNumber, string orientation)
    {
        try
        {
            bool deleted = await itineraryRegister.DeleteItinerary(lineNumber, orientation);
            Console.WriteLine(deleted);
            return deleted? NoContent() :  StatusCode(500, "An error occurred while deleting the itinerary.");
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            return ErrorManager.HandleError(e);
        }
    }
    
    private static ItineraryDto ConvertItineraryToDto(Itinerary itinerary)
    {
        List<ConnexionDto> connexionDtos = [..new ConnexionDto[itinerary.connexions.Count - 1]];
        
        Parallel.For(0, itinerary.connexions.Count - 1,i =>
        {
            Connexion connexionCurrent = itinerary.connexions[i];
            Connexion connexionNext = itinerary.connexions[i + 1];

            connexionDtos[i] = new ConnexionDto
            {
                CurrentNameStation = connexionCurrent.stationCurrent.NameStation,
                NextNameStation = connexionNext.stationCurrent.NameStation,
            };
        });
        
        return new ItineraryDto
        {
            LineNumber = itinerary.lineNumber,
            Orientation = itinerary.orientation.ToString(),
            Connexions = connexionDtos
        };
    }
}