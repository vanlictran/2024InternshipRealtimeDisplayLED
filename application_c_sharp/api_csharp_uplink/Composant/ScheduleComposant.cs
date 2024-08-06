using api_csharp_uplink.DirException;
using api_csharp_uplink.Entities;
using api_csharp_uplink.Interface;

namespace api_csharp_uplink.Composant;

public class ScheduleComposant(IScheduleRepository scheduleRepository, IStationFinder stationFinder) : IScheduleRegistration, IScheduleFinder
{
    public async Task<Schedule> AddSchedule(string nameStation, int lineNumber, string orientation, List<DateTime> hours)
    {
        Orientation enumOrientation = (Orientation) Enum.Parse(typeof(Orientation), orientation, true);
        HashSet<DateTime> setHours = [];

        Station station = await stationFinder.GetStation(nameStation);
        
        try
        {
            Schedule scheduleFind = await FindSchedule(station.NameStation, lineNumber, enumOrientation);
            
            foreach (DateTime hour in hours.Where(hour => !scheduleFind.Hours.Contains(hour)))
                setHours.Add(hour);
        }
        catch (NotFoundException)
        {
            foreach (DateTime hour in hours)
                setHours.Add(hour);
        }
        
        Schedule schedule = new Schedule(station.NameStation, lineNumber, enumOrientation, setHours.ToList());
        return await scheduleRepository.AddSchedule(schedule);
    }
    
    public async Task<Schedule> FindSchedule(string nameStation, int lineNumber, Orientation orientation)
    {
        VerifyArguments(nameStation, lineNumber);
        
        return await scheduleRepository.FindSchedule(nameStation, lineNumber, orientation) ??
               throw new NotFoundException($"The schedule with NameStation {nameStation}, LineNumber {lineNumber}, " +
                   $"Orientation {orientation}");
    }

    public Task<List<Schedule>> FindScheduleByStationNameOrientation(string nameStation, Orientation orientation)
    {
        if (string.IsNullOrEmpty(nameStation))
            throw new ArgumentNullException(nameof(nameStation), "The name of the station is empty");
        
        return scheduleRepository.FindScheduleByStationNameOrientation(nameStation, orientation);
    }

    private static void VerifyArguments(string nameStation, int lineNumber)
    {
        if (string.IsNullOrEmpty(nameStation))
            throw new ArgumentNullException(nameof(nameStation), "The name of the station is empty");
        if (lineNumber < 1)
            throw new ArgumentOutOfRangeException(nameof(lineNumber), "The line number must be greater than 0");
    }
}
