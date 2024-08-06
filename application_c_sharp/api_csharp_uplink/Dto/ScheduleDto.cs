using System.ComponentModel.DataAnnotations;

namespace api_csharp_uplink.Dto;

public class ScheduleDto
{
    [Required]
    [RegularExpression(@"\S+", ErrorMessage = "NameStation cannot be empty or whitespace.")]
    public string NameStation { get; init; } = "";
    
    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "LineNumber must be greater than 0")]
    public int LineNumber { get; init; }
    
    [Required]
    [RegularExpression("FORWARD|BACKWARD", ErrorMessage = "Orientation must be FORWARD or BACKWARD")]
    public string Orientation { get; init; } = "";
    
    [Required]
    [MinLength(1, ErrorMessage = "Hours list must not be empty.")]
    public List<HourDto> Hours { get; set; } = [];
}
