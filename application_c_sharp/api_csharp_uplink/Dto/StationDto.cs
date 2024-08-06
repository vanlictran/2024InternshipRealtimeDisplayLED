using System.ComponentModel.DataAnnotations;

namespace api_csharp_uplink.Dto;

public class StationDto
{
    [Required(ErrorMessage = "Position is required")] 
    public PositionDto Position { get; init; } = new();
    
    [Required]
    [RegularExpression(@"\S+", ErrorMessage = "NameStation cannot be empty or whitespace.")]
    public string NameStation { get; init; } = "";
}