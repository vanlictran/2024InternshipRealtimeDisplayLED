using System.Collections.Concurrent;
using api_csharp_uplink.Connectors;
using api_csharp_uplink.Connectors.ExternalEntities;
using api_csharp_uplink.DirException;
using api_csharp_uplink.Entities;
using api_csharp_uplink.Interface;
using Microsoft.Extensions.Caching.Memory;

namespace api_csharp_uplink.Composant;

public class GraphComposant(IGraphHelper graphHelperService, int timeCache=300) : IGraphPosition, IGraphItinerary
{
    private readonly MemoryCache _cache = new(new MemoryCacheOptions());
    private readonly ConcurrentDictionary<LineOrientation, LinkedList<Connexion>> _graph = new();
    private TimeStationCard? _oneStation;
    
    public async Task<int> RegisterPositionCard(Card card, Position position)
    {
        _cache.TryGetValue(card.DevEuiCard, out CardNearStation? value);
        
        if (value == null)
        {
            CardNearStation cardNewPosition = new(position);
            _cache.Set(card.DevEuiCard, cardNewPosition, TimeSpan.FromMinutes(timeCache));
        }
        else
        {
            _cache.Remove(card.DevEuiCard);
            _graph.TryGetValue(new LineOrientation(card.LineBus, value.Orientation), out LinkedList<Connexion>? connexions);
            
            (Station? station1, Station? station2) = GetMoreClosestStation(position, connexions);
            
            if (value.Time == -1)
                value = GetOrientation(value, station1, station2, position, connexions);
            
            if (connexions != null && connexions.Last?.Value.stationCurrent.NameStation == station1?.NameStation)
                value = new CardNearStation(position, 
                    value.Orientation == Orientation.FORWARD? Orientation.BACKWARD : 
                        Orientation.FORWARD, value.Time, value.Distance);
            else
                value = new CardNearStation(position, value.Orientation, value.Time, value.Distance);
            
            TimeDistance timeDistance = await graphHelperService.GetTimeAndDistance(value.Position, position);

            value = new CardNearStation(position, value.Orientation, timeDistance.Time, timeDistance.Distance);
            _cache.Set(card.DevEuiCard, value, TimeSpan.FromMinutes(timeCache));
            
            return timeDistance.Time;
        }

        return timeCache;
    }

    public async Task<int> RegisterPositionOneStation(Position positionCard)
    {
        Console.WriteLine(_oneStation == null);
        if (_oneStation == null)
            return -1;
        
        Console.WriteLine(_oneStation.Station.Position);
        TimeDistance timeDistance = await graphHelperService.GetTimeAndDistance(_oneStation.Station.Position, positionCard);
        Console.WriteLine(timeDistance);
        _oneStation = new TimeStationCard(_oneStation.Station, _oneStation.DevEui, timeDistance.Time);
        
        return timeDistance.Time;

    }

    private CardNearStation GetOrientation(CardNearStation lastPositionCard, Station? stationNearest1,
        Station? stationNearest2, Position positionActual, LinkedList<Connexion>? connexionsForward)
    {
        
        (Station? lastStationNearest1, Station? lastStationNearest2) = 
            GetMoreClosestStation(lastPositionCard.Position, connexionsForward);
        
        string stationName1 = stationNearest1?.NameStation ?? "";
        string stationName2 = stationNearest2?.NameStation ?? "";
        string lastStationName1 = lastStationNearest1?.NameStation ?? "";
        string lastStationName2 = lastStationNearest2?.NameStation ?? "";
        
        LinkedListNode<Connexion>? node = connexionsForward?.First;
        
        while (node != null) {
            if (node.Value.stationCurrent.NameStation == stationName1 || node.Value.stationCurrent.NameStation == stationName2)
                return new CardNearStation(positionActual, Orientation.BACKWARD);
            
            if (node.Value.stationCurrent.NameStation == lastStationName1 || node.Value.stationCurrent.NameStation == lastStationName2)
                return new CardNearStation(positionActual);
            
            node = node.Next;
        }

        return new CardNearStation(positionActual);
    }

