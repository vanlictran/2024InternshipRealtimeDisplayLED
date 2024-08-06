using api_csharp_uplink.Entities;

namespace api_csharp_uplink.Interface;

public interface IPositionRegister
{
    public Task<PositionCard> AddPosition(double latitude, double longitude, string devEuiCard);
    public Task<PositionCard> GetLastPosition(string devEuiCard);
}