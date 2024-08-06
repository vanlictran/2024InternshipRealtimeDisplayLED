from influxdb_client import InfluxDBClient
from influxdb_client.client.write_api import SYNCHRONOUS

# Configurations
url = "http://localhost:8086"  # Remplacez par l'URL de votre instance InfluxDB
token = "Xip2vb7g32N9seLG5mOSynBzRSZuY2zRjyNhcVMyTi407UkamsXkepFZAUP-EXdzfQkCiVYns9m2zQyuU-zBkw=="
org = "stationBus"
bucket = "bucketStation"

# Initialize the InfluxDB client
client = InfluxDBClient(url=url, token=token, org=org)

# Function to query InfluxDB
def query_station_info(station):
    query = f'''
    from(bucket: "{bucket}")
      |> range(start: -inf)
      |> filter(fn: (r) => r._measurement == "bus_stations_infosRetour")
      |> filter(fn: (r) => r.station_name == "{station}")
    '''
    
    query_api = client.query_api()
    tables = query_api.query(query)
    
    results = []
    for table in tables:
        for record in table.records:
            if record.values.get('_field') == 'latitude' :
                print(record.values.get('_value'))
            results.append((record.get_time(), record.get_value()))
    
    return results

# Example usage
#station = "Đối diện 136"
station = "Số 122"
results = query_station_info(station)

# Print the results
print(results)

# Close the client
client.close()