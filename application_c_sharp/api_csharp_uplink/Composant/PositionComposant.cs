using api_csharp_uplink.DirException;
using api_csharp_uplink.Entities;
using api_csharp_uplink.Interface;

namespace api_csharp_uplink.Composant;

public class PositionComposant(IPositionRepository positionRepository) : IPositionRegister
{
    public async Task<PositionCard> AddPosition(double latitude, double longitude, string devEuiCard)
    {
        if (latitude < -90 || latitude > 90 || longitude < -180 || longitude > 180)
            throw new ValueNotCorrectException("Latitude or longitude is not correct");
        
        PositionCard positionCard = new(new Position(latitude, longitude), devEuiCard);
        return await positionRepository.Add(positionCard);
    }

    public async Task<PositionCard> GetLastPosition(string devEuiCard)
    {
        return await positionRepository.GetLast(devEuiCard) ?? throw new NotFoundException($"Not Position for this devEui of the Card: {devEuiCard}");
    }
}