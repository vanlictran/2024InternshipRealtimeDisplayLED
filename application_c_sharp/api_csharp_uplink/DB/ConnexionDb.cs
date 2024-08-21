using InfluxDB.Client.Core;

namespace api_csharp_uplink.DB;

[Measurement("itinerary")]
public class ConnexionDb
{
    [Column("lineNumber", IsTag = true)]
    public int lineNumber { get; set; }

    [Column("orientation", IsTag = true)]
    public string orientation { get; set; } = "";

    [Column("timeToNextStation", IsTag = true)]
    public int timeToNextStation { get; set; }

    [Column("distanceToNextStation", IsTag = true)]
    public double distanceToNextStation { get; set; }

    [Column("currentStation", IsTag = true)]
    public string currentStation { get; set; } = "";

    [Column("Value")] public string Value { get; set; } = "";

    [Column(IsTimestamp = true)]
    public DateTime Timestamp { get; set; }
}