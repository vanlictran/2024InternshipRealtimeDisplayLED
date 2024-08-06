namespace api_csharp_uplink.Entities;

public class Schedule
{
    public string StationName { get; }
    public int LineNumber { get; }
    public Orientation Orientation { get; }
    public List<DateTime> Hours { get; }
    
    public Schedule(string stationName, int lineNumber, Orientation orientation, List<DateTime> hours)
    {
        if (string.IsNullOrEmpty(stationName))
            throw new ArgumentException("Station name cannot be null or empty.", nameof(stationName));
        if (lineNumber < 1)
            throw new ArgumentOutOfRangeException(nameof(lineNumber), "Line number must be greater than 0.");
        if (hours.Count == 0)
            throw new ArgumentOutOfRangeException(nameof(hours), "Hours list must not be empty.");
        
        StationName = stationName;
        LineNumber = lineNumber;
        Orientation = orientation;
        Hours = hours;
    }
    
    public override bool Equals(object? obj)
    {
        if (obj == null || GetType() != obj.GetType())
            return false;

        Schedule schedule = (Schedule) obj;
        return schedule.StationName == StationName && 
               schedule.LineNumber == LineNumber && 
               schedule.Orientation == Orientation &&
               new HashSet<DateTime>(Hours).SetEquals(schedule.Hours);
    }


    public override int GetHashCode()
    {
        int hashStationName = StationName.GetHashCode();
        int hashLineNumber = LineNumber.GetHashCode();
        int hashOrientation = Orientation.GetHashCode();
        int hashHours = Hours.GetHashCode();
        return hashStationName ^ hashLineNumber ^ hashOrientation ^ hashHours;
    }
    
    public override string ToString()
    {
        return $"Station Name: {StationName}, Line Number: {LineNumber}, Orientation: {Orientation}, Hours: {Hours}";
    }
}