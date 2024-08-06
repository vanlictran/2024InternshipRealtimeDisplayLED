using System.ComponentModel.DataAnnotations;

namespace api_csharp_uplink.Dto;

public class PositionDto
{
    [Range(-90.0, 90.0)]
    public double Latitude { get; set; }
    
    [Range(-180.0, 180.0)]
    public double Longitude { get; set; }
}