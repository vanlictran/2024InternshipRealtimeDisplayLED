using api_csharp_uplink.Entities;

namespace api_csharp_uplink.Repository.Interface;

public interface IItineraryRepository
{
    public Task<Itinerary> AddItinerary(Itinerary itinerary);
    public Task<Itinerary?> FindItinerary(int lineNumber, string orientation);
    public Task<Itinerary?> FindItineraryBetweenStation(int lineNumber, string orientation, string station1,
        string station2);
    public Task<bool> DeleteItinerary(int lineNumber, string orientation);
}