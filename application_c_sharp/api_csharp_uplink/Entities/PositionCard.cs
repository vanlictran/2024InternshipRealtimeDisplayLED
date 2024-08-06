namespace api_csharp_uplink.Entities;

public class PositionCard(Position position, string devEuiCard)
{
    public Position Position { get; } = position;
    public string DevEuiCard { get; } = devEuiCard;
    
    public override bool Equals(object? obj)
    {
        if (obj == this)
            return true;
        if (obj == null || obj.GetType() != GetType())
            return false;
        PositionCard positionCard = (PositionCard)obj;
        return positionCard.Position.Equals(Position)  && positionCard.DevEuiCard.Equals(DevEuiCard);
    }
    
    public override int GetHashCode()
    {
        return HashCode.Combine(Position, DevEuiCard);
    }

    public override string ToString()
    {
        return $"Position: {Position}, DevEuiCard: {DevEuiCard}";
    }
}