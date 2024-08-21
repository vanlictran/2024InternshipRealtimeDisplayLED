namespace api_csharp_uplink.Interface;

public interface ITimeProcessor
{
    public Task<int> GetTimeBetweenStations(int lineNumber, string nameStation1, string nameStation2);
    public Task<int> GetTimeToNextStation(string nameStation);
}