namespace api_csharp_uplink.Entities;

public class Itinerary
{
    public int lineNumber { get; }
    public Orientation orientation { get; }
    public List<Connexion> connexions { get; }

    public Itinerary(int lineNumber, string orientation, List<Connexion> connexions)
    {
        if (lineNumber < 0)
            throw new ArgumentOutOfRangeException(nameof(lineNumber), "Line number must be positive");
        if (connexions.Count < 2)
            throw new ArgumentOutOfRangeException(nameof(connexions), "Itinerary must have at least 2 connexions");
        
        this.lineNumber = lineNumber;
        this.orientation = Enum.Parse<Orientation>(orientation);
        this.connexions = connexions;
    }

    public override string ToString()
    {
        return $"Itinerary on line {lineNumber} with orientation {orientation} and {connexions.Count} connexions.";
    }

    public override bool Equals(object? obj)
    {
        if (obj == null || GetType() != obj.GetType())
            return false;

        Itinerary itinerary = (Itinerary)obj;
        return lineNumber == itinerary.lineNumber &&
               orientation == itinerary.orientation;
    }

    public override int GetHashCode()
    {
        return lineNumber.GetHashCode() ^ orientation.GetHashCode();
    }
}