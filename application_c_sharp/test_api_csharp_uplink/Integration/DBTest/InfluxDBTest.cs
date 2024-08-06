using InfluxDB.Client;

namespace test_api_csharp_uplink.Integration.DBTest;

public class InfluxDbTest
{
    private readonly string _organizationId;
    private readonly DeleteApi _deleteApi;

    public InfluxDbTest()
    {
        var client = new InfluxDBClient("http://influxdb:8086", "mNxnpUdxk7h6z8GOchqIL7AM8au7Zt3y9uXX_jz9OXhEdi0qnOkLc3ZjWqW5rSc-ASVLafSF0xk_-IIWxir78A==");
        _organizationId = "7676f3c1acc9cda6";
        _deleteApi = client.GetDeleteApi();
    }


    public async Task InitializeBucket()
    {
        await _deleteApi.Delete(DateTime.UnixEpoch, DateTime.UtcNow, "", "mybucket", _organizationId);
    }
}
