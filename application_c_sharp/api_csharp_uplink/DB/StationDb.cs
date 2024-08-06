using InfluxDB.Client.Core;

namespace api_csharp_uplink.DB;

[Measurement("station")]
public class StationDb
{
    [Column("nameStation", IsTag = true)] 
    public string NameStation { get; init; } = "RandomStation";
    
    [Column("longitude", IsTag = true)]
    public double Longitude { get; init;  }
    
    [Column("latitude", IsTag = true)]
    public double Latitude { get; init;  }

    [Column("Value")] 
    public string Value { get; init; } = "";
    
    [Column(IsTimestamp = true)]
    public DateTime Timestamp { get; set; }
}