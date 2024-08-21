using System.ComponentModel.DataAnnotations;

namespace api_csharp_uplink.Dto;

public class ItineraryDto
{
    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "LineNumber must be greater than 0")]
    public int LineNumber { get; set; }

    [Required]
    [RegularExpression("FORWARD|BACKWARD", ErrorMessage = "Orientation must be FORWARD or BACKWARD")]
    public string Orientation { get; set; } = "";

    [Required]
    [MinLength(1, ErrorMessage = "Connexions list must not be empty.")]
    public List<ConnexionDto> Connexions { get; set; } = [];
}