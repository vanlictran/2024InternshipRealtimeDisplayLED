using System.ComponentModel.DataAnnotations;

namespace api_csharp_uplink.Dto;

public class ConnexionDtoWithTime
{
    [Required]
    [Range(0, int.MaxValue, ErrorMessage = "LineNumber cannot be negative.")]
    public int LineNumber { get; set; }
    
    [Required]
    [RegularExpression(@"\S+", ErrorMessage = "Orientation cannot be empty or whitespace.")]
    public string Orientation { get; set; }
    
    [Required]
    [RegularExpression(@"\S+", ErrorMessage = "CurrentNameStation cannot be empty or whitespace.")]
    public string CurrentNameStation { get; set; }
    
    [Required]
    [RegularExpression(@"\S+", ErrorMessage = "NextNameStation cannot be empty or whitespace.")]
    public string NextNameStation { get; set; } = "";
    
    [Required]
    [Range(0, int.MaxValue, ErrorMessage = "Time to next station cannot be negative.")]
    public int TimeToNextStation { get; set; }
    
    [Required]
    [Range(0, double.MaxValue, ErrorMessage = "Distance to next station cannot be negative.")]
    public double DistanceToNextStation { get; set; }
}