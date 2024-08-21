namespace api_csharp_uplink.DB;

public interface IGlobalInfluxDb
{
    public Task<T> Save<T>(T data);
    
    public Task<List<T>> SaveAll<T>(List<T> data);

    public Task<List<T>> GetAll<T>(string measurementName);

    public Task<List<T>> Get<T>(string query);

    public Task<List<T>> Get<T>(string measurementName, string predicate);

    public Task Delete(string predicate);

    public Task Delete(string predicate, DateTime start, DateTime end);
}