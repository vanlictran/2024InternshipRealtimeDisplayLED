using System.ComponentModel.DataAnnotations;

namespace api_csharp_uplink.Dto;

public class HourDto
{
    [Required]
    [Range(0, 23, ErrorMessage = "Hour must be between 0 and 59")]
    public int Hour { get; init; }
    
    [Required]
    [Range(0, 59, ErrorMessage = "Minute must be between 0 and 59")]
    public int Minute { get; init; }
}