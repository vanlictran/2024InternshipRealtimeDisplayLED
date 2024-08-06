namespace api_csharp_uplink.DB;

public class InfluxDbSettings
{
    public string Url { get; init; } = "";
    public string Token { get; init; } = "";
    public string OrgName { get; init; } = "";
    public string BucketName { get; init; } = "";
}