using api_csharp_uplink.Dto;
using api_csharp_uplink.Entities;

namespace api_csharp_uplink.Interface;

public interface IItineraryRegister
{
    public Task<Itinerary> AddItinerary(int lineNumber, string orientation, List<ConnexionDto> connexions);
    public Task<Station> AddOneStation(string nameStation);
    public Task<bool> DeleteItinerary(int lineNumber, string orientation);
}