    public (Station? station1, Station? station2) GetMoreClosestStation(Position positionCard, LinkedList<Connexion>? connexions)
    {
        if (connexions == null)
            throw new NotFoundException("The line is not found");
        
        if (connexions.Count == 1)
            return (null, connexions.First?.Value.stationCurrent);
        
        List<double> distances = GetDistanceBetweenStation(positionCard, connexions);
        return GetClosestStation(distances, connexions);
    }
    
    private List<double> GetDistanceBetweenStation(Position positionCard, LinkedList<Connexion> connexions)
    {
        LinkedListNode<Connexion>? linkedConnexions = connexions.First;
        List<double> distances = [];
        
        while (linkedConnexions != null)
        {
            Station station = linkedConnexions.Value.stationCurrent;
            double distance = GraphHelperService.DistanceHaversine(positionCard, station.Position);
            distances.Add(distance);
            linkedConnexions = linkedConnexions.Next;
        }

        return distances;
    }
    
    private (Station? station1, Station? station2) GetClosestStation(List<double> distances, LinkedList<Connexion> connexions)
    {
        if (connexions.First == null || connexions.Last == null)
            return (null, null);
        
        double minDistance = Double.MaxValue;
        int indexMin = -1;
        
        for (int i = 0; i < distances.Count; i++)
        {
            if (distances[i] >= minDistance) 
                continue;
            
            minDistance = distances[i];
            indexMin = i;
        }

        if (indexMin == 0)
            return distances[1] > connexions.First.Value.distanceToNextStation
                ? (null, connexions.First.Value.stationCurrent)
                : (connexions.First.Value.stationCurrent, connexions.First.Next?.Value.stationCurrent);
        if (indexMin == connexions.Count - 1)
            return distances[connexions.Count - 2] > connexions.Last.Previous?.Value.distanceToNextStation
                ? (connexions.Last.Value.stationCurrent, null)
                : (connexions.Last.Previous?.Value.stationCurrent, connexions.Last.Value.stationCurrent);
        
        return GetClosestStationWith2StationMin(indexMin, distances, connexions);
    }
    
    private (Station? station1, Station? station2) GetClosestStationWith2StationMin(int indexMin, List<double> distances, 
        LinkedList<Connexion> connexions)
    {
        int indexCurrent = 0;
        LinkedListNode<Connexion>? node = connexions.First;
        
        while (node != null)
        {
            if (indexCurrent == indexMin - 1)
            {
                double distanceLastStation = distances[indexMin - 1] - node.Value.distanceToNextStation;
                double distanceNextStation = distances[indexMin + 1] - node.Next?.Value.distanceToNextStation ?? 0;
                
                if (distanceLastStation < distanceNextStation)
                    return (node.Value.stationCurrent, node.Next?.Value.stationCurrent);
                
                return (node.Next?.Value.stationCurrent, node.Next?.Next?.Value.stationCurrent);
            }

            indexCurrent++;
            node = node.Next;
        }
        
        return (null, null);
    }

    public Task RegisterItineraryCard(Itinerary itinerary)
    {
        LinkedList<Connexion> connexions = new(itinerary.connexions);
        LineOrientation lineOrientation = new(itinerary.lineNumber, itinerary.orientation);
        _graph.TryAdd(lineOrientation, connexions);
        return Task.CompletedTask;
    }

