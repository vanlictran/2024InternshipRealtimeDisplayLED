using api_csharp_uplink.DirException;
using api_csharp_uplink.Entities;
using api_csharp_uplink.Interface;

namespace api_csharp_uplink.Composant;

public class TimeComposant(IGraphPosition graphPosition, IGraphItinerary graphItinerary, ICardFinder cardFinder) 
    : IPositionProcessor, ITimeProcessor
{
    public async Task<int> RegisterPositionCard(PositionCard positionCard)
    {
        try
        {
            Card card = await cardFinder.GetCardByDevEuiCard(positionCard.DevEuiCard);
            return await graphPosition.RegisterPositionCard(card, positionCard.Position);
        }
        catch (NotFoundException)
        {
            return -1;
        }
    }

    public async Task<int> RegisterPositionOneStation(Position position)
    {
        Console.WriteLine("Register position one station");
        return await graphPosition.RegisterPositionOneStation(position);
    }
    
    public async Task<int> GetTimeToNextStation(string nameStation)
    {
        if (string.IsNullOrEmpty(nameStation))
            throw new ArgumentNullException(nameof(nameStation), "Name of station must not be null or empty.");
        
        int time = await graphItinerary.GetTimeStation(nameStation);
        if (time == -1)
            throw new NotFoundException("Station not found or position not registered.");
        
        return await graphItinerary.GetTimeStation(nameStation);
    }

    public async Task<int> GetTimeBetweenStations(int lineNumber, string nameStation1, string nameStation2)
    {
        if (string.IsNullOrEmpty(nameStation1) || string.IsNullOrEmpty(nameStation2))
            throw new ArgumentNullException(nameof(nameStation1), "Name of station must not be null or empty.");
        
        if (lineNumber < 1)
            throw new ArgumentOutOfRangeException(nameof(lineNumber), "Line number must be positive.");
        
        if (nameStation1 == nameStation2)
            return 0;
        
        int time = await graphItinerary.GetItineraryTime(lineNumber, nameStation1, nameStation2);
        return time == -1 ? throw new NotFoundException("Relation between stations not found.") : time;
    }
}