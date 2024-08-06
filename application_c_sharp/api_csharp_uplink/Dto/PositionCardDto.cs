using System.ComponentModel.DataAnnotations;

namespace api_csharp_uplink.Dto;

public class PositionCardDto
{
    [Required(ErrorMessage = "Position is required")] 
    public PositionDto Position { get; init; } = new();
    
    [Required]
    [RegularExpression(@"\S+", ErrorMessage = "DevEuiNumber cannot be empty or whitespace.")]
    public string DevEuiNumber { get; init; } = "";
}