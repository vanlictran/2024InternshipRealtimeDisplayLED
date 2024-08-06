namespace api_csharp_uplink.Entities;

public class Station {
    public Position Position { get; }
    
    public string NameStation { get; }
    
    public Station(double latitude, double longitude, string nameStation): 
        this(new Position(latitude, longitude), nameStation) { }
    
    public Station(Position position, string nameStation)
    {
        if (string.IsNullOrEmpty(nameStation))
            throw new ArgumentNullException(nameof(nameStation), "Name of station must not be null or empty.");
        
        Position = position;
        NameStation = nameStation;
    }
    
    public override bool Equals(object? obj)
    {
        if (obj == this)
            return true;
        if (obj == null || obj.GetType() != GetType())
            return false;
        Station station = (Station)obj;
        return station.Position.Equals(Position)  && station.NameStation.Equals(NameStation);
    }
    
    public override int GetHashCode()
    {
        return HashCode.Combine(Position, NameStation);
    }

    public override string ToString()
    {
        return $"Position: {Position}, NameStation: {NameStation}";
    }
}