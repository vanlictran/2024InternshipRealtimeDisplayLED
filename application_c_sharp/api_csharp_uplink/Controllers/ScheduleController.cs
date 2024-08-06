using Microsoft.AspNetCore.Mvc;
using api_csharp_uplink.DirException;
using api_csharp_uplink.Dto;
using api_csharp_uplink.Entities;
using api_csharp_uplink.Interface;

namespace api_csharp_uplink.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ScheduleController(IScheduleRegistration scheduleRegistration, IScheduleFinder scheduleFinder) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(ScheduleDto), 201)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> AddSchedule(ScheduleDto schedule)
    {
        try
        {
            List<DateTime> hours = schedule.Hours.Select(hour =>
                new DateTime(2024, 7, 15, hour.Hour, hour.Minute, 0, DateTimeKind.Utc)).ToList();
            Schedule scheduleAdd = await scheduleRegistration.AddSchedule(schedule.NameStation, schedule.LineNumber,
                schedule.Orientation, hours);
            
            return Created($"api/Schedule/{scheduleAdd.StationName}/{scheduleAdd.LineNumber}/{scheduleAdd.Orientation.ToString()}"
                ,ConvertScheduleToScheduleDto(scheduleAdd));
        }
        catch (Exception e)
        {
            return ErrorManager.HandleError(e);
        }
    }
    
    /// <summary>
    /// Get all the schedule of a bus station forward depending on NameStation and LineNumber
    /// </summary>
    /// <param name="stationName">The name of the bus station</param>
    /// <param name="lineNumber">The line number of the bus station</param>
    /// <returns>A response with all the schedule of the day or NotFound response</returns>
    [HttpGet("forward/{stationName}/{lineNumber:int}")]
    [ProducesResponseType(typeof(ScheduleDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetScheduleForward(string stationName, int lineNumber)
    {
        try
        {
            Schedule schedule = await scheduleFinder.FindSchedule(stationName, lineNumber, Orientation.FORWARD);
            return Ok(ConvertScheduleToScheduleDto(schedule));
        }
        catch (Exception e)
        {
            return ErrorManager.HandleError(e);
        }
    }
    
    /// <summary>
    /// Get all the schedule of a bus station backward depending on NameStation and LineNumber
    /// </summary>
    /// <param name="stationName">The name of the bus station</param>
    /// <param name="lineNumber">The line number of the bus station</param>
    /// <returns>A response with all the schedule of the day or NotFound response</returns>
    [HttpGet("backward/{stationName}/{lineNumber:int}")]
    [ProducesResponseType(typeof(ScheduleDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetScheduleBackward(string stationName, int lineNumber)
    {
        try
        {
            Schedule schedule = await scheduleFinder.FindSchedule(stationName, lineNumber, Orientation.BACKWARD);
            return Ok(ConvertScheduleToScheduleDto(schedule));
        }
        catch (Exception e)
        {
            return ErrorManager.HandleError(e);
        }
    }
    
    /// <summary>
    /// Get all the schedule of a bus station forward depending on NameStation
    /// </summary>
    /// <param name="stationName">The name of the bus station</param>
    /// <returns>A response with all the schedule of the day or NotFound response</returns>
    [HttpGet("forward/{stationName}")]
    [ProducesResponseType(typeof(List<ScheduleDto>), 200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> GetScheduleByStationNameForward(string stationName)
    {
        try
        {
            List<Schedule> schedules = await scheduleFinder.FindScheduleByStationNameOrientation(stationName, Orientation.FORWARD);
            return Ok(schedules.AsParallel().Select(ConvertScheduleToScheduleDto).ToList());
        }
        catch (Exception e)
        {
            return ErrorManager.HandleError(e);
        }
    }
    
    /// <summary>
    /// Get all the schedule of a bus station backward depending on NameStation
    /// </summary>
    /// <param name="stationName">The name of the bus station</param>
    /// <returns>A response with all the schedule of the day or NotFound response</returns>
    [HttpGet("backward/{stationName}")]
    [ProducesResponseType(typeof(List<ScheduleDto>), 200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> GetScheduleByStationNameBackward(string stationName)
    {
        try
        {
            List<Schedule> schedules = await scheduleFinder.FindScheduleByStationNameOrientation(stationName, Orientation.BACKWARD);
            return Ok(schedules.AsParallel().Select(ConvertScheduleToScheduleDto).ToList());
        }
        catch (Exception e)
        {
            return ErrorManager.HandleError(e);
        }
    } 
    private static ScheduleDto ConvertScheduleToScheduleDto(Schedule schedule)
    {
        return new ScheduleDto
        {
            NameStation = schedule.StationName,
            LineNumber = schedule.LineNumber,
            Orientation = schedule.Orientation.ToString(),
            Hours = schedule.Hours.Select(hour => new HourDto {Hour = hour.Hour, Minute = hour.Minute}).ToList()
        };
    }
}