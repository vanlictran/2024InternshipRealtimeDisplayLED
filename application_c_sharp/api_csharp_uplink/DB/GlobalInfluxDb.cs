using api_csharp_uplink.DirException;
using InfluxDB.Client;
using InfluxDB.Client.Api.Domain;
using Microsoft.Extensions.Options;

namespace api_csharp_uplink.DB;

public class GlobalInfluxDb(IOptions<InfluxDbSettings> influxDbSettings) : IGlobalInfluxDb
{
    private readonly InfluxDBClient _client = new(influxDbSettings.Value.Url, influxDbSettings.Value.Token);
    private readonly string _bucketName = influxDbSettings.Value.BucketName;
    private readonly string _orgName = influxDbSettings.Value.OrgName;
    private const string BaseQuery = "from(bucket: \"mybucket\")\n  |> range(start: 0)\n";
    private const string MessageErrorName = "Error writing to InfluxDB cloud: ";
    
    public async Task<T> Save<T>(T data)
    {
        try
        {
            await _client.GetWriteApiAsync().WriteMeasurementAsync(data, WritePrecision.Ms, _bucketName, _orgName);
            return data;
        }
        catch (Exception e)
        {
            throw new DbException(MessageErrorName + e.Message);
        }
    }

    public async Task<List<T>> SaveAll<T>(List<T> data)
    {
        try
        {
            await _client.GetWriteApiAsync().WriteMeasurementsAsync(data, WritePrecision.Ms, _bucketName, _orgName);
            return data;
        }
        catch (Exception e)
        {
            throw new DbException(MessageErrorName + e.Message);
        }
    }

    public async Task<List<T>> GetAll<T>(string measurementName)
    {
        try
        {
            string query = BaseQuery + $"|> filter(fn: (r) => r._measurement == \"{measurementName}\")";
            return await _client.GetQueryApi().QueryAsync<T>(query, _orgName);
        }
        catch (Exception e)
        {
            throw new DbException(MessageErrorName + e.Message);
        }
    }
    
    public async Task<List<T>> Get<T>(string query)
    {
        try
        {
            return await _client.GetQueryApi().QueryAsync<T>(query, _orgName);
        }
        catch (Exception e)
        {
            throw new DbException(MessageErrorName + e.Message);
        }
    }

    public Task<List<T>> Get<T>(string measurementName, string predicate)
    {
        return Get<T>(BaseQuery + $"|> filter(fn: (r) => r._measurement == \"{measurementName}\")\n" + predicate);
    }

    public Task Delete(string predicate)
    {
        return Delete(predicate, DateTime.UnixEpoch, DateTime.Now);
    }

    public async Task Delete(string predicate, DateTime start, DateTime end)
    {
        try
        {
            await _client.GetDeleteApi().Delete(start, end, predicate, _bucketName, _orgName);
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            throw new DbException(MessageErrorName + e.Message);
        }
    }
}
