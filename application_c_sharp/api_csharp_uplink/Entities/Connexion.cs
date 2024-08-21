namespace api_csharp_uplink.Entities;

public class Connexion
{
    public int lineNumber { get; }
    public Orientation orientation { get; }
    public Station stationCurrent { get; }
    public int timeToNextStation { get; set; }
    public double distanceToNextStation { get; set; }
    
    public Connexion(int lineNumber, string orientation, Station stationCurrent, int timeToNextStation, double distanceToNextStation)
    {
        if (lineNumber < 0)
            throw new ArgumentOutOfRangeException(nameof(lineNumber), "Line number cannot be negative.");
        if (timeToNextStation < 0)
            throw new ArgumentOutOfRangeException(nameof(timeToNextStation), "Time to next station cannot be negative.");
        if (distanceToNextStation < 0)
            throw new ArgumentOutOfRangeException(nameof(distanceToNextStation), "Distance to next station cannot be negative.");
        
        this.lineNumber = lineNumber;
        this.orientation = Enum.Parse<Orientation>(orientation);
        this.stationCurrent = stationCurrent;
        this.timeToNextStation = timeToNextStation;
        this.distanceToNextStation = distanceToNextStation;
    }
    
    public override string ToString()
    {
        return $"Connexion {stationCurrent.NameStation} " +
               $"with time to next station: {timeToNextStation} and distance to next station: {distanceToNextStation}";
    }
    
    public override bool Equals(object? obj)
    {
        if (obj == null || GetType() != obj.GetType())
            return false;
        Connexion connexion = (Connexion) obj;
        return stationCurrent.Equals(connexion.stationCurrent) && lineNumber == connexion.lineNumber &&
               orientation == connexion.orientation;
    }
    
    public override int GetHashCode()
    {
        return HashCode.Combine(lineNumber, orientation, stationCurrent);
    }
}