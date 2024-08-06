using api_csharp_uplink.Composant;
using api_csharp_uplink.DirException;
using api_csharp_uplink.Entities;
using api_csharp_uplink.Interface;
using test_api_csharp_uplink.Unitaire.DBTest;

namespace test_api_csharp_uplink.Unitaire.Composant;

public class ScheduleComposantTest
{
    private readonly ScheduleComposant _scheduleComposant;
    private readonly DateTime _dateTime1010 = new(2024, 7, 15, 10, 10, 0, DateTimeKind.Utc);
    private readonly DateTime _dateTime1020 = new(2024, 7, 15, 10, 20, 0, DateTimeKind.Utc);
    
    public ScheduleComposantTest()
    {
        IScheduleRepository scheduleRepository = new DbTestSchedule();
        IStationRepository stationRepository = new DbTestStation();
        IStationFinder stationFinder = new StationComposant(stationRepository);
        
        stationRepository.Add(new Station(10.0, 10.0, "Station1"));
        stationRepository.Add(new Station(10.0, 10.0, "Station2"));
        
        _scheduleComposant = new ScheduleComposant(scheduleRepository, stationFinder);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task AddScheduleTest()
    {
        Schedule scheduleExpected = new Schedule("Station1", 1, Orientation.FORWARD, [_dateTime1010]);
        
        Schedule scheduleActual = await _scheduleComposant.AddSchedule("Station1", 1, "FORWARD", [_dateTime1010, _dateTime1010]);
        Assert.Equal(scheduleExpected, scheduleActual);
        
        scheduleActual = await _scheduleComposant.FindSchedule("Station1", 1, Orientation.FORWARD);
        Assert.Equal(scheduleExpected, scheduleActual);
        
        scheduleActual = await _scheduleComposant.AddSchedule("Station1", 1, "FORWARD", [_dateTime1010, _dateTime1020]);
        scheduleExpected = new Schedule("Station1", 1, Orientation.FORWARD, [_dateTime1020]);
        Assert.Equal(scheduleExpected, scheduleActual);
        
        scheduleActual = await _scheduleComposant.FindSchedule("Station1", 1, Orientation.FORWARD);
        scheduleExpected = new Schedule("Station1", 1, Orientation.FORWARD, [_dateTime1010, _dateTime1020]);
        Assert.Equal(scheduleExpected, scheduleActual);
        
        scheduleActual = await _scheduleComposant.AddSchedule("Station1", 1, "BACKWARD", [_dateTime1010]);
        scheduleExpected = new Schedule("Station1", 1, Orientation.BACKWARD, [_dateTime1010]);
        Assert.Equal(scheduleExpected, scheduleActual);
        
        scheduleActual = await _scheduleComposant.FindSchedule("Station1", 1, Orientation.BACKWARD);
        Assert.Equal(scheduleExpected, scheduleActual);
        
        scheduleActual = await _scheduleComposant.FindSchedule("Station1", 1, Orientation.FORWARD);
        scheduleExpected = new Schedule("Station1", 1, Orientation.FORWARD, [_dateTime1010, _dateTime1020]);
        Assert.Equal(scheduleExpected, scheduleActual);
        
        scheduleActual = await _scheduleComposant.AddSchedule("Station1", 2, "FORWARD", [_dateTime1010]);
        scheduleExpected = new Schedule("Station1", 2, Orientation.FORWARD, [_dateTime1010]);
        Assert.Equal(scheduleExpected, scheduleActual);
        
        scheduleActual =  await _scheduleComposant.AddSchedule("Station2", 1, "FORWARD", [_dateTime1010]);
        scheduleExpected = new Schedule("Station2", 1, Orientation.FORWARD, [_dateTime1010]);
        Assert.Equal(scheduleExpected, scheduleActual);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task AddErrorScheduleTest()
    {
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => _scheduleComposant.AddSchedule("Station1", 1, "FORWARD", []));
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => _scheduleComposant.AddSchedule("Station1", 0, "FORWARD", [_dateTime1010]));
        await Assert.ThrowsAsync<ArgumentNullException>(() => _scheduleComposant.AddSchedule("", 1, "FORWARD", [_dateTime1010]));
        await Assert.ThrowsAsync<ArgumentException>(() => _scheduleComposant.AddSchedule("Station1", 1, "FORWAR", [_dateTime1010]));
        await Assert.ThrowsAsync<NotFoundException>(() => _scheduleComposant.AddSchedule("Station3", 1, "FORWARD", [_dateTime1010]));
       
        await _scheduleComposant.AddSchedule("Station1", 1, "FORWARD", [_dateTime1010]);
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => _scheduleComposant.AddSchedule("Station1", 1, "FORWARD", [_dateTime1010]));
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task FindScheduleTest()
    {
        await _scheduleComposant.AddSchedule("Station1", 1, "FORWARD", [_dateTime1010]);
        
        Schedule scheduleExpected = new Schedule("Station1", 1, Orientation.FORWARD, [_dateTime1010]);
        Schedule scheduleActual = await _scheduleComposant.FindSchedule("Station1", 1, Orientation.FORWARD);
        Assert.Equal(scheduleExpected, scheduleActual);
        
        await _scheduleComposant.AddSchedule("Station1", 1, "FORWARD", [_dateTime1020]);
        scheduleActual = await _scheduleComposant.FindSchedule("Station1", 1, Orientation.FORWARD);
        scheduleExpected = new Schedule("Station1", 1, Orientation.FORWARD, [_dateTime1010, _dateTime1020]);
        Assert.Equal(scheduleExpected, scheduleActual);
        
        await _scheduleComposant.AddSchedule("Station1", 1, "BACKWARD", [_dateTime1010]);
        scheduleActual = await _scheduleComposant.FindSchedule("Station1", 1, Orientation.BACKWARD);
        scheduleExpected = new Schedule("Station1", 1, Orientation.BACKWARD, [_dateTime1010]);
        Assert.Equal(scheduleExpected, scheduleActual);
        
        scheduleActual = await _scheduleComposant.FindSchedule("Station1", 1, Orientation.FORWARD);
        scheduleExpected = new Schedule("Station1", 1, Orientation.FORWARD, [_dateTime1010, _dateTime1020]);
        Assert.Equal(scheduleExpected, scheduleActual);
        
        await _scheduleComposant.AddSchedule("Station1", 2, "FORWARD", [_dateTime1010]);
        scheduleActual = await _scheduleComposant.FindSchedule("Station1", 2, Orientation.FORWARD);
        scheduleExpected = new Schedule("Station1", 2, Orientation.FORWARD, [_dateTime1010]);
        Assert.Equal(scheduleExpected, scheduleActual);
        
        await _scheduleComposant.AddSchedule("Station2", 1, "FORWARD", [_dateTime1010]);
        scheduleActual = await _scheduleComposant.FindSchedule("Station2", 1, Orientation.FORWARD);
        scheduleExpected = new Schedule("Station2", 1, Orientation.FORWARD, [_dateTime1010]);
        Assert.Equal(scheduleExpected, scheduleActual);
    }
    
