using api_csharp_uplink.Entities;

namespace api_csharp_uplink.Interface;

public interface IGraphItinerary
{
    public Task RegisterItineraryCard(Itinerary itinerary);
    public Task<int> GetItineraryTime(int lineNumber, string nameStation1, string nameStation2);
    public Task RemoveItineraryCard(int lineNumber, Orientation orientation);
    public Task<Station> AddStationGraph(Station station);
    public Task<int> GetTimeStation(string nameStation);
}