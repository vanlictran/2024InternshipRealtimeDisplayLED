namespace api_csharp_uplink.Entities;

public class ConnexionWithTime
{
    public int lineNumber { get; }
    public Orientation orientation { get; }
    public string stationCurrent { get; }
    public string stationNext { get; }
    public int timeToNextStation { get; set; }
    public double distanceToNextStation { get; set; }

    public ConnexionWithTime(int lineNumber, string orientation, string stationCurrent, string stationNext,
        int timeToNextStation, double distanceToNextStation)
    {
        if (lineNumber < 0)
            throw new ArgumentOutOfRangeException(nameof(lineNumber), "Line number cannot be negative.");
        if (timeToNextStation < 0)
            throw new ArgumentOutOfRangeException(nameof(timeToNextStation),
                "Time to next station cannot be negative.");
        if (distanceToNextStation < 0)
            throw new ArgumentOutOfRangeException(nameof(distanceToNextStation),
                "Distance to next station cannot be negative.");

        this.lineNumber = lineNumber;
        this.orientation = Enum.Parse<Orientation>(orientation);
        this.stationCurrent = stationCurrent;
        this.stationNext = stationNext;
        this.timeToNextStation = timeToNextStation;
        this.distanceToNextStation = distanceToNextStation;
    }

    public override string ToString()
    {
        return $"Connexion {stationCurrent} and {stationNext} " +
               $"with time to next station: {timeToNextStation} and distance to next station: {distanceToNextStation}";
    }

    public override bool Equals(object? obj)
    {
        if (obj == null || GetType() != obj.GetType())
            return false;
        ConnexionWithTime connexion = (ConnexionWithTime) obj;
        return stationCurrent == connexion.stationCurrent && lineNumber == connexion.lineNumber &&
               orientation == connexion.orientation && stationNext == connexion.stationNext;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(lineNumber, orientation, stationCurrent, stationNext);
    }
}