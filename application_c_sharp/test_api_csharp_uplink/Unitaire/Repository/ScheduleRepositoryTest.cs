using api_csharp_uplink.DB;
using api_csharp_uplink.Entities;
using api_csharp_uplink.Repository;
using Moq;

namespace test_api_csharp_uplink.Unitaire.Repository;

public class ScheduleRepositoryTest
{
    private const string MeasurementSchedule = "schedule";
    private static readonly DateTime DateTime1010 = new(2024, 7, 15, 10, 10, 0, DateTimeKind.Utc);
    private static readonly DateTime DateTime1020 = new(2024, 7, 15, 10, 20, 0, DateTimeKind.Utc);
    private static readonly ScheduleDb ScheduleDb1010F1 = new() {Hours = DateTime1010, LineNumber = 1, Orientation = "FORWARD", StationName = "Station1"};
    private static readonly ScheduleDb ScheduleDb1010B1 = new(){Hours = DateTime1010, LineNumber = 1, Orientation = "BACKWARD", StationName = "Station1"};
    private static readonly ScheduleDb ScheduleDb1010F2 = new(){Hours = DateTime1010, LineNumber = 2, Orientation = "FORWARD", StationName = "Station1"};
    private static readonly ScheduleDb ScheduleDb1010F1Station2 = new(){Hours = DateTime1010, LineNumber = 1, Orientation = "FORWARD", StationName = "Station2"};
    private static readonly Schedule Schedule1010F1 = new("Station1", 1, Orientation.FORWARD, [DateTime1010]);
    private static readonly Schedule Schedule1010B1 = new("Station1", 1, Orientation.BACKWARD, [DateTime1010]);
    private static readonly Schedule Schedule1010F2 = new("Station1", 2, Orientation.FORWARD, [DateTime1010]);
    private static readonly Schedule Schedule1010F1Station2 = new("Station2", 1, Orientation.FORWARD, [DateTime1010]);
    
    [Fact]
    [Trait("Category", "Unit")]
    public async Task TestAddSchedule()
    {
        ScheduleDb scheduleDb1020 = new ScheduleDb{Hours = DateTime1020, LineNumber = 1, Orientation = "FORWARD", StationName = "Station1"};
        Schedule scheduleExpected = new Schedule("Station1", 1, Orientation.FORWARD, [DateTime1010, DateTime1020]);
        
        Mock<IGlobalInfluxDb> mock = new();
        mock.SetupSequence(globalInfluxDb => globalInfluxDb.Save(It.IsAny<ScheduleDb>()))
            .ReturnsAsync(ScheduleDb1010F1)
            .ReturnsAsync(scheduleDb1020);
        ScheduleRepository scheduleRepository = new(mock.Object);
        
        Schedule result = await scheduleRepository.AddSchedule(scheduleExpected);
        Assert.Equal(scheduleExpected, result);
    }
    
    [Fact]
    [Trait("Category", "Unit")]
    public async Task TestFindSchedule()
    {
        Mock<IGlobalInfluxDb> mock = new();
        mock.SetupSequence(globalInfluxDb => globalInfluxDb.Get<ScheduleDb>(MeasurementSchedule,
                "|> filter(fn: (r) => r.stationName == \"Station1\" and r.lineNumber == \"1\" and r.orientation == \"FORWARD\")"))
            .ReturnsAsync([])
            .ReturnsAsync([ScheduleDb1010B1]);
        ScheduleRepository scheduleRepository = new(mock.Object);
        
        Schedule? result = await scheduleRepository.FindSchedule("Station1", 1, Orientation.FORWARD);
        Assert.Null(result);
        
        result = await scheduleRepository.FindSchedule("Station1", 1, Orientation.FORWARD);
        Assert.Equal(Schedule1010F1, result);

        mock.Setup(globalInfluxDb => globalInfluxDb.Get<ScheduleDb>(MeasurementSchedule,
                "|> filter(fn: (r) => r.stationName == \"Station1\" and r.lineNumber == \"1\" and r.orientation == \"BACKWARD\")"))
            .ReturnsAsync([ScheduleDb1010B1]);
        result = await scheduleRepository.FindSchedule("Station1", 1, Orientation.BACKWARD);
        Assert.Equal(Schedule1010B1, result);
        
        mock.Setup(globalInfluxDb => globalInfluxDb.Get<ScheduleDb>(MeasurementSchedule,
                "|> filter(fn: (r) => r.stationName == \"Station1\" and r.lineNumber == \"2\" and r.orientation == \"FORWARD\")"))
            .ReturnsAsync([ScheduleDb1010F2]);
        result = await scheduleRepository.FindSchedule("Station1", 2, Orientation.FORWARD);
        Assert.Equal(Schedule1010F2, result);
        
        mock.Setup(globalInfluxDb => globalInfluxDb.Get<ScheduleDb>(MeasurementSchedule,
                "|> filter(fn: (r) => r.stationName == \"Station2\" and r.lineNumber == \"1\" and r.orientation == \"FORWARD\")"))
            .ReturnsAsync([ScheduleDb1010F1Station2]);
        result = await scheduleRepository.FindSchedule("Station2", 1, Orientation.FORWARD);
        Assert.Equal(Schedule1010F1Station2, result);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task TestFindScheduleByStationNameOrientation()
    {
        Mock<IGlobalInfluxDb> mock = new();
        mock.Setup(globalInfluxDb => globalInfluxDb.Get<ScheduleDb>(MeasurementSchedule,
                "|> filter(fn: (r) => r.stationName == \"Station1\" and r.orientation == \"FORWARD\")"))
            .ReturnsAsync([ScheduleDb1010F1, ScheduleDb1010F2]);
        ScheduleRepository scheduleRepository = new(mock.Object);
        
        List<Schedule> result = await scheduleRepository.FindScheduleByStationNameOrientation("Station1", Orientation.FORWARD);
        Assert.Equal([Schedule1010F1, Schedule1010F2], result);

        mock.Setup(globalInfluxDb => globalInfluxDb.Get<ScheduleDb>(MeasurementSchedule,
                "|> filter(fn: (r) => r.stationName == \"Station1\" and r.orientation == \"BACKWARD\")"))
            .ReturnsAsync([ScheduleDb1010B1]);
        
        result = await scheduleRepository.FindScheduleByStationNameOrientation("Station1", Orientation.BACKWARD);
        Assert.Equal([Schedule1010B1], result);
        
        mock.Setup(globalInfluxDb => globalInfluxDb.Get<ScheduleDb>(MeasurementSchedule,
                "|> filter(fn: (r) => r.stationName == \"Station2\" and r.orientation == \"FORWARD\")"))
            .ReturnsAsync([ScheduleDb1010F1Station2]);
        
        result = await scheduleRepository.FindScheduleByStationNameOrientation("Station2", Orientation.FORWARD);
        Assert.Equal([Schedule1010F1Station2], result);
    }
}