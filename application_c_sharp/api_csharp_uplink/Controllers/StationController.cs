using api_csharp_uplink.DirException;
using api_csharp_uplink.Dto;
using api_csharp_uplink.Entities;
using api_csharp_uplink.Interface;
using Microsoft.AspNetCore.Mvc;

namespace api_csharp_uplink.Controllers;

/// <summary>
/// Controller to manage Station.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class StationController(IStationRegister register, IStationFinder finder) : ControllerBase
{
    
    /// <summary>
    /// Register a new station depending on name, longitude and latitude
    /// </summary>
    /// <param name="stationDto">The schema station with Longitude, Latitude and Name of Station</param>
    /// <returns>A station created</returns>
    /// <response code="200">Returns the station.</response>
    /// <response code="400">if the position of the station has not good value or the name is Empty</response>
    /// <response code="500">If there is a server error.</response>
    [HttpPost]
    [ProducesResponseType(typeof(StationDto), 201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> AddStation([FromBody] StationDto stationDto)
    {
        try
        {
           Station station = await register.AddStation(stationDto.Position.Latitude,
                stationDto.Position.Longitude,
                stationDto.NameStation);
            
            return Created($"api/Station?nameStation={stationDto.NameStation}", ConvertStationIntoDto(station));
        }
        catch (Exception e)
        {
            return ErrorManager.HandleError(e);
        }
    }
    
    /// <summary>
    /// Get a station depending on Name of station
    /// </summary>
    /// <param name="nameStation">The name of station</param>
    /// <returns>A response with station</returns>
    /// <response code="200">Returns station</response>
    /// <response code="400">If the name of station is empty or null.</response>
    /// <response code="404">If a station with the name of station is not found.</response>
    /// <response code="500">If there is a server error.</response>
    [HttpGet]
    [ProducesResponseType(typeof(StationDto), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> GetStationByName([FromQuery] string nameStation)
    {
        try
        {
            Station station = await finder.GetStation(nameStation);
            return Ok(ConvertStationIntoDto(station));
        }
        catch (Exception e)
        {
            return ErrorManager.HandleError(e);
        }
    }
    
    /// <summary>
    /// Get a station depending on Name of station
    /// </summary>
    /// <param name="latitude">The latitude of station</param>
    /// <param name="longitude">The longitude of station</param>
    /// <returns>A response with station</returns>
    /// <response code="200">Returns station</response>
    /// <response code="400">If the position is not valid.</response>
    /// <response code="404">If a station with the position of station is not found.</response>
    /// <response code="500">If there is a server error.</response>
    [HttpGet("{latitude:double}/{longitude:double}")]
    [ProducesResponseType(typeof(StationDto), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> GetStationByPosition(double latitude, double longitude)
    {
        try
        {
            Station station = await finder.GetStation(latitude, longitude);
            return Ok(ConvertStationIntoDto(station));
        }
        catch (Exception e)
        {
            return ErrorManager.HandleError(e);
        }
    }

    /**
     * Convert a Station into a StationDto
     * @param station The Station to convert
     */
    private static StationDto ConvertStationIntoDto(Station station)
    {
        return new StationDto
        {
            Position = new PositionDto
            {
                Latitude = station.Position.Latitude,
                Longitude = station.Position.Longitude
            },
            NameStation = station.NameStation
        };
    }
}