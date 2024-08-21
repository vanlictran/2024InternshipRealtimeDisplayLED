using api_csharp_uplink.DirException;
using api_csharp_uplink.Interface;
using Microsoft.AspNetCore.Mvc;

namespace api_csharp_uplink.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TimeEngineController(ITimeProcessor timeProcessor) : ControllerBase
{ 
    [HttpGet("{nameStation}")]
    [ProducesResponseType(typeof(string), 200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> GetTimeOneStation(string nameStation)
    {
        try
        {
            int time = await timeProcessor.GetTimeToNextStation(nameStation);
            return Ok(time + " mn");
        }
        catch (Exception e)
        {
            return ErrorManager.HandleError(e);
        }
    }
    
    [HttpGet("{lineNumber:int}/{station1}/{station2}")]
    [ProducesResponseType(typeof(int), 200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> GetTimeDistance(int lineNumber, string station1, string station2)
    {
        try
        {
            int time = await timeProcessor.GetTimeBetweenStations(lineNumber, station1, station2);
            return Ok(time);
        }
        catch (Exception e)
        {
            return ErrorManager.HandleError(e);
        }
    }
}