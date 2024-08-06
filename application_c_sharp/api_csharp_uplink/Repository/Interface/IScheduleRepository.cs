using api_csharp_uplink.Entities;

namespace api_csharp_uplink.Interface;

public interface IScheduleRepository
{
    Task<Schedule> AddSchedule(Schedule schedule);
    Task<Schedule?> FindSchedule(string nameStation, int lineNumber, Orientation orientation);
    Task<List<Schedule>> FindScheduleByStationNameOrientation(string nameStation, Orientation orientation);
}