    public Task<int> GetItineraryTime(int lineNumber,string nameStation1, string nameStation2)
    {
        LineOrientation lineOrientationForward = new(lineNumber, Orientation.FORWARD);
        LineOrientation lineOrientationBackward = new(lineNumber, Orientation.BACKWARD);
        
        bool existForward = _graph.TryGetValue(lineOrientationForward, out LinkedList<Connexion>? connexionsForward);
        bool existBackward = _graph.TryGetValue(lineOrientationBackward, out LinkedList<Connexion>? connexionsBackward);
        
        if (!existForward && !existBackward)
            throw new NotFoundException($"The line with lineNumber {lineNumber} not found");

        int[] indexesForward = GetIndexesStation(connexionsForward, nameStation1, nameStation2);
        int[] indexesBackward = GetIndexesStation(connexionsBackward, nameStation1, nameStation2);
        int time = -1;

        if (indexesForward[0] != -1 && indexesForward[1] != -1 && indexesBackward[0] != -1 && indexesBackward[1] != -1)
            time = indexesForward[0] < indexesForward[1] ? GetTimeBetweenStations(connexionsForward, indexesForward[0], indexesForward[1]) 
                : GetTimeBetweenStations(connexionsForward, indexesBackward[0], indexesBackward[1]);
        
        if (indexesForward[0] != -1 && indexesForward[1] != -1)
            time = indexesForward[0] < indexesForward[1] ? GetTimeBetweenStations(connexionsForward, indexesForward[0], indexesForward[1]) 
                : GetTimeBetweenStations(connexionsForward, indexesForward[1], indexesForward[0]);
        
        if (indexesBackward[0] != -1 && indexesBackward[1] != -1)
            time = indexesBackward[0] < indexesBackward[1] ? GetTimeBetweenStations(connexionsBackward, indexesBackward[0], indexesBackward[1]) 
                : GetTimeBetweenStations(connexionsBackward, indexesBackward[1], indexesBackward[0]);
            
        return Task.FromResult(time);
    }

    public static int[] GetIndexesStation(LinkedList<Connexion>? connexions, string nameStation1, string nameStation2)
    {
        if (connexions == null)
            return [-1, -1];
        
        LinkedListNode<Connexion>? linkedConnexions = connexions.First;
        int indexOne = -1;
        int indexTwo = -1;
        int indexCurrent = 0;
        
        while (linkedConnexions != null)
        {
            Station station = linkedConnexions.Value.stationCurrent;

            if (station.NameStation == nameStation1)
                indexOne = indexCurrent;
            else if (station.NameStation == nameStation2)
                indexTwo = indexCurrent;
            
            linkedConnexions = linkedConnexions.Next;
            indexCurrent += 1;
        }

        return [indexOne, indexTwo];
    }

    public Task RemoveItineraryCard(int lineNumber, Orientation orientation)
    {
        LineOrientation lineOrientation = new(lineNumber, orientation);
        
        if (!_graph.TryRemove(lineOrientation, out _))
            throw new NotFoundException($"The line with lineNumber {lineNumber} and orientation {orientation} not found");
        
        return Task.CompletedTask;
    }

    public Task<Station> AddStationGraph(Station station)
    {
        _oneStation = new TimeStationCard(station);
        Console.WriteLine(_oneStation != null);
        return Task.FromResult(station);
    }

    public Task<int> GetTimeStation(string nameStation)
    {
        if (nameStation == _oneStation?.Station.NameStation)
            return Task.FromResult(_oneStation.Time);
        return Task.FromResult(-1);
    }

    public static int GetTimeBetweenStations(LinkedList<Connexion>? connexions, int indexStation1, int indexStation2)
    {
        if (connexions == null || connexions.Count <= indexStation2)
            return -1;
        
        LinkedListNode<Connexion>? linkedConnexions = connexions.First;
        int indexCurrent = 0;
        int time = 0;
        
        while (linkedConnexions != null && indexCurrent < indexStation2)
        {
            if(indexCurrent >= indexStation1 && indexCurrent < indexStation2)
                time += linkedConnexions.Value.timeToNextStation;
            
            indexCurrent += 1;
            linkedConnexions = linkedConnexions.Next;
        }

        return time;
    }
}

public class TimeStationCard(Station station, string devEui = "", int time = -1)
{
    public Station Station { get; } = station;
    public string DevEui { get; set; } = devEui;
    public int Time { get; set; } = time;
}