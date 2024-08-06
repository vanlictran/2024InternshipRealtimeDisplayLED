namespace api_csharp_uplink.Entities;
public class Card(string devEuiCard, int lineBus)
{
    public string DevEuiCard { get; } = devEuiCard;
    public int LineBus { get; } = lineBus;

    public override string ToString()
    {
        return $"LineBus: {LineBus}, DevEuiCard: {DevEuiCard}";
    }


    public override bool Equals(object? obj)
    {
        if (obj == null || GetType() != obj.GetType())
            return false;

        var bus = (Card)obj;
        return LineBus == bus.LineBus &&
               DevEuiCard == bus.DevEuiCard;
    }


    public override int GetHashCode()
    {
        int hashLineBus = LineBus.GetHashCode();
        int hashDevEuiCard = DevEuiCard.GetHashCode();

        return hashLineBus ^ hashDevEuiCard;
    }
}
