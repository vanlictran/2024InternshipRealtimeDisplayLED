using System.Globalization;
using System.Text;
using api_csharp_uplink.Connectors.ExternalEntities;
using api_csharp_uplink.DirException;
using api_csharp_uplink.Entities;
using api_csharp_uplink.Interface;
using api_csharp_uplink.Settings;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace api_csharp_uplink.Connectors;

public class GraphHelperService(IOptions<GraphHopperSettings> graphHopperSettings) : IGraphHelper
{
    private readonly HttpClient _client = new();
    private readonly string _request = graphHopperSettings.Value.Url + "?key=" + graphHopperSettings.Value.Token;

    private static string ConvertPositionToRequestGraphHopper(Position position1, Position position2)
    {
        string[][] points =
        [
            [
                position1.Longitude.ToString(CultureInfo.CurrentCulture),
                position1.Latitude.ToString(CultureInfo.CurrentCulture)
            ],
            [
                position2.Longitude.ToString(CultureInfo.CurrentCulture),
                position2.Latitude.ToString(CultureInfo.CurrentCulture)
            ]
        ];
        
        var jsonToConvert = new
        {
            profile = "car",
            points,
            snap_preventions = new[]
            {
                "ferry"
            }
        };
        
        return JsonConvert.SerializeObject(jsonToConvert);
    }
    
    
    public async Task<TimeDistance> GetTimeAndDistance(Position position1, Position position2)
    {
        try
        {
            double distance = DistanceHaversine(position1, position2);
            
            if (!graphHopperSettings.Value.Use) 
                return new TimeDistance(5, distance);
            
            string json = ConvertPositionToRequestGraphHopper(position1, position2);
            StringContent content = new(json, Encoding.UTF8, "application/json");
            Console.WriteLine(_request);
            HttpResponseMessage response = await _client.PostAsync(_request, content);
            string responseContent = await response.Content.ReadAsStringAsync();

            var jsonResult = JsonConvert.DeserializeObject<dynamic>(responseContent);
            if (jsonResult == null)
                throw new RequestExternalServiceException("Error in the request to the external service");

            int time = jsonResult["paths"][0]["time"];
            return new TimeDistance(time < 1000? 1 : time / 1000, distance);
        }
        catch (Exception e)
        {
            throw new RequestExternalServiceException("Error in the request to the external service : " + e.Message);
        }
    }

    public static double DistanceHaversine(Position position1, Position position2)
    {
        double rayonTerre = 6371.0;
        double piRadian = Math.PI / 180;
        
        double lat1Rad = position1.Latitude * piRadian;
        double lon1Rad = position1.Longitude * piRadian;
        double lat2Rad = position2.Latitude * piRadian;
        double lon2Rad = position2.Longitude * piRadian;

        // Différences des latitudes et longitudes
        double deltaLat = lat2Rad - lat1Rad;
        double deltaLon = lon2Rad - lon1Rad;

        // Formule de Haversine
        double a = Math.Sin(deltaLat / 2) * Math.Sin(deltaLat / 2) + Math.Cos(lat1Rad) * Math.Cos(lat2Rad) 
            * Math.Sin(deltaLon / 2) * Math.Sin(deltaLon / 2);
        double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        
        return rayonTerre * c;
    }
}