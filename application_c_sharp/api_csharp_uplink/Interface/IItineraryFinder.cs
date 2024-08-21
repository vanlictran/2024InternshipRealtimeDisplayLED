using api_csharp_uplink.Entities;

namespace api_csharp_uplink.Interface;

public interface IItineraryFinder
{
    public Task<Itinerary> FindItinerary(int lineNumber, string orientation);
    public Task<Itinerary> FindItineraryBetweenStation(int lineNumber, string orientation, string station1,
        string station2);
}