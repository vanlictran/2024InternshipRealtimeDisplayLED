namespace api_csharp_uplink.Connectors.ExternalEntities;

public class TimeDistance(int time, double distance)
{
    public int Time { get; init; } = time;
    public double Distance { get; init; } = distance;
}