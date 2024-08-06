using api_csharp_uplink.Composant;
using api_csharp_uplink.Controllers;
using api_csharp_uplink.Dto;
using api_csharp_uplink.Entities;
using api_csharp_uplink.Interface;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using test_api_csharp_uplink.Unitaire.DBTest;

namespace test_api_csharp_uplink.Unitaire.Controllers;

public class ScheduleControllerTest
{
    private readonly ScheduleController _scheduleController;
    private readonly HourDto _hourDto1010 = new(){Hour = 10, Minute = 10};
    private readonly HourDto _hourDto1020 = new(){Hour = 10, Minute = 20};
    
    public ScheduleControllerTest()
    {
        IScheduleRepository scheduleRepository = new DbTestSchedule();
        IStationRepository stationRepository = new DbTestStation();
        IStationFinder stationFinder = new StationComposant(stationRepository);
        
        stationRepository.Add(new Station(10.0, 10.0, "Station1"));
        stationRepository.Add(new Station(10.0, 10.0, "Station2"));
        
        ScheduleComposant scheduleComposant = new ScheduleComposant(scheduleRepository, stationFinder);
        _scheduleController = new ScheduleController(scheduleComposant, scheduleComposant);
    }
    
    private static void VerifyObjectResult<T>(ScheduleDto scheduleExpected, IActionResult result) where T : ObjectResult
    {
        result.Should().BeOfType<T>();
        T okObjectResult = (T) result;
        okObjectResult.Should().NotBeNull();
        okObjectResult.Value.Should().BeEquivalentTo(scheduleExpected);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task TestAddSchedule()
    {
        ScheduleDto scheduleExpected = new(){Hours = [_hourDto1010], LineNumber = 1, NameStation = "Station1", Orientation = "FORWARD"};
        
        IActionResult result = await _scheduleController.AddSchedule(
            new ScheduleDto{Hours = [_hourDto1010, _hourDto1010], LineNumber = 1, NameStation = "Station1", Orientation = "FORWARD"});
        VerifyObjectResult<CreatedResult>(scheduleExpected, result);
        
        result = await _scheduleController.GetScheduleForward("Station1", 1);
        VerifyObjectResult<OkObjectResult>(scheduleExpected, result);
        
        scheduleExpected.Hours = [_hourDto1020];
        result = await _scheduleController.AddSchedule(scheduleExpected);
        VerifyObjectResult<CreatedResult>(scheduleExpected, result);
        
        result = await _scheduleController.GetScheduleForward("Station1", 1);
        scheduleExpected.Hours.Add(_hourDto1010);
        VerifyObjectResult<OkObjectResult>(scheduleExpected, result);
        
        scheduleExpected = new ScheduleDto{Hours = [_hourDto1010], LineNumber = 1, NameStation = "Station1", Orientation = "BACKWARD"};
        result = await _scheduleController.AddSchedule(scheduleExpected);
        VerifyObjectResult<CreatedResult>(scheduleExpected, result);
        
        result = await _scheduleController.GetScheduleBackward("Station1", 1);
        VerifyObjectResult<OkObjectResult>(scheduleExpected, result);
        
        result = await _scheduleController.GetScheduleForward("Station1", 1);
        scheduleExpected = new ScheduleDto{Hours = [_hourDto1010, _hourDto1020], LineNumber = 1, NameStation = "Station1", Orientation = "FORWARD"};
        VerifyObjectResult<OkObjectResult>(scheduleExpected, result);
        
        scheduleExpected = new ScheduleDto{Hours = [_hourDto1010], LineNumber = 2, NameStation = "Station1", Orientation = "FORWARD"};
        result = await _scheduleController.AddSchedule(scheduleExpected);
        VerifyObjectResult<CreatedResult>(scheduleExpected, result);
        
        scheduleExpected = new ScheduleDto{Hours = [_hourDto1010], LineNumber = 1, NameStation = "Station2", Orientation = "FORWARD"};
        result = await _scheduleController.AddSchedule(scheduleExpected);
        VerifyObjectResult<CreatedResult>(scheduleExpected, result);
        
        
    }
    
    [Fact]
    [Trait("Category", "Unit")]
    public async Task TestAddScheduleError()
    {
        IActionResult result = await _scheduleController.AddSchedule(
            new ScheduleDto{Hours = [], LineNumber = 1, NameStation = "Station1", Orientation = "FORWARD"});
        result.Should().BeOfType<BadRequestObjectResult>();
        
        result = await _scheduleController.AddSchedule(
            new ScheduleDto{Hours = [_hourDto1010], LineNumber = 0, NameStation = "Station1", Orientation = "FORWARD"});
        result.Should().BeOfType<BadRequestObjectResult>();
        
        result = await _scheduleController.AddSchedule(
            new ScheduleDto{Hours = [_hourDto1010], LineNumber = 1, NameStation = "", Orientation = "FORWARD"});
        result.Should().BeOfType<BadRequestObjectResult>();
        
        result = await _scheduleController.AddSchedule(
            new ScheduleDto{Hours = [_hourDto1010], LineNumber = 1, NameStation = "Station1", Orientation = "FORWAR"});
        result.Should().BeOfType<BadRequestObjectResult>();
        
        await _scheduleController.AddSchedule(
            new ScheduleDto{Hours = [_hourDto1010], LineNumber = 1, NameStation = "Station1", Orientation = "FORWARD"});
        result = await _scheduleController.AddSchedule(
            new ScheduleDto{Hours = [_hourDto1010], LineNumber = 1, NameStation = "Station1", Orientation = "FORWARD"});
        result.Should().BeOfType<BadRequestObjectResult>();
        
        result = await _scheduleController.AddSchedule(
            new ScheduleDto{Hours = [_hourDto1010], LineNumber = 1, NameStation = "Station1", Orientation = "FORWAR"});
        result.Should().BeOfType<BadRequestObjectResult>();
        
        result = await _scheduleController.AddSchedule(
            new ScheduleDto{Hours = [_hourDto1010], LineNumber = 1, NameStation = "Station3", Orientation = "FORWARD"});
        result.Should().BeOfType<NotFoundObjectResult>();
    }
    
    [Fact]
    [Trait("Category", "Unit")]
    public async Task TestFindSchedule()
    {
        await _scheduleController.AddSchedule(
            new ScheduleDto{Hours = [_hourDto1010], LineNumber = 1, NameStation = "Station1", Orientation = "FORWARD"});
        
        ScheduleDto scheduleExpected = new(){Hours = [_hourDto1010], LineNumber = 1, NameStation = "Station1", Orientation = "FORWARD"};
        IActionResult result = await _scheduleController.GetScheduleForward("Station1", 1);
        VerifyObjectResult<OkObjectResult>(scheduleExpected, result);
        
        await _scheduleController.AddSchedule(
            new ScheduleDto{Hours = [_hourDto1020], LineNumber = 1, NameStation = "Station1", Orientation = "FORWARD"});
        scheduleExpected = new ScheduleDto{Hours = [_hourDto1010, _hourDto1020], LineNumber = 1, NameStation = "Station1", Orientation = "FORWARD"};
        result = await _scheduleController.GetScheduleForward("Station1", 1);
        VerifyObjectResult<OkObjectResult>(scheduleExpected, result);
        
        scheduleExpected = new ScheduleDto{Hours = [_hourDto1010], LineNumber = 1, NameStation = "Station1", Orientation = "BACKWARD"};
        await _scheduleController.AddSchedule(scheduleExpected);
        result = await _scheduleController.GetScheduleBackward("Station1", 1);
        VerifyObjectResult<OkObjectResult>(scheduleExpected, result);
        
        result = await _scheduleController.GetScheduleForward("Station1", 1);
        scheduleExpected = new ScheduleDto{Hours = [_hourDto1010, _hourDto1020], LineNumber = 1, NameStation = "Station1", Orientation = "FORWARD"};
        VerifyObjectResult<OkObjectResult>(scheduleExpected, result);
        
        scheduleExpected = new ScheduleDto{Hours = [_hourDto1010], LineNumber = 2, NameStation = "Station1", Orientation = "FORWARD"};
        await _scheduleController.AddSchedule(scheduleExpected);
        result = await _scheduleController.GetScheduleForward("Station1", 2);
        VerifyObjectResult<OkObjectResult>(scheduleExpected, result);
        
        scheduleExpected = new ScheduleDto{Hours = [_hourDto1010], LineNumber = 1, NameStation = "Station2", Orientation = "FORWARD"};
        await _scheduleController.AddSchedule(scheduleExpected);
        result = await _scheduleController.GetScheduleForward("Station2", 1);
        VerifyObjectResult<OkObjectResult>(scheduleExpected, result);
    }
    
    [Fact]
    [Trait("Category", "Unit")]
    public async Task TestFindScheduleError()
    {
        IActionResult result = await _scheduleController.GetScheduleForward("", 1);
        result.Should().BeOfType<BadRequestObjectResult>();
        result = await _scheduleController.GetScheduleForward("Station1", 0);
        result.Should().BeOfType<BadRequestObjectResult>();
        
        await _scheduleController.AddSchedule(
            new ScheduleDto{Hours = [_hourDto1010], LineNumber = 1, NameStation = "Station1", Orientation = "FORWARD"});
        result = await _scheduleController.GetScheduleForward("Station1", 2);
        result.Should().BeOfType<NotFoundObjectResult>();
        
        result = await _scheduleController.GetScheduleForward("Station2", 1);
        result.Should().BeOfType<NotFoundObjectResult>();
        
        result = await _scheduleController.GetScheduleBackward("Station1", 1);
        result.Should().BeOfType<NotFoundObjectResult>();
    }
    
    private static void VerifyObjectResultList<T>(List<ScheduleDto> schedulesExpected, IActionResult result) where T : ObjectResult
    {
        result.Should().BeOfType<T>();
        T okObjectResult = (T) result;
        okObjectResult.Should().NotBeNull();
        okObjectResult.Value.Should().BeEquivalentTo(schedulesExpected);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task TestFindScheduleByNameOrientation()
    {
        await _scheduleController.AddSchedule(
            new ScheduleDto { Hours = [_hourDto1010, _hourDto1020], LineNumber = 1, NameStation = "Station1", Orientation = "FORWARD"});
        await _scheduleController.AddSchedule(
            new ScheduleDto { Hours = [_hourDto1010, _hourDto1020], LineNumber = 2, NameStation = "Station1", Orientation = "FORWARD"});

        List<ScheduleDto> schedulesExpected = [
            new ScheduleDto{ Hours = [_hourDto1010, _hourDto1020], LineNumber = 1, NameStation = "Station1", Orientation = "FORWARD" },
            new ScheduleDto{ Hours = [_hourDto1010, _hourDto1020], LineNumber = 2, NameStation = "Station1", Orientation = "FORWARD" }
        ];
        IActionResult result = await _scheduleController.GetScheduleByStationNameForward("Station1");
        VerifyObjectResultList<OkObjectResult>(schedulesExpected, result);
        
        result = await _scheduleController.GetScheduleByStationNameForward("Station2");
        VerifyObjectResultList<OkObjectResult>([], result);
        
        result = await _scheduleController.GetScheduleByStationNameBackward("Station1");
        VerifyObjectResultList<OkObjectResult>([], result);

        await _scheduleController.AddSchedule(
            new ScheduleDto { Hours = [_hourDto1010, _hourDto1020], NameStation = "Station2", LineNumber = 1, Orientation = "FORWARD" });
        await _scheduleController.AddSchedule(
            new ScheduleDto { Hours = [_hourDto1010, _hourDto1020], NameStation = "Station1", LineNumber = 1, Orientation = "BACKWARD" });
        
        schedulesExpected = [ new ScheduleDto{ Hours = [_hourDto1010, _hourDto1020], LineNumber = 1, NameStation = "Station2", Orientation = "FORWARD" } ];
        result = await _scheduleController.GetScheduleByStationNameForward("Station2");
        VerifyObjectResultList<OkObjectResult>(schedulesExpected, result);
        
        schedulesExpected = [ new ScheduleDto{ Hours = [_hourDto1010, _hourDto1020], LineNumber = 1, NameStation = "Station1", Orientation = "BACKWARD" } ];
        result = await _scheduleController.GetScheduleByStationNameBackward("Station1");
        VerifyObjectResultList<OkObjectResult>(schedulesExpected, result);
    }
    
    [Fact]
    [Trait("Category", "Unit")]
    public async Task TestFindScheduleByNameOrientationError()
    {
        IActionResult result = await _scheduleController.GetScheduleByStationNameForward("");
        result.Should().BeOfType<BadRequestObjectResult>();
        
        result = await _scheduleController.GetScheduleByStationNameBackward("");
        result.Should().BeOfType<BadRequestObjectResult>();
    }
}