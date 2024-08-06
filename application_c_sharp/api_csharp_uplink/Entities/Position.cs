namespace api_csharp_uplink.Entities;

public class Position
{
    public double Latitude { get; }
    
    public double Longitude { get; }

    public Position(double latitude, double longitude)
    {
        if (latitude is > 90.0 or < -90.0)
            throw new ArgumentOutOfRangeException(nameof(latitude), "Latitude must be between -90 and 90 degrees.");
        if (longitude is > 180.0 or < -180.0)
            throw new ArgumentOutOfRangeException(nameof(longitude), "Longitude must be between -180 and 180 degrees.");
        
        Latitude = latitude;
        Longitude = longitude;
    }
    public override bool Equals(object? obj)
    {
        if (obj == this)
            return true;
        if (obj == null || obj.GetType() != GetType())
            return false;
        Position position = (Position) obj;
        return Math.Abs(position.Latitude - Latitude) < 0.001 && Math.Abs(position.Longitude - Longitude) < 0.001;
    }
    
    public override int GetHashCode()
    {
        return HashCode.Combine(Latitude, Longitude);
    }

    public override string ToString()
    {
        return $"Latitude: {Latitude}, Longitude: {Longitude}";
    }
}