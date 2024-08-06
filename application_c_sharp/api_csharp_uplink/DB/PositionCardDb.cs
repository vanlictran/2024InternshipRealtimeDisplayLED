using InfluxDB.Client.Core;

namespace api_csharp_uplink.DB;

[Measurement("positionCard")]
public class PositionCardDb
{
    [Column("devEuiCard", IsTag = true)] 
    public string DevEuiCard { get; init; } = "0";
    
    [Column("longitude", IsTag = true)]
    public double Longitude { get; init;  }
    
    [Column("latitude", IsTag = true)]
    public double Latitude { get; init;  }

    [Column("Value")] 
    public string Value { get; set; } = "";
    
    [Column(IsTimestamp = true)]
    public DateTime Timestamp { get; set; }
}