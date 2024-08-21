using System.ComponentModel.DataAnnotations;

namespace api_csharp_uplink.Dto;

public class ConnexionDto
{
    [Required]
    [RegularExpression(@"\S+", ErrorMessage = "CurrentNameStation cannot be empty or whitespace.")]
    public string CurrentNameStation { get; set; }
    
    [Required]
    [RegularExpression(@"\S+", ErrorMessage = "NextNameStation cannot be empty or whitespace.")]
    public string NextNameStation { get; set; } = "";
}