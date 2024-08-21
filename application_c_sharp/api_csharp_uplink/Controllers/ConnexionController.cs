using api_csharp_uplink.DirException;
using api_csharp_uplink.Dto;
using api_csharp_uplink.Entities;
using api_csharp_uplink.Interface;
using Microsoft.AspNetCore.Mvc;

namespace api_csharp_uplink.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ConnexionController(IConnexionFinder connexionFinder) : ControllerBase
{
    [HttpGet("{lineNumber:int}/{orientation}/{station}")]
    [ProducesResponseType(typeof(ConnexionDtoWithTime), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> FindNextStation(int lineNumber, string orientation, string station)
    {
        try
        {
            ConnexionWithTime connexion = await connexionFinder.FindConnexion(lineNumber, orientation, station);
            return Ok(ConvertConnexionToDto(connexion));
        }
        catch (Exception e)
        {
            return ErrorManager.HandleError(e);
        }
    }
    
    [HttpGet("{station}")]
    [ProducesResponseType(typeof(List<ConnexionDtoWithTime>), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> FindConnexion(string station)
    {
        try
        {
            List<ConnexionWithTime> connexions = await connexionFinder.FindNextConnexion(station);
            return Ok(connexions.AsParallel().Select(ConvertConnexionToDto).ToList());
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            return ErrorManager.HandleError(e);
        }
    }

    private static ConnexionDtoWithTime ConvertConnexionToDto(ConnexionWithTime connexion)
    {
        return new ConnexionDtoWithTime
        {
            LineNumber = connexion.lineNumber,
            CurrentNameStation = connexion.stationCurrent,
            NextNameStation = connexion.stationNext,
            TimeToNextStation = connexion.timeToNextStation,
            Orientation = connexion.orientation.ToString(),
            DistanceToNextStation = connexion.distanceToNextStation
        };
    }
}