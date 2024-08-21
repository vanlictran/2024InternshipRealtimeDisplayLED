namespace api_csharp_uplink.Entities;

public class CardNearStation (Position position, Orientation orientation=Orientation.FORWARD, int time=-1, double distance=-1.0)
{
    public Position Position { get; set; } = position;
    public Orientation Orientation { get; set; } = orientation;
    public int Time { get; set; } = time;
    public double Distance { get; set; } = distance;
}