using api_csharp_uplink.DB;
using api_csharp_uplink.Entities;
using api_csharp_uplink.Interface;

namespace api_csharp_uplink.Repository;

public class ScheduleRepository(IGlobalInfluxDb globalInfluxDb) : IScheduleRepository
{
    private const string MeasurementName = "schedule";
    
    public async Task<Schedule> AddSchedule(Schedule schedule)
    {
        string nameOrientation = schedule.Orientation.ToString();
        List<Task<ScheduleDb>> schedules = schedule.Hours.AsParallel()
            .Select(hour => 
                globalInfluxDb.Save(ConvertScheduleToScheduleDb(schedule.StationName, schedule.LineNumber, nameOrientation, hour)))
            .ToList();

        ScheduleDb[] scheduleDbs = await Task.WhenAll(schedules);
        List<DateTime> hours = scheduleDbs.AsParallel().Select(scheduleDb => scheduleDb.Hours).ToList();
        return new Schedule(schedule.StationName, schedule.LineNumber, schedule.Orientation, hours);
    }

    public async Task<Schedule?> FindSchedule(string nameStation, int lineNumber, Orientation orientation)
    {
        string predicate = $"|> filter(fn: (r) => r.stationName == \"{nameStation}\" and r.lineNumber == \"{lineNumber}\" and r.orientation == \"{orientation}\")";
        List<ScheduleDb> list = await globalInfluxDb.Get<ScheduleDb>(MeasurementName, predicate);
        
        if (list.Count == 0)
            return null;
        
        List<DateTime> hours = list.AsParallel().Select(scheduleDb => scheduleDb.Hours).ToList();
        return new Schedule(nameStation, lineNumber, orientation, hours);
        
    }

    public async Task<List<Schedule>> FindScheduleByStationNameOrientation(string nameStation, Orientation orientation)
    {
        string predicate = $"|> filter(fn: (r) => r.stationName == \"{nameStation}\" and r.orientation == \"{orientation}\")";
        List<ScheduleDb> scheduleDbs = await globalInfluxDb.Get<ScheduleDb>(MeasurementName, predicate);

        List<Schedule> schedules = scheduleDbs.AsParallel()
            .GroupBy(scheduleDb => new FindScheduleMap(scheduleDb.StationName, scheduleDb.LineNumber, orientation))
            .Select(findScheduleMap =>
            {
                List<DateTime> hours = findScheduleMap.AsParallel().Select(scheduleDb => scheduleDb.Hours)
                    .ToList();
                hours.Sort((hour1, hour2) => 
                    hour1.Hour == hour2.Hour? hour1.Minute.CompareTo(hour2.Minute) : hour1.Hour.CompareTo(hour2.Hour));
                
                FindScheduleMap key = findScheduleMap.Key;
                return new Schedule(key.nameStation, key.lineNumber, key.orientation, hours);
            }).ToList();
        
        schedules.Sort((schedule1, schedule2) => schedule1.LineNumber.CompareTo(schedule2.LineNumber));
        return schedules;
    }

    private static ScheduleDb ConvertScheduleToScheduleDb(string nameStation, int lineNumber, string orientation, DateTime hour)
    {
        return new ScheduleDb
        {
            StationName = nameStation,
            LineNumber = lineNumber,
            Orientation = orientation,
            Hours = hour
        };
    }
}

public record FindScheduleMap(string nameStation, int lineNumber, Orientation orientation);