    [Fact]
    [Trait("Category", "Unit")]
    public async Task FindErrorScheduleTest()
    {
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => _scheduleComposant.FindSchedule("Station1", 0, Orientation.FORWARD));
        await Assert.ThrowsAsync<ArgumentNullException>(() => _scheduleComposant.FindSchedule("", 1, Orientation.FORWARD));
        
        await _scheduleComposant.AddSchedule("Station1", 1, "FORWARD", [_dateTime1010]);
        await Assert.ThrowsAsync<NotFoundException>(() => _scheduleComposant.FindSchedule("Station1", 1, Orientation.BACKWARD));
        await Assert.ThrowsAsync<NotFoundException>(() => _scheduleComposant.FindSchedule("Station1", 2, Orientation.FORWARD));
        await Assert.ThrowsAsync<NotFoundException>(() => _scheduleComposant.FindSchedule("Station2", 1, Orientation.FORWARD));
    }
    
    [Fact]
    [Trait("Category", "Unit")]
    public async Task FindScheduleByNameOrientationTest()
    {
        await _scheduleComposant.AddSchedule("Station1", 1, "FORWARD", [_dateTime1010, _dateTime1020]);
        await _scheduleComposant.AddSchedule("Station1", 2, "FORWARD", [_dateTime1010, _dateTime1020]);
        
        List<Schedule> schedulesExpected = [
            new Schedule("Station1", 1, Orientation.FORWARD, [_dateTime1010, _dateTime1020]),
            new Schedule("Station1", 2, Orientation.FORWARD, [_dateTime1010, _dateTime1020])
        ];
        List<Schedule> schedulesActual = await _scheduleComposant.FindScheduleByStationNameOrientation("Station1", Orientation.FORWARD);
        Assert.Equal(schedulesExpected, schedulesActual);
        
        schedulesActual = await _scheduleComposant.FindScheduleByStationNameOrientation("Station2", Orientation.FORWARD);
        Assert.Empty(schedulesActual);
        
        schedulesActual = await _scheduleComposant.FindScheduleByStationNameOrientation("Station1", Orientation.BACKWARD);
        Assert.Empty(schedulesActual);
        
        await _scheduleComposant.AddSchedule("Station2", 1, "FORWARD", [_dateTime1010, _dateTime1020]);
        await _scheduleComposant.AddSchedule("Station1", 1, "BACKWARD", [_dateTime1010, _dateTime1020]);
        
        schedulesExpected = [new Schedule("Station1", 1, Orientation.BACKWARD, [_dateTime1010, _dateTime1020])];
        schedulesActual = await _scheduleComposant.FindScheduleByStationNameOrientation("Station1", Orientation.BACKWARD);
        Assert.Equal(schedulesExpected, schedulesActual);
        
        schedulesExpected = [new Schedule("Station2", 1, Orientation.FORWARD, [_dateTime1010, _dateTime1020])];
        schedulesActual = await _scheduleComposant.FindScheduleByStationNameOrientation("Station2", Orientation.FORWARD);
        Assert.Equal(schedulesExpected, schedulesActual);
    }
    
    [Fact]
    [Trait("Category", "Unit")]
    public async Task FindErrorScheduleByNameOrientationTest()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(() => _scheduleComposant.FindScheduleByStationNameOrientation("", Orientation.FORWARD));
    }
}