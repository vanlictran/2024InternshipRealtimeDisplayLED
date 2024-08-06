using api_csharp_uplink.Entities;
using api_csharp_uplink.Interface;

namespace test_api_csharp_uplink.Unitaire.DBTest;

public class DbTestSchedule : IScheduleRepository
{
    private readonly Dictionary<FindScheduleMap, Schedule> _dico = [];
    
    public Task<Schedule> AddSchedule(Schedule schedule)
    {
        FindScheduleMap findScheduleMap =
            new FindScheduleMap(schedule.StationName, schedule.LineNumber, schedule.Orientation);

        if (_dico.TryGetValue(findScheduleMap, out Schedule? scheduleDico))
        {
            scheduleDico.Hours.AddRange(schedule.Hours);
            return Task.FromResult(schedule);
        }
        
        _dico.Add(findScheduleMap, schedule);
        return Task.FromResult(schedule);
    }

    public Task<Schedule?> FindSchedule(string nameStation, int lineNumber, Orientation orientation)
    {
        _dico.TryGetValue(new FindScheduleMap(nameStation, lineNumber, orientation), out Schedule? schedule);
        return Task.FromResult(schedule);
    }

    public Task<List<Schedule>> FindScheduleByStationNameOrientation(string nameStation, Orientation orientation)
    {
        return Task.FromResult(_dico.Keys
            .Where(findScheduleMap => findScheduleMap.nameStation.Equals(nameStation) && findScheduleMap.orientation == orientation)
            .Select(map => _dico[map]).ToList());
    }
    
    private record FindScheduleMap(string nameStation, int lineNumber, Orientation orientation);
}